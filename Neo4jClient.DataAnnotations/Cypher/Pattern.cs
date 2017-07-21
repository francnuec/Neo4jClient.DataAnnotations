using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Neo4jClient.Cypher;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Expressions;
using Newtonsoft.Json.Linq;
using Neo4jClient.Serialization;
using Newtonsoft.Json;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public class Pattern : Annotated, IPattern
    {
        public Pattern(ICypherFluentQuery query, IPathExtent path)
            : base(query)
        {
            Path = path;
        }

        private EntityResolver entityResolver;
        private EntityConverter entityConverter;
        private ISerializer serializer;
        private Func<object, string> actualSerializer;
        private IGraphClient client;

        public IPathExtent Path { get; protected internal set; }

        private PatternBuildStrategy? buildStrategy;
        public PatternBuildStrategy BuildStrategy
        {
            get
            {
                return buildStrategy ?? 
                    Path?.Builder?.PatternBuildStrategy ?? 
                    PatternBuildStrategy.NoParams;
            }
            set
            {
                buildStrategy = value;
            }
        }

        internal string aParam, rParam, bParam;
        private bool aParamIsAuto, rParamIsAuto, bParamIsAuto;

        public string AParameter
        {
            get
            {
                return aParam ?? 
                    (aParam = (abSelector ?? arSelector)?.Parameters[0].Name) ?? 
                    GetParameter("A", ref aParam, ref aParamIsAuto);
            }

            protected internal set
            {
                if (value != aParam)
                    aParamIsAuto = false;

                aParam = !string.IsNullOrWhiteSpace(value) ? value : null;
            }
        }

        public string RParameter
        {
            get
            {
                return rParam ?? 
                    (rParam = rbSelector?.Parameters[0].Name) ?? 
                    GetParameter("R", ref rParam, ref rParamIsAuto);
            }

            protected internal set
            {
                if (value != rParam)
                    rParamIsAuto = false;

                rParam = !string.IsNullOrWhiteSpace(value) ? value : null;
            }
        }

        public string BParameter
        {
            get
            {
                return bParam ?? GetParameter("B", ref bParam, ref bParamIsAuto);
            }

            protected internal set
            {
                if (value != bParam)
                    bParamIsAuto = false;

                bParam = !string.IsNullOrWhiteSpace(value) ? value : null;
            }
        }


        public bool AParamIsAuto { get => AParameter != null && aParamIsAuto; }

        public bool RParamIsAuto { get => RParameter != null && rParamIsAuto; }

        public bool BParamIsAuto { get => BParameter != null && bParamIsAuto; }


        public bool HasAType { get { return AType != null && AType != Defaults.CypherObjectType; } }

        public bool HasRType { get { return RType != null && RType != Defaults.CypherObjectType; } }

        public bool HasBType { get { return BType != null && BType != Defaults.CypherObjectType; } }


        internal Type aType, rType, bType;
        internal bool aTypeSet, rTypeSet, bTypeSet;

        public Type AType
        {
            get
            {
                if (!aTypeSet)
                {
                    aTypeSet = true; //set this first to avoid a loop in the HasxType calls

                    if ((aType == null || aType == Defaults.CypherObjectType)
                        && HasRType && HasBType)
                    {
                        //infer A from R and B.
                        var direction = dir;
                        var type = GetOtherNodeFromRAndKnownNode(RType, BType, findingA: true, direction: ref direction);
                        aType = type ?? aType;
                    }
                }

                return aType;
            }
            protected internal set
            {
                aType = value;
                aTypeSet = false;
            }
        }

        public Type RType
        {
            get
            {
                if (!rTypeSet)
                {
                    rTypeSet = true;

                    if ((rType == null || rType == Defaults.CypherObjectType)
                        && abSelector == null //if we already have a selector describing a predefined relationship, finding R is futile.
                        && HasAType && HasBType)
                    {
                        //infer R from A and B.
                        var direction = dir;
                        var prop = GetR(ATypeInfo, BTypeInfo, findingA: false, dir: ref direction);

                        if (prop == null || prop.Item2 == null)
                        {
                            //switch them and try again
                            var direction2 = dir;
                            prop = GetR(BTypeInfo, ATypeInfo, findingA: true, dir: ref direction2);
                        }

                        rType = prop?.Item2;
                    }
                }

                return rType;
            }
            protected internal set
            {
                rType = value;
                rTypeSet = false;
            }
        }

        public Type BType
        {
            get
            {
                if (!bTypeSet)
                {
                    bTypeSet = true;

                    if ((bType == null || bType == Defaults.CypherObjectType)
                        && HasAType && HasRType)
                    {
                        //infer B from A and R.
                        var direction = dir;
                        var type = GetOtherNodeFromRAndKnownNode(RType, AType, findingA: false, direction: ref direction);
                        bType = type ?? bType;
                    }
                }

                return bType;
            }
            protected internal set
            {
                bType = value;
                bTypeSet = false;
            }
        }


        private LambdaExpression abSelector, arSelector, rbSelector;
        private bool abSelSet, arSelSet, rbSelSet;

        public LambdaExpression ABSelector
        {
            get
            {
                if (!abSelSet)
                {
                    abSelSet = true;

                    //if ABSel is not null, abort.
                    //if ARSel or RBSel is not null, abort
                    //if R is not null or CypherObject, abort.
                    //if A or B is CypherObject, abort.
                    if (abSelector == null
                        && arSelector == null
                        && rbSelector == null
                        && !HasRType
                        && HasAType
                        && HasBType)
                    {
                        //if all checks out, then continue
                        //try to find the nav prop between A and B types from their classes.
                        var navigationProp = GetNavigationPropertyBetweenTypes(ATypeInfo, BTypeInfo,
                            out var inverseProp, assertColumnAttrHasName: true);

                        if (navigationProp != null)
                        {
                            //set navprops
                            ANavProperty = navigationProp;
                            BNavProperty = inverseProp ?? bNavProp;

                            //create expression
                            abSelector = CreateNavigationPropertySelector(AType, navigationProp, AParameter);
                        }
                    }
                }

                return abSelector;
            }
            protected internal set
            {
                abSelector = value;
                abSelSet = value != null;
            }
        }

        public LambdaExpression ARSelector
        {
            get
            {
                if (!arSelSet)
                {
                    arSelSet = true;

                    //if ABSel is not null, abort.
                    //if ARSel is not null, abort
                    //if A or R is CypherObject, abort.
                    if (abSelector == null
                        && arSelector == null
                        && HasAType
                        && HasRType)
                    {
                        //if all checks out, then continue

                        //try to find the nav prop between A and R types from their classes.
                        var navigationProperty = GetNavigationPropertyBetweenTypes(ATypeInfo, RTypeInfo,
                            out var inverseProperty,
                            assertColumnAttrHasName: false); //column with name is not necessary in this case

                        if (navigationProperty != null)
                        {
                            //set navprops
                            ANavProperty = navigationProperty;

                            //create expression
                            arSelector = CreateNavigationPropertySelector(AType, navigationProperty, AParameter);
                        }
                    }
                }

                return arSelector;
            }
            protected internal set
            {
                arSelector = value;
                arSelSet = value != null;
            }
        }

        public LambdaExpression RBSelector
        {
            get
            {
                if (!rbSelSet)
                {
                    rbSelSet = true;

                    //if ABSel is not null, abort.
                    //if RBSel is not null, abort
                    //if R or B is CypherObject, abort.
                    if (abSelector == null
                        && rbSelector == null
                        && HasRType
                        && HasBType)
                    {
                        //if all checks out, then continue

                        //try to find the nav prop between R and B types from their classes.
                        var navigationProp = GetNavigationPropertyBetweenTypes(RTypeInfo, BTypeInfo,
                            out var inverseProp, assertColumnAttrHasName: true);

                        if (navigationProp != null)
                        {
                            //set navprops
                            BNavProperty = inverseProp ?? bNavProp;
                            
                            //create expression
                            rbSelector = CreateNavigationPropertySelector(RType, navigationProp, RParameter);
                        }
                    }
                }

                return rbSelector;
            }
            protected internal set
            {
                rbSelector = value;
                rbSelSet = value != null;
            }
        }


        public Tuple<int?, int?> RHops { get; protected internal set; }


        private List<string> aLabels, rTypes, bLabels;
        protected bool aLabelsSet, rTypesSet, bLabelsSet;

        public IEnumerable<string> ALabels
        {
            get
            {
                if (!aLabelsSet)
                {
                    /*
                     * NODE LABELS
                     * 1. All Table Attribute Names in the class inheritance heirarchy starting from the class itself to all base classes.
                     * 2. If no Table Attributes found in class hierarchy, the class name is used instead.
                     * 3. Then labels specified by user are appended to the above.
                     *              
                     */

                    if (aLabels == null)
                    {
                        aLabels = new List<string>();
                    }

                    aLabelsSet = true;

                    if (!UseGivenALabelsOnly && AType != Defaults.CypherObjectType)
                    {
                        aLabels.AddRange(ATypeInfo.LabelsWithTypeNameCatch);
                    }

                    aLabels = aLabels.Distinct().Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
                }

                return aLabels;
            }
            protected internal set
            {
                aLabels = value?.ToList();
                aLabelsSet = false;
            }
        }

        public IEnumerable<string> RTypes
        {
            get
            {
                /*
                * RELATIONSHIP TYPE
                * 1. Column Attribute Name on only one side of the relationship.
                * 2. ForeignKey Attribute Name on only one side of the relationship that doesn't point to any property on the class.
                * 3. First Table Attribute Name on the relationship class inheritance hierarchy starting from the class itself to all base classes.
                * 4. Name of navigation property with direction considered, and the class name of the property type provided it's a relationship class.
                */

                if (!rTypesSet)
                {
                    if (rTypes == null)
                    {
                        rTypes = new List<string>();
                    }

                    rTypesSet = true;

                    ResolveRTypes(rTypes);

                    rTypes = rTypes.Distinct().Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
                }

                return rTypes;
            }
            protected internal set
            {
                rTypes = value?.ToList();
                rTypesSet = false;
            }
        }

        public IEnumerable<string> BLabels
        {
            get
            {
                if (!bLabelsSet)
                {
                    if (bLabels == null)
                    {
                        bLabels = new List<string>();
                    }

                    bLabelsSet = true;

                    if (!UseGivenBLabelsOnly && BType != Defaults.CypherObjectType)
                    {
                        bLabels.AddRange(BTypeInfo.LabelsWithTypeNameCatch);
                    }

                    bLabels = bLabels.Distinct().Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
                }

                return bLabels;
            }
            protected internal set
            {
                bLabels = value?.ToList();
                bLabelsSet = false;
            }
        }


        public bool UseGivenALabelsOnly { get; protected internal set; }
        public bool UseGivenRTypesOnly { get; protected internal set; }
        public bool UseGivenBLabelsOnly { get; protected internal set; }


        private LambdaExpression aProps, rProps, bProps;
        public LambdaExpression AProperties
        {
            get { return aProps; }
            protected internal set
            {
                aProps = value;
                aFinalPropsSet = false;
            }
        }

        public LambdaExpression RProperties
        {
            get { return rProps; }
            protected internal set
            {
                rProps = value;
                rFinalPropsSet = false;
            }
        }

        public LambdaExpression BProperties
        {
            get { return bProps; }
            protected internal set
            {
                bProps = value;
                bFinalPropsSet = false;
            }
        }

        private LambdaExpression aConstrs, rConstrs, bConstrs;
        public LambdaExpression AConstraints
        {
            get { return aConstrs; }
            protected internal set
            {
                aConstrs = value;
                aFinalPropsSet = false;
            }
        }

        public LambdaExpression RConstraints
        {
            get { return rConstrs; }
            protected internal set
            {
                rConstrs = value;
                rFinalPropsSet = false;
            }
        }

        public LambdaExpression BConstraints
        {
            get { return bConstrs; }
            protected internal set
            {
                bConstrs = value;
                bFinalPropsSet = false;
            }
        }

        private RelationshipDirection? dir;
        private bool dirSet, isAutoDirection;
        public RelationshipDirection? Direction
        {
            get
            {
                if (!dirSet)
                {
                    dirSet = true;

                    //1. Direction as specified by user.
                    if (dir == null)
                    {
                        dir = GetDirection(this);
                    }

                    dir = dir != RelationshipDirection.Automatic ? dir : null;
                }

                return !isAutoDirection ? dir : RelationshipDirection.Automatic;
            }
            protected internal set
            {
                //avoid the automatic direction internally. we should only use that for the final output instead.
                isAutoDirection = value == RelationshipDirection.Automatic;
                dir = !isAutoDirection ? value : null;
                dirSet = dir != null;
            }
        }


        public bool IsExtension { get; protected internal set; }


        private JObject aFinalProps, rFinalProps, bFinalProps;
        private bool aFinalPropsSet, rFinalPropsSet, bFinalPropsSet;
        /// <summary>
        /// This would contain the properties as they would be written to cypher.
        /// Parameters would be a <see cref="JRaw"/> value here, and not <see cref="JValue"/> string.
        /// </summary>
        public JObject AFinalProperties
        {
            get
            {
                if (!aFinalPropsSet)
                {
                    aFinalPropsSet = true;
                    aFinalProps = GetFinalProperties(this, AProperties, AConstraints, AType);
                }

                return aFinalProps;
            }
        }

        public JObject RFinalProperties
        {
            get
            {
                if (!rFinalPropsSet)
                {
                    rFinalPropsSet = true;
                    rFinalProps = GetFinalProperties(this, RProperties, RConstraints, RType);
                }

                return rFinalProps;
            }
        }

        public JObject BFinalProperties
        {
            get
            {
                if (!bFinalPropsSet)
                {
                    bFinalPropsSet = true;
                    bFinalProps = GetFinalProperties(this, BProperties, BConstraints, BType);
                }

                return bFinalProps;
            }
        }


        private EntityTypeInfo aInfo, rInfo, bInfo;

        public EntityTypeInfo ATypeInfo
        {
            get { return aInfo ?? (aInfo = Neo4jAnnotations.GetEntityTypeInfo(AType)); }
        }

        public EntityTypeInfo RTypeInfo
        {
            get { return rInfo ?? (rInfo = Neo4jAnnotations.GetEntityTypeInfo(RType)); }
        }

        public EntityTypeInfo BTypeInfo
        {
            get { return bInfo ?? (bInfo = Neo4jAnnotations.GetEntityTypeInfo(BType)); }
        }


        private PropertyInfo aNavProp, bNavProp;
        private bool aNavPropSet, bNavPropSet;

        public PropertyInfo ANavProperty
        {
            get
            {
                if (!aNavPropSet)
                {
                    aNavPropSet = true;

                    //calling the selectors forces their methods to run, and navprops to be set if found.
                    var sel = ABSelector ?? ARSelector;

                    if (aNavProp == null && sel != null)
                    {
                        //still null
                        //maybe the selectors were set manually, so find nav here from selected property.
                        aNavProp = Utilities.GetPropertyInfo(sel, AType, null);
                    }
                }

                return aNavProp;
            }
            set
            {
                aNavProp = value;
                aNavPropSet = value != null;
            }
        }

        public PropertyInfo BNavProperty
        {
            get
            {
                if (!bNavPropSet)
                {
                    bNavPropSet = true;

                    //calling the selectors forces their methods to run, and navprops to be set if found.
                    var sel = ABSelector ?? RBSelector;

                    if (bNavProp == null && sel != null)
                    {
                        //still null
                        //maybe the selectors were set manually, so find nav here.

                        var otherType = sel.Parameters[0].Type;
                        var otherTypeInfo = Neo4jAnnotations.GetEntityTypeInfo(otherType);
                        var selectedProp = Utilities.GetPropertyInfo(sel, otherType, BType);

                        var navigationProp =
                            GetNavigationPropertyBetweenTypes(otherTypeInfo, BTypeInfo,
                            out var inverseProp, assertColumnAttrHasName: true);

                        if (navigationProp != null && inverseProp != null //our interest is in both properties here
                            && navigationProp == selectedProp //these must match
                            )
                        {
                            //we have a winner
                            bNavProp = inverseProp;
                        }
                    }
                }

                return bNavProp;
            }
            set
            {
                bNavProp = value;
                bNavPropSet = value != null;
            }
        }


        public override string Build()
        {
            //CHECK LIST
            //Parameters
            //Labels
            //Hops
            //FinalProperties
            //Direction

            StringBuilder builder = new StringBuilder();

            string A = null, R = null, B = null;

            if (!IsExtension)
            {
                //Build A
                A = BuildNode(this, aParam, aParamIsAuto, AParameter, ALabels, AFinalProperties);

                builder.Append(A);
            }

            //Build R
            R = BuildRelationship(this, rParam, rParamIsAuto, RParameter, RTypes, RHops, RFinalProperties);
            if (R == "[]")
                R = null; //just omit the empty ones

            //Build B
            B = BuildNode(this, bParam, bParamIsAuto, BParameter, BLabels, BFinalProperties);

            if (IsExtension || R != null || HasBType || (B != null && B != "()"))
            {
                if (Direction == RelationshipDirection.Incoming)
                {
                    builder.Append("<");
                }

                builder.Append("-");

                if (R != null)
                {
                    builder.Append(R);
                }

                builder.Append("-");

                if (Direction == RelationshipDirection.Outgoing)
                {
                    builder.Append(">");
                }

                builder.Append(B);
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            try
            {
                return Build();
            }
            catch
            {

            }

            return base.ToString();
        }


        private void ResolveRTypes(List<string> rTypes)
        {
            if (!UseGivenRTypesOnly && RType != Defaults.CypherObjectType)
            {
                //RELATIONSHIP TYPE(declaring class takes precedence over relationship class)
                //1. Column Attribute Name on only one side of the relationship.
                //2. ForeignKey Attribute Name on only one side of the relationship that doesn't point to any property on the class.
                //3. First Table Attribute Name on the relationship class inheritance hierarchy starting from the class itself to all base classes.
                //4. Name of navigation property with direction considered, and/ or the class name of a provided relationship class
                //   (which should also be the same type as the relationship property type).

                var additionalTypes = new List<string>();

                //get the navs.
                var aNavProp = ANavProperty; var bNavProp = BNavProperty;
                if (aNavProp != null || bNavProp != null)
                {
                    //1. Column Attribute Name on only one side of the relationship.
                    Attribute aAttr = aNavProp?.GetCustomAttribute(Defaults.ColumnType);
                    Attribute bAttr = bNavProp?.GetCustomAttribute(Defaults.ColumnType);

                    var attrArray = new[] { aAttr, bAttr };

                    ColumnAttribute columnAttr = attrArray.Where(a => a != null
                    && !string.IsNullOrWhiteSpace(((ColumnAttribute)a).Name))
                    .SingleOrDefault() as ColumnAttribute; //we expect just one attribute to be defined on the relationship. two would create confusion.

                    if (columnAttr != null)
                    {
                        additionalTypes.Add(columnAttr.Name);
                    }

                    //2. ForeignKey Attribute Name on only one side of the relationship that doesn't point to any property on the class.
                    if (additionalTypes.Count == 0)
                    {
                        attrArray[0] = aNavProp != null ? ATypeInfo.ForeignKeyProperties.Where(fk =>
                        fk.NavigationProperty == aNavProp && !fk.IsAttributeAutoCreated && !fk.IsAttributePointingToProperty)
                        .Select(fk => fk.Attribute).FirstOrDefault() : null;

                        attrArray[1] = bNavProp != null ? BTypeInfo.ForeignKeyProperties.Where(fk =>
                        fk.NavigationProperty == bNavProp && !fk.IsAttributeAutoCreated && !fk.IsAttributePointingToProperty)
                        .Select(fk => fk.Attribute).FirstOrDefault() : null;

                        var fkAttr = attrArray.Where(a => a != null).SingleOrDefault() as ForeignKeyAttribute;

                        if (fkAttr != null && !string.IsNullOrWhiteSpace(fkAttr.Name))
                        {
                            additionalTypes.Add(fkAttr.Name);
                        }
                    }
                }

                //3. First Table Attribute Name on the relationship class inheritance hierarchy starting from the class itself to all base classes.
                if (additionalTypes.Count == 0 && HasRType)
                {
                    var label = RTypeInfo.Labels.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(label))
                        additionalTypes.Add(label);
                }

                //last try
                //4. Name of navigation property with direction considered, and/ or the class name of a provided relationship class
                //   (which should also be the same type as the relationship property type).
                if (additionalTypes.Count == 0)
                {
                    var direction = Direction;

                    switch (direction)
                    {
                        case RelationshipDirection.Outgoing:
                            {
                                if (ANavProperty != null)
                                {
                                    additionalTypes.Add(ANavProperty.Name);
                                }
                                break;
                            }
                        case RelationshipDirection.Incoming:
                            {
                                if (BNavProperty != null)
                                {
                                    additionalTypes.Add(BNavProperty.Name);
                                }
                                break;
                            }
                        default:
                            {
                                if (ANavProperty != null || BNavProperty != null)
                                {
                                    additionalTypes.Add((ANavProperty ?? BNavProperty).Name);
                                }
                                break;
                            }
                    }

                    if (HasRType)
                    {
                        additionalTypes.Add(RType.Name);
                    }
                }

                rTypes.AddRange(additionalTypes);
            }
        }

        internal static string BuildNode(Pattern pattern,
            string param, bool paramIsAuto, string Parameter, //NOTE: this call order is important, and method signature should not be reshuffled.
            IEnumerable<string> labels, JObject finalProperties)
        {
            //e.g. (param:labels {finalProperties})

            StringBuilder builder = new StringBuilder();

            if (!paramIsAuto && param != null)
            {
                builder.Append(param);
            }

            if (labels?.Count() > 0)
            {
                builder.Append(":" + labels.Aggregate((first, second) => $"{first}:{second}"));
            }

            if (finalProperties?.Count > 0)
            {
                string value = null;

                switch (pattern.BuildStrategy)
                {
                    case PatternBuildStrategy.WithParams:
                    case PatternBuildStrategy.WithParamsForValues:
                        {
                            pattern.InternalCypherQuery.WithParam(Parameter, finalProperties);

                            value = Parameter;

                            if (pattern.BuildStrategy == PatternBuildStrategy.WithParamsForValues)
                            {
                                value = finalProperties.Properties()
                                    .Select(jp => $"{jp.Name}: {{{Parameter}}}.{jp.Name}")
                                    .Aggregate((first, second) => $"{first}, {second}");
                            }
                            break;
                        }
                    case PatternBuildStrategy.NoParams:
                        {
                            ResolveClientAndSerializers(pattern);

                            value = finalProperties.Properties()
                                    .Select(jp => $"{jp.Name}: {pattern.actualSerializer(jp.Value)}")
                                    .Aggregate((first, second) => $"{first}, {second}");
                            break;
                        }
                }

                if (value != null)
                    builder.Append($" {{ {value} }}");
            }

            return $"({builder.ToString()})";
        }

        internal static string BuildRelationship(Pattern pattern,
            string param, bool paramIsAuto, string Parameter, //NOTE: this call order is important, and method signature should not be reshuffled.
            IEnumerable<string> types, Tuple<int?, int?> hops, JObject finalProperties)
        {
            //e.g. (param:types*hops {finalProperties})

            StringBuilder builder = new StringBuilder();

            if (!paramIsAuto && param != null)
            {
                builder.Append(param);
            }

            if (types?.Count() > 0)
            {
                builder.Append(":" + types.Aggregate((first, second) => $"{first}|{second}"));
            }

            if (hops != null)
            {
                builder.Append("*");

                if (hops.Item1 != null || hops.Item2 != null)
                {
                    if (hops.Item1 == hops.Item2)
                    {
                        builder.Append(hops.Item1);
                    }
                    else
                    {
                        if (hops.Item1 != null)
                        {
                            builder.Append(hops.Item1);
                        }

                        builder.Append("..");

                        if (hops.Item2 != null)
                        {
                            builder.Append(hops.Item2);
                        }
                    }
                }
            }

            if (finalProperties?.Count > 0)
            {
                string value = null;

                switch (pattern.BuildStrategy)
                {
                    case PatternBuildStrategy.WithParams:
                    case PatternBuildStrategy.WithParamsForValues:
                        {
                            pattern.InternalCypherQuery.WithParam(Parameter, finalProperties);

                            value = Parameter;

                            if (pattern.BuildStrategy == PatternBuildStrategy.WithParamsForValues)
                            {
                                value = finalProperties.Properties()
                                    .Select(jp => $"{jp.Name}: {{{Parameter}}}.{jp.Name}")
                                    .Aggregate((first, second) => $"{first}, {second}");
                            }
                            break;
                        }
                    case PatternBuildStrategy.NoParams:
                        {
                            ResolveClientAndSerializers(pattern);

                            value = finalProperties.Properties()
                                    .Select(jp => $"{jp.Name}: {pattern.actualSerializer(jp.Value)}")
                                    .Aggregate((first, second) => $"{first}, {second}");
                            break;
                        }
                }

                if (value != null)
                    builder.Append($" {{ {value} }}");
            }

            return $"[{builder.ToString()}]";
        }

        internal static string GetParameter(string entity, ref string current, ref bool isAutoFlag)
        {
            if (current == null)
            {
                isAutoFlag = true;
                current = GetRandomParameterFor(entity);
            }

            return current;
        }

        internal static List<PropertyInfo> FilterNavProperties
            (EntityTypeInfo typeInfo, Type navPropertyType, bool includeIEnumerableArg = true)
        {
            return FilterProperties(typeInfo.NavigationableProperties, navPropertyType, includeIEnumerableArg);
        }

        internal static List<PropertyInfo> FilterProperties(List<PropertyInfo> properties,
            Type propertyType, bool includeIEnumerableArg = true)
        {
            Type iEnumerableType = typeof(IEnumerable<>);
            var ret = properties.Where(p => p.PropertyType == propertyType
                         || (includeIEnumerableArg && Utilities.GetEnumerableGenericType(p.PropertyType) == propertyType)
                       ).ToList();

            return ret;
        }

        internal static Dictionary<Type, List<PropertyInfo>> SortNavPropsByAttribute(List<PropertyInfo> navProperties)
        {
            var foreignKeyType = Defaults.ForeignKeyType;
            var inversePropType = Defaults.InversePropertyType;
            var columnType = Defaults.ColumnType;
            var keyType = Defaults.KeyType;

            var navPropsDict = new Dictionary<Type, List<PropertyInfo>>()
                        {
                            { inversePropType, new List<PropertyInfo>() },
                            { foreignKeyType, new List<PropertyInfo>() },
                            { columnType, new List<PropertyInfo>() },
                            { keyType, new List<PropertyInfo>() }
                        };

            //sort them out
            foreach (var navProp in navProperties)
            {
                if (navProp.IsDefined(inversePropType)) navPropsDict[inversePropType].Add(navProp);
                if (navProp.IsDefined(foreignKeyType)) navPropsDict[foreignKeyType].Add(navProp);
                if (navProp.IsDefined(columnType)) navPropsDict[columnType].Add(navProp);
                if (navProp.IsDefined(keyType)) navPropsDict[keyType].Add(navProp);
            }

            return navPropsDict;
        }

        internal static Tuple<List<PropertyInfo>, List<ForeignKeyProperty>> MergeNavPropsWithFKs
            (EntityTypeInfo typeInfo,
            List<PropertyInfo> navPropsOfSameType,
            Dictionary<Type, List<PropertyInfo>> navPropsDict, Type attributeToFilter)
        {
            var props = new List<PropertyInfo>(navPropsDict[attributeToFilter]);

            List<ForeignKeyProperty> FKs;
            props = props
                .UnionNavPropsWithFKs(typeInfo, attributeToFilter,
                fk => fk.NavigationProperty != null && navPropsOfSameType.Contains(fk.NavigationProperty),
                out FKs).ToList();

            return new Tuple<List<PropertyInfo>, List<ForeignKeyProperty>>(props, FKs);
        }

        internal static PropertyInfo GetNavPropertyFromInverseProperty(EntityTypeInfo aTypeInfo,
            List<PropertyInfo> aNavProps, Dictionary<Type, List<PropertyInfo>> aNavPropsDict,
            EntityTypeInfo bTypeInfo,
            List<PropertyInfo> bNavProps, Dictionary<Type, List<PropertyInfo>> bNavPropsDict,
            out PropertyInfo inverseProperty,
            bool assertColumnHasName = true)
        {
            //1.Matching inverse properties.
            //2.Inverse property on one side to a matching foreign keyed property.
            //3.Inverse property on one side to a matching columned property.
            //4.Inverse property on one side to a matching keyed property.
            //5.Inverse property on one side to a matching property.

            inverseProperty = null;
            PropertyInfo navigationProp = null, localInverseProp = null;

            //1.Matching inverse properties.
            List<PropertyInfo> aInvProps = aNavPropsDict[Defaults.InversePropertyType],
            bInvProps = bNavPropsDict[Defaults.InversePropertyType];

            if (aInvProps.Count > 0 && bInvProps.Count > 0)
            {
                //find its matching property. the first match wins
                navigationProp = aInvProps
                    .FirstOrDefault(p => bInvProps
                    .Any(bp => p.Name == bp.GetCustomAttribute<InversePropertyAttribute>().Property
                    && bp.Name == p.GetCustomAttribute<InversePropertyAttribute>().Property
                    && (localInverseProp = bp) != null //HACK: of course it won't be null, but for assignment sakes, we do this. Make sure it's last statement so we don't have a false assignment before conditions are run.
                    ));
            }

            //2.Inverse property on one side to a matching foreign keyed property.
            if (navigationProp == null)
            {
                //check AClass inverse prop to BClass foreign keyed property
                if (aInvProps.Count > 0)
                {
                    //this would do quite unnecessary processing. but for the sakes of future refactoring, we do.
                    var bProps = MergeNavPropsWithFKs(bTypeInfo, bNavProps, bNavPropsDict, Defaults.ForeignKeyType);

                    //find its matching property. the first match wins
                    navigationProp = aInvProps
                        .FirstOrDefault(ap => bProps.Item1
                        .Any(bp => bp.Name == ap.GetCustomAttribute<InversePropertyAttribute>().Property
                        && (localInverseProp = bp) != null));
                }

                //check BClass inverse prop to AClass foreign keyed property
                if (navigationProp == null
                    && bInvProps.Count > 0)
                {
                    //this would do quite unnecessary processing. but for the sakes of future refactoring, we do.
                    var aProps = MergeNavPropsWithFKs(aTypeInfo, aNavProps, aNavPropsDict, Defaults.ForeignKeyType);

                    //find its matching property. the first match wins
                    navigationProp = aProps.Item1
                        .FirstOrDefault(ap => bInvProps
                        .Any(bp => ap.Name == bp.GetCustomAttribute<InversePropertyAttribute>().Property
                        && (localInverseProp = bp) != null));
                }
            }

            //3.Inverse property on one side to a matching columned property.
            if (navigationProp == null)
            {
                //check AClass inverse prop to BClass columned property
                if (aInvProps.Count > 0)
                {
                    var bProps = MergeNavPropsWithFKs(bTypeInfo, bNavProps, bNavPropsDict, Defaults.ColumnType);

                    //find its matching property. the first match wins
                    navigationProp = aInvProps
                        .FirstOrDefault(ap => bProps.Item1
                        .Any(bp => bp.Name == ap.GetCustomAttribute<InversePropertyAttribute>().Property
                        && (assertColumnHasName ? (
                        !string.IsNullOrWhiteSpace(
                            (bp.GetCustomAttribute<ColumnAttribute>() ?? //if the property itself does not have the column attr, it means it is on a scalar property of aforeign key
                            bProps.Item2.First(fk => fk.NavigationProperty == bp)
                            .ScalarProperty.GetCustomAttribute<ColumnAttribute>()).Name)
                        ) : true)
                        && (localInverseProp = bp) != null));
                }

                //check BClass inverse prop to AClass columned property
                if (navigationProp == null
                    && bInvProps.Count > 0)
                {
                    var aProps = MergeNavPropsWithFKs(aTypeInfo, aNavProps, aNavPropsDict, Defaults.ColumnType);

                    //find its matching property. the first match wins
                    navigationProp = aProps.Item1
                        .FirstOrDefault(ap => bInvProps
                        .Any(bp => ap.Name == bp.GetCustomAttribute<InversePropertyAttribute>().Property
                        && (localInverseProp = bp) != null)
                        && (assertColumnHasName ? (
                        !string.IsNullOrWhiteSpace(
                            (ap.GetCustomAttribute<ColumnAttribute>() ?? //if the property itself does not have the column attr, it means it is on a scalar property of aforeign key
                            aProps.Item2.First(fk => fk.NavigationProperty == ap)
                            .ScalarProperty.GetCustomAttribute<ColumnAttribute>()).Name)
                        ) : true));
                }
            }

            //4.Inverse property on one side to a matching keyed property.
            if (navigationProp == null)
            {
                //check AClass inverse prop to BClass keyed property
                if (aInvProps.Count > 0)
                {
                    var bProps = MergeNavPropsWithFKs(bTypeInfo, bNavProps, bNavPropsDict, Defaults.KeyType);

                    //find its matching property. the first match wins
                    navigationProp = aInvProps
                        .FirstOrDefault(ap => bProps.Item1
                        .Any(bp => bp.Name ==
                        ap.GetCustomAttribute<InversePropertyAttribute>().Property
                        && (localInverseProp = bp) != null));
                }

                //check BClass inverse prop to AClass keyed property
                if (navigationProp == null
                    && bInvProps.Count > 0)
                {
                    var aProps = MergeNavPropsWithFKs(aTypeInfo, aNavProps, aNavPropsDict, Defaults.KeyType);

                    //find its matching property. the first match wins
                    navigationProp = aProps.Item1
                        .FirstOrDefault(ap => bInvProps
                        .Any(bp => ap.Name ==
                        bp.GetCustomAttribute<InversePropertyAttribute>().Property
                        && (localInverseProp = bp) != null));
                }
            }

            //5.Inverse property on one side to a matching property.
            if (navigationProp == null)
            {
                //check for a matching property in the other side
                navigationProp = aInvProps
                    .FirstOrDefault(ap => bNavProps
                    .Any(bp => bp.Name == ap.GetCustomAttribute<InversePropertyAttribute>().Property
                    && (localInverseProp = bp) != null));

                navigationProp = navigationProp ?? aNavProps
                    .FirstOrDefault(ap => bInvProps
                    .Any(bp => ap.Name == bp.GetCustomAttribute<InversePropertyAttribute>().Property
                    && (localInverseProp = bp) != null));
            }

            if (navigationProp != null)
            {
                //only assign if we found something
                inverseProperty = localInverseProp;
            }

            return navigationProp;
        }

        internal static PropertyInfo GetNavPropertyFromNonInverseProperty(EntityTypeInfo typeInfo,
            List<PropertyInfo> navProps, Dictionary<Type, List<PropertyInfo>> navPropsDict)
        {
            //Order
            //6.Foreign Key.
            //7.Non-scalar column-attributed property or equivalent collection.
            //8.Non-scalar key-attributed property or equivalent collection.
            //9.First non-scalar property or equivalent collection.

            PropertyInfo navigationProp = null;

            //6.Foreign Key.
            if (navigationProp == null)
            {
                //this would do quite unnecessary processing. but for the sakes of future refactoring, we do.
                var props = MergeNavPropsWithFKs(typeInfo, navProps, navPropsDict, Defaults.ForeignKeyType);
                //first match wins
                navigationProp = props.Item1.FirstOrDefault();
            }

            //7.Non-scalar column-attributed property or equivalent collection.
            if (navigationProp == null)
            {
                var props = MergeNavPropsWithFKs(typeInfo, navProps, navPropsDict, Defaults.ColumnType);
                //first match wins
                navigationProp = props.Item1
                    .FirstOrDefault(ap => !string.IsNullOrWhiteSpace(
                        (ap.GetCustomAttribute<ColumnAttribute>() ?? //if the property itself does not have the column attr, it means it is on a scalar property of a foreign key
                        props.Item2.First(fk => fk.NavigationProperty == ap)
                        .ScalarProperty.GetCustomAttribute<ColumnAttribute>()).Name)
                    );
            }

            //8.Non-scalar key-attributed property or equivalent collection.
            if (navigationProp == null)
            {
                var props = MergeNavPropsWithFKs(typeInfo, navProps, navPropsDict, Defaults.KeyType);
                //first match wins
                navigationProp = props.Item1.FirstOrDefault();
            }

            //9.First non-scalar property or equivalent collection.
            //this is the last line of defence
            if (navigationProp == null)
            {
                //all else has failed, just pick one.
                navigationProp = navProps.FirstOrDefault();
            }

            return navigationProp;
        }

        internal static PropertyInfo GetNavigationPropertyBetweenTypes(
            EntityTypeInfo aTypeInfo, EntityTypeInfo bTypeInfo,
            out PropertyInfo inverseProperty,
            bool assertColumnAttrHasName = true)
        {
            inverseProperty = null;

            //get the nav properties
            var aNavProps = FilterNavProperties(aTypeInfo, bTypeInfo.Type, true);
            var bNavProps = FilterNavProperties(bTypeInfo, aTypeInfo.Type, true);

            var aNavPropsDict = SortNavPropsByAttribute(aNavProps);
            var bNavPropsDict = SortNavPropsByAttribute(bNavProps);

            //Order
            //1.Matching inverse properties.
            //2.Inverse property on one side to a matching foreign keyed property.
            //3.Inverse property on one side to a matching columned property.
            //4.Inverse property on one side to a matching keyed property.
            //5.Inverse property on one side to a matching property.
            //6.Foreign Key.
            //7.Non-scalar column-attributed property or equivalent collection.
            //8.Non-scalar key-attributed property or equivalent collection.
            //9.First non-scalar property or equivalent collection.

            PropertyInfo navigationProp = null;

            //1 - 5
            navigationProp = GetNavPropertyFromInverseProperty
                (aTypeInfo, aNavProps, aNavPropsDict, bTypeInfo, bNavProps, bNavPropsDict,
                out inverseProperty,
                assertColumnHasName: assertColumnAttrHasName);

            //6 - 9
            if (navigationProp == null)
            {
                navigationProp = GetNavPropertyFromNonInverseProperty(aTypeInfo, aNavProps, aNavPropsDict);
                //get the inverse too using same rules.
                inverseProperty = GetNavPropertyFromNonInverseProperty(bTypeInfo, bNavProps, bNavPropsDict);
            }

            return navigationProp;
        }

        internal static LambdaExpression CreateNavigationPropertySelector(Type type, PropertyInfo navProperty, string parameter)
        {
            var parameterExpression = Expression.Parameter(type, parameter);
            var memberExpression = Expression.MakeMemberAccess(parameterExpression, navProperty);
            var lambda = Expression.Lambda(memberExpression, parameterExpression);

            return lambda;
        }

        internal static string GetRandomParameterFor(string entity)
        {
            Random random;

            return "___"
                + $"{entity}"
                + (random = new Random(DateTime.UtcNow.Millisecond)).Next(1, 100)
                + random.Next(1, 100);
        }

        internal static Type GetOtherNodeFromRAndKnownNode(Type R, Type knownNode, bool findingA, ref RelationshipDirection? direction)
        {
            var rInfo = Neo4jAnnotations.GetEntityTypeInfo(R);

            //first determine if the known node exists in the R type
            var rNavProps = FilterNavProperties(rInfo, knownNode, includeIEnumerableArg: false);

            if (rNavProps == null
                || rNavProps.Count == 0
                || rInfo.NavigationableProperties.Count == 1)
            {
                //nothing we can do here.
                return null;
            }

            Tuple<PropertyInfo, RelationshipDirection?> ret = null;

            //Order
            //1. NavProperties with key and optional column with order attributes.
            //2. NavProperties with foreign key and optional column with order attributes.
            //3. Pick the next property from nav properties.
            //4. return null.

            var navProps = rInfo.NavigationableProperties;

            var navPropsDict = SortNavPropsByAttribute(navProps);

            Tuple<PropertyInfo, RelationshipDirection?> getOtherProperty(List<PropertyInfo> props,
                RelationshipDirection? dir)
            {
                if (props.Count == 0)
                    return null;

                //get distinct property types
                props = props.GroupBy(p => p.PropertyType).Select(pg => pg.First()).ToList();

                if (props.Count == 1)
                    return null;

                //find the first property of type knownNode
                var rProp = props.Where(p => rNavProps.Contains(p)).Select((p, i) => new
                {
                    Property = p,
                    Index = props.IndexOf(p)
                }).FirstOrDefault();

                if (rProp == null)
                {
                    return null;
                }

                if (dir == null || dir == RelationshipDirection.Automatic)
                {
                    //try find direction
                    var beforeDiff = rProp.Index; var afterDiff = props.Count - rProp.Index + 1;

                    if (beforeDiff < afterDiff)
                    {
                        if (findingA)
                        {
                            dir = RelationshipDirection.Outgoing;
                        }
                        else
                        {
                            dir = RelationshipDirection.Incoming;
                        }
                    }
                    else if (beforeDiff != afterDiff)
                    {
                        if (findingA)
                        {
                            dir = RelationshipDirection.Incoming;
                        }
                        else
                        {
                            dir = RelationshipDirection.Outgoing;
                        }
                    }
                }

                PropertyInfo prop = null;

                if (dir == RelationshipDirection.Outgoing)
                {
                    if (findingA)
                    {
                        //pick the later property
                        prop = rProp.Index + 1 < props.Count ? props[rProp.Index + 1] : null;
                    }
                    else
                    {
                        //pick the earlier property
                        prop = rProp.Index > 0 ? props[rProp.Index - 1] : null;
                    }
                }
                else if (dir == RelationshipDirection.Incoming)
                {
                    if (findingA)
                    {
                        //pick the earlier property
                        prop = rProp.Index > 0 ? props[rProp.Index - 1] : null;
                    }
                    else
                    {
                        //pick the later property
                        prop = rProp.Index + 1 < props.Count ? props[rProp.Index + 1] : null;
                    }
                }
                else
                {
                    //assume outgoing
                    //but attempt both approaches
                    //hopefully there are only two properties in total
                    if (findingA)
                    {
                        prop = rProp.Index + 1 < props.Count ? props[rProp.Index + 1]
                            : (rProp.Index > 0 ? props[rProp.Index - 1] : null);
                    }
                    else
                    {
                        prop = rProp.Index > 0 ? props[rProp.Index - 1]
                            : (rProp.Index + 1 < props.Count ? props[rProp.Index + 1] : null);
                    }
                }

                return new Tuple<PropertyInfo, RelationshipDirection?>(prop, dir);
            };

            //1. NavProperties with key and optional column with order attributes.
            var properties = navPropsDict[Defaults.KeyType]
                .UnionNavPropsWithFKs(rInfo, Defaults.KeyType, null, out var FKs).ToList();

            ret = getOtherProperty(properties, direction);
            direction = direction ?? ret?.Item2;

            //2. NavProperties with foreign key and optional column with order attributes.
            if (ret == null)
            {
                properties = navPropsDict[Defaults.ForeignKeyType]
                    .UnionNavPropsWithFKs(rInfo, Defaults.ForeignKeyType, null, out FKs).ToList();
                ret = getOtherProperty(properties, direction);
                direction = direction ?? ret?.Item2;
            }

            //3. Pick the next property from nav properties.
            if (ret == null)
            {
                properties = navProps;
                ret = getOtherProperty(properties, direction);
                direction = direction ?? ret?.Item2;
            }

            return ret?.Item1?.PropertyType;
        }

        internal static Tuple<PropertyInfo, Type> GetR(EntityTypeInfo typeInfo, EntityTypeInfo otherTypeInfo,
            bool findingA, ref RelationshipDirection? dir)
        {
            //get all nav properties from Type, and go through each one by one

            var navProps = new List<PropertyInfo>(typeInfo.NavigationableProperties);
            var navPropsWithOtherType = FilterProperties(navProps, otherTypeInfo.Type, includeIEnumerableArg: true);
            var navPropsWithType = FilterProperties(navProps, typeInfo.Type, includeIEnumerableArg: true);
            //remove any nav property with bType
            //also remove any nav property with aType
            navProps = navProps.Except(navPropsWithOtherType).Except(navPropsWithType).ToList();

            //continue if we still have something
            if (navProps.Count > 0)
            {
                foreach (var navProp in navProps)
                {
                    //check to see if this is the one
                    //first match wins
                    var propertyType = navProp.PropertyType;

                    bool hasRepeated = false;

                    repeat:
                    RelationshipDirection? direction = dir;

                    var retrieved = GetOtherNodeFromRAndKnownNode
                        (propertyType, typeInfo.Type, findingA, ref direction);

                    if (retrieved == otherTypeInfo.Type)
                    {
                        //we found a winner
                        //only assign direction when we find a winner
                        dir = dir ?? direction;
                        return new Tuple<PropertyInfo, Type>(navProp, propertyType);
                    }
                    else if (!hasRepeated && propertyType.GetTypeInfo().IsGenericType) //maybe it's a generic type we didn't consider
                    {
                        hasRepeated = true;
                        propertyType = Utilities.GetEnumerableGenericType(propertyType);

                        if (propertyType != null)
                        {
                            //we guessed right
                            //now repeat
                            goto repeat;
                        }
                    }
                }
            }

            return null;
        }

        internal static RelationshipDirection? GetDirection(Pattern pattern)
        {
            //RELATIONSHIP DIRECTION
            //1. Direction as specified by user.
            //2. ForeignKey Attribute on only one side of the relationship indicates outgoing.
            //3. Navigation property on only one side of the relationship indicates outgoing.
            //4. Column Attribute with Name on only one side of the relationship indicates outgoing.
            //5. Key attributed properties in a relationship class arranged in descending order of their position in the class indicates outgoing.
            //   That is, the relationship arrow grows from the keyed property that appeared last in the class towards the one that appeared first.
            //   You can use an explicit column attribute order to rearrange this order, or to ensure an accurate arrangement in the first place.
            //   Column attribute order value must be 1 or greater.
            //6. Foreign Key attributed properties in a relationship class arranged in descending order of their position in the class indicates outgoing.
            //7. Navigation properties in a relationship class arranged in descending order of their position in the class indicates outgoing.
            //8. Assume outgoing.

            RelationshipDirection? dir = null;

            //get the nav props
            var aNavProp = pattern.ANavProperty; var bNavProp = pattern.BNavProperty;

            //2-4
            if (aNavProp != null || bNavProp != null)
            {
                //2. ForeignKey Attribute on only one side of the relationship indicates outgoing.
                object aObject = aNavProp != null ? pattern.ATypeInfo.ForeignKeyProperties
                    .Where(fk => fk.NavigationProperty == aNavProp)
                    .Select(fk => fk.Attribute).FirstOrDefault() : null;

                object bObject = bNavProp != null ? pattern.BTypeInfo.ForeignKeyProperties
                    .Where(fk => fk.NavigationProperty == bNavProp)
                    .Select(fk => fk.Attribute).FirstOrDefault() : null;

                Func<bool> foundSomething = () => aObject != bObject && (aObject == null || bObject == null);

                //3. Navigation property on only one side of the relationship indicates outgoing.
                if (!foundSomething())
                {
                    aObject = aNavProp;
                    bObject = bNavProp;
                }

                //4. Column Attribute with Name on only one side of the relationship indicates outgoing.
                if (!foundSomething())
                {
                    ColumnAttribute attr = null;
                    aObject = !string.IsNullOrWhiteSpace
                        ((attr = aNavProp?.GetCustomAttribute(Defaults.ColumnType) as ColumnAttribute)?.Name) ?
                        attr : null;
                    bObject = !string.IsNullOrWhiteSpace
                        ((attr = bNavProp?.GetCustomAttribute(Defaults.ColumnType) as ColumnAttribute)?.Name) ?
                        attr : null;
                }

                if (foundSomething())
                {
                    //we need just one object
                    //if it is for A, outgoing, else incoming.
                    dir = aObject != null ? RelationshipDirection.Outgoing : RelationshipDirection.Incoming;
                }
            }

            //5-7
            if (dir == null && pattern.HasRType)
            {
                var knownType = pattern.HasAType ? pattern.AType : (pattern.HasBType ? pattern.BType : null);

                if (knownType != null)
                {
                    //this method assign a direction if found via the rules 5 - 7 above
                    var otherType = GetOtherNodeFromRAndKnownNode
                        (pattern.RType, knownType, findingA: knownType != pattern.AType, direction: ref dir);

                    if (otherType == null)
                    {
                        //then don't trust the direction returned
                        dir = null;
                    }
                }
            }

            //8. Assume outgoing.
            if (dir == null)
            {
                //all else failed
                //we tried
                //this is our last resort
                dir = RelationshipDirection.Outgoing;
            }

            return dir;
        }

        internal static LambdaExpression GetConstraintsAsProperties(LambdaExpression constraints, Type type)
        {
            var setMethod = Utilities.GetMethodInfo(() => Extensions.Set<object>(null, null, true), type);

            return Expression.Lambda(Expression.Call(setMethod, Expression.Constant(type.GetDefaultValue(), type),
                constraints, Expression.Constant(true) //i.e, usePredicateOnly: true
                ));
        }

        internal static void ResolveClientAndSerializers(Pattern pattern)
        {
            if (pattern.client == null || pattern.serializer == null || (pattern.entityResolver == null && pattern.entityConverter == null))
            {
                var client = (pattern.InternalCypherQuery as IAttachedReference)?.Client;

                var serializer = client?.Serializer ?? new CustomJsonSerializer()
                {
                    JsonContractResolver = client?.JsonContractResolver ?? GraphClient.DefaultJsonContractResolver,
                    JsonConverters = client?.JsonConverters ?? (IEnumerable<JsonConverter>)GraphClient.DefaultJsonConverters
                };

                var resolver = client?.JsonContractResolver as EntityResolver ??
                    (serializer as CustomJsonSerializer)?.JsonContractResolver as EntityResolver;

                var converters = new List<JsonConverter>((IEnumerable<JsonConverter>)client?.JsonConverters ?? new JsonConverter[0]);
                converters.AddRange((serializer as CustomJsonSerializer)?.JsonConverters ?? new JsonConverter[0]);

                var converter = converters.FirstOrDefault(c => c is EntityConverter) as EntityConverter;

                Func<object, string> actualSerializer = serializer != null ? (obj) => serializer.Serialize(obj) : (Func<object, string>)null;

                pattern.client = client;
                pattern.serializer = serializer;
                pattern.entityResolver = resolver;
                pattern.entityConverter = converter;
                pattern.actualSerializer = actualSerializer;
            }
        }

        internal static JObject GetFinalProperties(Pattern pattern, 
            LambdaExpression properties, LambdaExpression constraints, Type type)
        {
            ResolveClientAndSerializers(pattern);

            var lambdaExpr = properties ?? (constraints != null ? GetConstraintsAsProperties(constraints, type) : null);

            return Utilities.GetFinalProperties(lambdaExpr, pattern.entityResolver, pattern.entityConverter, pattern.actualSerializer);
        }
    }

    public enum PatternBuildStrategy
    {
        /// <summary>
        /// Writes the properties directly into the generated pattern.
        /// E.g. (a:Movie { title: "Grey's Anatomy", year: 2017 }
        /// </summary>
        NoParams = 0,
        /// <summary>
        /// Stores the properties via a <see cref="ICypherFluentQuery.WithParam(string, object)"/> call, and replaces it with a parameter.
        /// E.g. (a:Movie { movie }), where "movie" is the parameter. This style can be used for the CREATE and CREATE UNIQUE statements.
        /// </summary>
        WithParams,
        /// <summary>
        /// Stores the properties via a <see cref="ICypherFluentQuery.WithParam(string, object)"/> call, and replaces each property value with corresponding parameter property.
        /// E.g. (a:Movie { title: {movie}.title, year: {movie}.year }), where "movie" is the parameter. This style is applicable to the MATCH and OPTIONAL MATCH statements.
        /// </summary>
        WithParamsForValues,
    }
}
