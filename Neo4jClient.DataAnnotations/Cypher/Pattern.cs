using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Utils;
using Neo4jClient.Serialization;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public class Pattern : Annotated, IPattern
    {
        private LambdaExpression abSelector, arSelector, rbSelector;
        private bool abSelSet, arSelSet, rbSelSet;

        private LambdaExpression aConstrs, rConstrs, bConstrs;

        private object aFinalObj, rFinalObj, bFinalObj;
        private bool aFinalObjSet, rFinalObjSet, bFinalObjSet;


        private JObject aFinalProps, rFinalProps, bFinalProps;
        private bool aFinalPropsSet, rFinalPropsSet, bFinalPropsSet;


        private bool aHasFuncs, rHasFuncs, bHasFuncs;


        private EntityTypeInfo aInfo, rInfo, bInfo;


        private List<string> aLabels, rTypes, bLabels;
        protected bool aLabelsSet, rTypesSet, bLabelsSet;


        private PropertyInfo aNavProp, bNavProp;
        private bool aNavPropSet, bNavPropSet;


        private LambdaExpression aProps, rProps, bProps;


        internal Type aType, rType, bType;
        internal bool aTypeSet, rTypeSet, bTypeSet;

        internal string aVar, rVar, bVar;
        private bool aVarIsAuto, rVarIsAuto, bVarIsAuto;

        private PropertiesBuildStrategy? buildStrategy;

        private RelationshipDirection? dir;
        private bool dirSet, isAutoDirection;

        public Pattern(ICypherFluentQuery query, IPathExtent path)
            : this(query, path, null)
        {
        }

        public Pattern(ICypherFluentQuery query, IPathExtent path, AnnotationsContext context = null)
            : base(query, context)
        {
            Path = path;
        }

        protected EntityResolver Resolver => QueryContext?.Resolver;
        protected EntityConverter Converter => QueryContext?.Converter;
        protected ISerializer Serializer => QueryContext?.ISerializer;
        protected Func<object, string> SerializerFunc => QueryContext?.SerializeCallback;
        protected IGraphClient Client => QueryContext?.Client;
        protected Func<ICypherFluentQuery, QueryWriterWrapper> QueryWriterGetter => QueryContext.QueryWriterGetter;
        protected QueryContext QueryContext { get; set; }


        public bool? AIsAlreadyBound { get; protected internal set; }

        public bool? RIsAlreadyBound { get; protected internal set; }

        public bool? BIsAlreadyBound { get; protected internal set; }


        public bool AVarIsAuto => AVariable != null && aVarIsAuto;

        public bool RVarIsAuto => RVariable != null && rVarIsAuto;

        public bool BVarIsAuto => BVariable != null && bVarIsAuto;


        public bool HasAType => AType != null && AType != Defaults.CypherObjectType;

        public bool HasRType => RType != null && RType != Defaults.CypherObjectType;

        public bool HasBType => BType != null && BType != Defaults.CypherObjectType;

        /// <summary>
        ///     This would contain the properties as they would be written to cypher.
        ///     Variables would be a <see cref="JRaw" /> value here, and not <see cref="JValue" /> string.
        /// </summary>
        public JObject AFinalProperties
        {
            get
            {
                if (!aFinalPropsSet)
                {
                    aFinalPropsSet = true;
                    aFinalProps = GetFinalProperties(this, AProperties, AConstraints, AType, out aHasFuncs);
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
                    rFinalProps = GetFinalProperties(this, RProperties, RConstraints, RType, out rHasFuncs);
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
                    bFinalProps = GetFinalProperties(this, BProperties, BConstraints, BType, out bHasFuncs);
                }

                return bFinalProps;
            }
        }

        /// <summary>
        ///     This would contain the properties as they would be written to cypher.
        ///     Variables would be a <see cref="JRaw" /> value here, and not <see cref="JValue" /> string.
        /// </summary>
        public object AFinalObject
        {
            get
            {
                if (!aFinalObjSet)
                {
                    aFinalObjSet = true;
                    aFinalObj = GetFinalObject(this, AProperties, AConstraints, AType);
                }

                return aFinalObj;
            }
        }

        public object RFinalObject
        {
            get
            {
                if (!rFinalObjSet)
                {
                    rFinalObjSet = true;
                    rFinalObj = GetFinalObject(this, RProperties, RConstraints, RType);
                }

                return rFinalObj;
            }
        }

        public object BFinalObject
        {
            get
            {
                if (!bFinalObjSet)
                {
                    bFinalObjSet = true;
                    bFinalObj = GetFinalObject(this, BProperties, BConstraints, BType);
                }

                return bFinalObj;
            }
        }

        public bool AFinalPropertiesHasFunctions => aHasFuncs;

        public bool RFinalPropertiesHasFunctions => rHasFuncs;

        public bool BFinalPropertiesHasFunctions => bHasFuncs;

        public EntityTypeInfo ATypeInfo => aInfo ?? (aInfo = EntityService.GetEntityTypeInfo(AType));

        public EntityTypeInfo RTypeInfo => rInfo ?? (rInfo = EntityService.GetEntityTypeInfo(RType));

        public EntityTypeInfo BTypeInfo => bInfo ?? (bInfo = EntityService.GetEntityTypeInfo(BType));

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
                        //still null
                        //maybe the selectors were set manually, so find nav here from selected property.
                        aNavProp = Utils.Utilities.GetPropertyInfoFrom(sel.Body, AType, null);
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
                        var otherTypeInfo = EntityService.GetEntityTypeInfo(otherType);
                        var selectedProp = Utils.Utilities.GetPropertyInfoFrom(sel.Body, otherType, BType);

                        var navigationProp =
                            GetNavigationPropertyBetweenTypes(otherTypeInfo, BTypeInfo,
                                out var inverseProp, selectedProp);

                        if (navigationProp != null && inverseProp != null //our interest is in both properties here
                                                   && navigationProp == selectedProp //these must match
                            )
                            //we have a winner
                            bNavProp = inverseProp;
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

        public IPathExtent Path { get; protected internal set; }

        public PropertiesBuildStrategy BuildStrategy
        {
            get =>
                buildStrategy ??
                Path?.Builder?.PatternBuildStrategy ??
                PropertiesBuildStrategy.NoParams;
            set => buildStrategy = value;
        }

        public string AVariable
        {
            get =>
                aVar ??
                (aVar = (abSelector ?? arSelector)?.Parameters[0].Name) ??
                GetVariable("A", ref aVar, ref aVarIsAuto);

            protected internal set
            {
                if (value != aVar)
                    aVarIsAuto = false;

                aVar = !string.IsNullOrWhiteSpace(value) ? value : null;
            }
        }

        public string RVariable
        {
            get =>
                rVar ??
                (rVar = rbSelector?.Parameters[0].Name) ??
                GetVariable("R", ref rVar, ref rVarIsAuto);

            protected internal set
            {
                if (value != rVar)
                    rVarIsAuto = false;

                rVar = !string.IsNullOrWhiteSpace(value) ? value : null;
            }
        }

        public string BVariable
        {
            get => bVar ?? GetVariable("B", ref bVar, ref bVarIsAuto);
            protected internal set
            {
                if (value != bVar)
                    bVarIsAuto = false;

                bVar = !string.IsNullOrWhiteSpace(value) ? value : null;
            }
        }

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
                        var type = GetOtherNodeFromRAndKnownNode(EntityService, RType, BType, true, ref direction);
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
                        && abSelector ==
                        null //if we already have a selector describing a predefined relationship, finding R is futile.
                        && HasAType && HasBType)
                    {
                        //infer R from A and B.
                        var direction = dir;
                        var prop = GetR(ATypeInfo, BTypeInfo, false, ref direction);

                        if (prop == null || prop.Item2 == null)
                        {
                            //switch them and try again
                            var direction2 = dir;
                            prop = GetR(BTypeInfo, ATypeInfo, true, ref direction2);
                        }

                        rType = prop?.Item2 ?? rType;
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
                        var type = GetOtherNodeFromRAndKnownNode(EntityService, RType, AType, false, ref direction);
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
                            out var inverseProp);

                        if (navigationProp != null)
                        {
                            //set navprops
                            ANavProperty = navigationProp;
                            BNavProperty = inverseProp ?? bNavProp;

                            //create expression
                            abSelector = CreateNavigationPropertySelector(AType, navigationProp, AVariable);
                        }
                    }
                }

                return abSelector;
            }
            protected internal set
            {
                abSelector = value;
                abSelSet = value != null;

                if (value != null && AVarIsAuto) AVariable = null; //allow it to reset to this
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
                            out var inverseProperty, null,
                            false); //column with name is not necessary in this case

                        if (navigationProperty != null)
                        {
                            //set navprops
                            ANavProperty = navigationProperty;

                            //create expression
                            arSelector = CreateNavigationPropertySelector(AType, navigationProperty, AVariable);
                        }
                    }
                }

                return arSelector;
            }
            protected internal set
            {
                arSelector = value;
                arSelSet = value != null;

                if (value != null && AVarIsAuto) AVariable = null; //allow it to reset to this
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
                            out var inverseProp);

                        if (navigationProp != null)
                        {
                            //set navprops
                            BNavProperty = inverseProp ?? bNavProp;

                            //create expression
                            rbSelector = CreateNavigationPropertySelector(RType, navigationProp, RVariable);
                        }
                    }
                }

                return rbSelector;
            }
            protected internal set
            {
                rbSelector = value;
                rbSelSet = value != null;

                if (value != null && RVarIsAuto) RVariable = null; //allow it to reset to this
            }
        }


        public Tuple<int?, int?> RHops { get; protected internal set; }

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

                    if (aLabels == null) aLabels = new List<string>();

                    aLabelsSet = true;

                    if (!UseGivenALabelsOnly && AType != Defaults.CypherObjectType)
                        aLabels.AddRange(ATypeInfo.LabelsWithTypeNameCatch);

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
                    if (rTypes == null) rTypes = new List<string>();

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
                    if (bLabels == null) bLabels = new List<string>();

                    bLabelsSet = true;

                    if (!UseGivenBLabelsOnly && BType != Defaults.CypherObjectType)
                        bLabels.AddRange(BTypeInfo.LabelsWithTypeNameCatch);

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

        public LambdaExpression AProperties
        {
            get => aProps;
            protected internal set
            {
                aProps = value;
                aFinalPropsSet = false;
            }
        }

        public LambdaExpression RProperties
        {
            get => rProps;
            protected internal set
            {
                rProps = value;
                rFinalPropsSet = false;
            }
        }

        public LambdaExpression BProperties
        {
            get => bProps;
            protected internal set
            {
                bProps = value;
                bFinalPropsSet = false;
            }
        }

        public LambdaExpression AConstraints
        {
            get => aConstrs;
            protected internal set
            {
                aConstrs = value;
                aFinalPropsSet = false;
            }
        }

        public LambdaExpression RConstraints
        {
            get => rConstrs;
            protected internal set
            {
                rConstrs = value;
                rFinalPropsSet = false;
            }
        }

        public LambdaExpression BConstraints
        {
            get => bConstrs;
            protected internal set
            {
                bConstrs = value;
                bFinalPropsSet = false;
            }
        }

        public RelationshipDirection? Direction
        {
            get
            {
                if (!dirSet)
                {
                    dirSet = true;

                    //1. Direction as specified by user.
                    if (dir == null) dir = GetDirection(this);

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


        protected override string InternalBuild()
        {
            //CHECK LIST
            //Variables
            //Labels
            //Hops
            //FinalProperties
            //Direction

            ResolveInternalUtilities(this);

            if (HasAType) AnnotationsContext.EntityService.AddEntityType(AType);

            if (HasRType) AnnotationsContext.EntityService.AddEntityType(RType);

            if (HasBType) AnnotationsContext.EntityService.AddEntityType(BType);

            var builder = new StringBuilder();

            string A = null, R = null, B = null;

            if (!IsExtension)
            {
                //Build A
                A = BuildNode(this, aVar, aVarIsAuto, AVariable, AIsAlreadyBound, ALabels, () => AFinalObject,
                    () => AFinalProperties, () => AFinalPropertiesHasFunctions);

                builder.Append(A);
            }

            //Build R
            R = BuildRelationship(this, rVar, rVarIsAuto, RVariable, RIsAlreadyBound, RTypes, RHops, () => RFinalObject,
                () => RFinalProperties, () => RFinalPropertiesHasFunctions);
            if (R == "[]")
                R = null; //just omit the empty ones

            //Build B
            B = BuildNode(this, bVar, bVarIsAuto, BVariable, BIsAlreadyBound, BLabels, () => BFinalObject,
                () => BFinalProperties, () => BFinalPropertiesHasFunctions);

            if (IsExtension || R != null || HasBType || B != null && B != "()")
            {
                if (Direction == RelationshipDirection.Incoming) builder.Append("<");

                builder.Append("-");

                if (R != null) builder.Append(R);

                builder.Append("-");

                if (Direction == RelationshipDirection.Outgoing) builder.Append(">");

                builder.Append(B);
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            try
            {
                return InternalBuild();
            }
            catch
            {
            }

            return base.ToString();
        }


        private void ResolveRTypes(List<string> rTypes)
        {
            if (!UseGivenRTypesOnly) // && RType != Defaults.CypherObjectType)
            {
                //RELATIONSHIP TYPE(declaring class takes precedence over relationship class)
                //1. Column Attribute Name on only one side of the relationship.
                //2. ForeignKey Attribute Name on only one side of the relationship that doesn't point to any property on the class.
                //3. First Table Attribute Name on the relationship class inheritance hierarchy starting from the class itself to all base classes.
                //4. Name of navigation property with direction considered, and/ or the class name of a provided relationship class
                //   (which should also be the same type as the relationship property type).

                var additionalTypes = new List<string>();

                //get the navs.
                var aNavProp = ANavProperty;
                var bNavProp = BNavProperty;
                if (aNavProp != null || bNavProp != null)
                {
                    //1. Column Attribute Name on only one side of the relationship.
                    var aAttr = aNavProp?.GetCustomAttribute(Defaults.ColumnType);
                    var bAttr = bNavProp?.GetCustomAttribute(Defaults.ColumnType);

                    var attrArray = new[] { aAttr, bAttr };

                    var columnAttr = attrArray.Where(a => a != null
                                                          && !string.IsNullOrWhiteSpace(((ColumnAttribute)a).Name))
                            .SingleOrDefault() as
                        ColumnAttribute; //we expect just one attribute to be defined on the relationship. two would create confusion.

                    if (columnAttr != null) additionalTypes.Add(columnAttr.Name);

                    //2. ForeignKey Attribute Name on only one side of the relationship that doesn't point to any property on the class.
                    if (additionalTypes.Count == 0)
                    {
                        attrArray[0] = aNavProp != null
                            ? ATypeInfo.ForeignKeyProperties.Where(fk =>
                                    fk.NavigationProperty == aNavProp && !fk.IsAttributeAutoCreated &&
                                    !fk.IsAttributePointingToProperty)
                                .Select(fk => fk.Attribute).FirstOrDefault()
                            : null;

                        attrArray[1] = bNavProp != null
                            ? BTypeInfo.ForeignKeyProperties.Where(fk =>
                                    fk.NavigationProperty == bNavProp && !fk.IsAttributeAutoCreated &&
                                    !fk.IsAttributePointingToProperty)
                                .Select(fk => fk.Attribute).FirstOrDefault()
                            : null;

                        var fkAttr = attrArray.Where(a => a != null).SingleOrDefault() as ForeignKeyAttribute;

                        if (fkAttr != null && !string.IsNullOrWhiteSpace(fkAttr.Name)) additionalTypes.Add(fkAttr.Name);
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
                                if (ANavProperty != null) additionalTypes.Add(ANavProperty.Name);
                                break;
                            }
                        case RelationshipDirection.Incoming:
                            {
                                if (BNavProperty != null) additionalTypes.Add(BNavProperty.Name);
                                break;
                            }
                        default:
                            {
                                if (ANavProperty != null || BNavProperty != null)
                                    additionalTypes.Add((ANavProperty ?? BNavProperty).Name);
                                break;
                            }
                    }

                    if (HasRType) additionalTypes.Add(RType.Name);
                }

                rTypes.AddRange(additionalTypes);
            }
        }

        internal static string BuildNode(Pattern pattern,
            string variable, bool variableIsAuto,
            string Variable, //NOTE: this call order is important, and method signature should not be reshuffled.
            bool? isAlreadyBound, IEnumerable<string> labels,
            Func<object> finalObjectGetter, Func<JObject> finalPropertiesGetter,
            Func<bool> finalPropsHasFuncsGetter)
        {
            //e.g. (param:labels {finalProperties})

            var builder = new StringBuilder();

            if (!variableIsAuto && variable != null)
            {
                builder.Append(variable);
                //don't continue is this is already bound
                isAlreadyBound = isAlreadyBound == null ? IsAlreadyBound(pattern, variable) : isAlreadyBound;
            }

            if (isAlreadyBound != true)
            {
                if (labels?.Count() > 0) builder.Append(":" + labels.Aggregate((first, second) => $"{first}:{second}"));

                ResolveInternalUtilities(pattern);

                var properties = BuildProperties
                (finalObjectGetter, finalPropertiesGetter, finalPropsHasFuncsGetter,
                    ref pattern.CypherQuery, Variable,
                    pattern.BuildStrategy, pattern.SerializerFunc);

                if (properties != null && properties != "{  }")
                    builder.Append($" {properties}");
            }

            return $"({builder.ToString().Trim()})";
        }

        internal static bool IsAlreadyBound(Pattern pattern, string variable)
        {
            //this is a crude test.
            var alreadyBound = pattern.QueryWriterGetter != null
                               && pattern.QueryWriterGetter(pattern.CypherQuery)?.ContainsParameterWithKey(variable) ==
                               true;

            if (!alreadyBound) alreadyBound = pattern.CypherQuery.Query?.QueryParameters?.ContainsKey(variable) == true;

            return alreadyBound;
        }

        internal static string BuildRelationship(Pattern pattern,
            string variable, bool variableIsAuto,
            string Variable, //NOTE: this call order is important, and method signature should not be reshuffled.
            bool? isAlreadyBound, IEnumerable<string> types,
            Tuple<int?, int?> hops,
            Func<object> finalObjectGetter,
            Func<JObject> finalPropertiesGetter,
            Func<bool> finalPropsHasFuncsGetter)
        {
            //e.g. (param:types*hops {finalProperties})

            var builder = new StringBuilder();

            string hopsText = null;
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
                        if (hops.Item1 != null) builder.Append(hops.Item1);

                        builder.Append("..");

                        if (hops.Item2 != null) builder.Append(hops.Item2);
                    }
                }

                hopsText = builder.ToString();
                builder.Clear();
            }

            var hasHops = hopsText != null;
            //bool isAlreadyBound = false;

            if (!variableIsAuto && variable != null)
            {
                if (!hasHops)
                {
                    builder.Append(variable);
                    isAlreadyBound = isAlreadyBound == null ? IsAlreadyBound(pattern, variable) : isAlreadyBound;
                }
                else
                {
                    //Neo4j has deprecated variable for hops relationships i.e. something like: MATCH (a)-[r*1..2]-(b) RETURN a, r, b; r is no longer allowed in this case.
                    //The advised pattern to use is: MATCH p = (a)-[*1..2]-(b) RETURN a, relationships(p) AS r, b.
                    //So here, we just automatically assign the path variable for the user.
                    pattern.Path = pattern.Path?.SharedAssign();
                }
            }

            if ((hasHops || isAlreadyBound != true) && types?.Count() > 0
            ) //add types even with hops. if the types needs to be skipped, the user should explicitly specify null types.
                builder.Append(":" + types.Aggregate((first, second) => $"{first}|{second}"));

            if (hasHops) builder.Append(hopsText);

            if (hasHops || isAlreadyBound != true /*&& finalPropertiesGetter?.Count > 0*/
            ) //add the properties even with hops.
            {
                var properties = BuildProperties
                (finalObjectGetter, finalPropertiesGetter, finalPropsHasFuncsGetter,
                    ref pattern.CypherQuery, Variable, pattern.BuildStrategy, pattern.SerializerFunc);

                if (properties != null && properties != "{  }")
                    builder.Append($" {properties}");
            }

            return $"[{builder.ToString().Trim()}]";
        }

        internal static string BuildProperties
        (Func<object> finalObjectGetter, Func<JObject> finalPropertiesGetter, Func<bool> finalPropsHasFuncsGetter,
            ref ICypherFluentQuery query, string Variable,
            PropertiesBuildStrategy buildStrategy, Func<object, string> serializer)
        {
            var queryContext = CypherUtilities.GetQueryContext(query);
            queryContext.CurrentBuildStrategy = buildStrategy;

            var value = CypherUtilities.BuildFinalProperties(queryContext,
                Variable, finalObjectGetter, finalPropertiesGetter, finalPropsHasFuncsGetter,
                ref buildStrategy, out var param, out var newProperties,
                ": ", null, false,
                true, true);

            //CypherUtilities.ResolveFinalObjectProperties(finalObjectGetter, finalPropertiesGetter, finalPropsHasFuncsGetter,
            //    ref buildStrategy, out var finalObject, out var finalProperties);

            //string value = null;

            //if (finalObject != null || finalProperties?.Count > 0)
            //{
            //    switch (buildStrategy)
            //    {
            //        case PropertiesBuildStrategy.WithParams:
            //        case PropertiesBuildStrategy.WithParamsForValues:
            //            {
            //                var _finalProperties = finalProperties;

            //                value = "$" + Variable;

            //                if (buildStrategy == PropertiesBuildStrategy.WithParamsForValues)
            //                {
            //                    value = CypherUtilities.BuildWithParamsForValues(finalProperties, serializer,
            //                        getKey: (propertyName) => propertyName, separator: ": ",
            //                        getValue: (propertyName) => $"${Variable}.{propertyName}",
            //                        hasRaw: out var hasRaw, newFinalProperties: out var newFinalProperties);

            //                    _finalProperties = newFinalProperties ?? _finalProperties;
            //                }

            //                query = query.WithParam(Variable, finalObject != null && _finalProperties == finalProperties ? finalObject : _finalProperties);
            //                break;
            //            }
            //        case PropertiesBuildStrategy.NoParams:
            //            {
            //                value = finalProperties.Properties()
            //                        .Select(jp => $"{jp.Name}: {serializer(jp.Value)}")
            //                        .Aggregate((first, second) => $"{first}, {second}");
            //                break;
            //            }
            //    }

            //    return value?.StartsWith("$") != true ? $"{{ {value} }}" : value;
            //}

            return value;
        }

        internal static string GetVariable(string entity, ref string current, ref bool isAutoFlag)
        {
            if (current == null)
            {
                isAutoFlag = true;
                current = Utils.Utilities.GetRandomVariableFor(entity);
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
            var iEnumerableType = typeof(IEnumerable<>);
            var ret = properties.Where(p => p.PropertyType == propertyType
                                            || includeIEnumerableArg &&
                                            Utils.Utilities.GetEnumerableGenericType(p.PropertyType) == propertyType
            ).ToList();

            return ret;
        }

        internal static Dictionary<Type, List<PropertyInfo>> SortNavPropsByAttribute(List<PropertyInfo> navProperties)
        {
            var foreignKeyType = Defaults.ForeignKeyType;
            var inversePropType = Defaults.InversePropertyType;
            var columnType = Defaults.ColumnType;
            var keyType = Defaults.KeyType;

            var navPropsDict = new Dictionary<Type, List<PropertyInfo>>
            {
                {inversePropType, new List<PropertyInfo>()},
                {foreignKeyType, new List<PropertyInfo>()},
                {columnType, new List<PropertyInfo>()},
                {keyType, new List<PropertyInfo>()}
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

        internal static PropertyInfo GetNavPropertyFromInversePropertyAttribute(EntityTypeInfo aTypeInfo,
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
            List<PropertyInfo> aInvProps = aNavPropsDict?[Defaults.InversePropertyType],
                bInvProps = bNavPropsDict?[Defaults.InversePropertyType];

            if (aInvProps.Count > 0 && bInvProps.Count > 0)
                //find its matching property. the first match wins
                navigationProp = aInvProps
                    .FirstOrDefault(p => bInvProps
                        .Any(bp => p.Name == bp.GetCustomAttribute<InversePropertyAttribute>().Property
                                   && bp.Name == p.GetCustomAttribute<InversePropertyAttribute>().Property
                                   && (localInverseProp = bp) !=
                                   null //HACK: of course it won't be null, but for assignment sakes, we do this. Make sure it's last statement so we don't have a false assignment before conditions are run.
                        ));

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
                                       && (assertColumnHasName
                                           ? !string.IsNullOrWhiteSpace(
                                               (bp.GetCustomAttribute<ColumnAttribute
                                                >() ?? //if the property itself does not have the column attr, it means it is on a scalar property of aforeign key
                                                bProps.Item2.First(fk => fk.NavigationProperty == bp)
                                                    .ScalarProperty.GetCustomAttribute<ColumnAttribute>()).Name)
                                           : true)
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
                                                  .Any(bp => ap.Name == bp
                                                                 .GetCustomAttribute<InversePropertyAttribute>()
                                                                 .Property
                                                             && (localInverseProp = bp) != null)
                                              && (assertColumnHasName
                                                  ? !string.IsNullOrWhiteSpace(
                                                      (ap.GetCustomAttribute<ColumnAttribute
                                                       >() ?? //if the property itself does not have the column attr, it means it is on a scalar property of aforeign key
                                                       aProps.Item2.First(fk => fk.NavigationProperty == ap)
                                                           .ScalarProperty.GetCustomAttribute<ColumnAttribute>()).Name)
                                                  : true));
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
                        .Any(bp => ap.Name == bp.GetCustomAttribute<InversePropertyAttribute>()
                                       .Property
                                   && (localInverseProp = bp) != null));
            }

            if (navigationProp != null)
                //only assign if we found something
                inverseProperty = localInverseProp;

            return navigationProp;
        }

        internal static PropertyInfo GetNavPropertyFromNonInversePropertyAttribute(EntityTypeInfo typeInfo,
            List<PropertyInfo> navProps, Dictionary<Type, List<PropertyInfo>> navPropsDict,
            PropertyInfo knownInverseProperty)
        {
            //Order
            //6.Foreign Key.
            //7.Non-scalar column-attributed property or equivalent collection.
            //8.Non-scalar key-attributed property or equivalent collection.
            //9.First non-scalar property or equivalent collection.

            //If there is a known inverse prop, make sure they aren't on the same rank to avoid clashes later.

            PropertyInfo navigationProp = null;
            var hasKnownInverse = knownInverseProperty != null;

            //6.Foreign Key.
            var knownInverseHasFk =
                hasKnownInverse &&
                knownInverseProperty.IsDefined(Defaults.ForeignKeyType); //they must not be on the same rank
            if (navigationProp == null && !knownInverseHasFk)
            {
                //this would do quite unnecessary processing. but for the sakes of future refactoring, we do.
                var props = MergeNavPropsWithFKs(typeInfo, navProps, navPropsDict, Defaults.ForeignKeyType);
                //first match wins
                navigationProp = props.Item1.FirstOrDefault();
            }

            //7.Non-scalar column-attributed property or equivalent collection.
            if (navigationProp == null && !(hasKnownInverse && knownInverseProperty.IsDefined(Defaults.ColumnType)))
            {
                var props = MergeNavPropsWithFKs(typeInfo, navProps, navPropsDict, Defaults.ColumnType);
                //first match wins
                navigationProp = props.Item1
                    .FirstOrDefault(ap => !string.IsNullOrWhiteSpace(
                        (ap.GetCustomAttribute<ColumnAttribute
                         >() ?? //if the property itself does not have the column attr, it means it is on a scalar property of a foreign key
                         props.Item2.First(fk => fk.NavigationProperty == ap)
                             .ScalarProperty.GetCustomAttribute<ColumnAttribute>()).Name)
                    );
            }

            //8.Non-scalar key-attributed property or equivalent collection.
            if (navigationProp == null && !(hasKnownInverse && knownInverseProperty.IsDefined(Defaults.KeyType)))
            {
                var props = MergeNavPropsWithFKs(typeInfo, navProps, navPropsDict, Defaults.KeyType);
                //first match wins
                navigationProp = props.Item1.FirstOrDefault();
            }

            //9.First non-scalar property or equivalent collection with no attributes of interest.
            //this is the last line of defence
            if (navigationProp == null)
                //all else has failed, just pick one of the plain ones.
                navigationProp = navProps
                    .SkipWhile(i => i.IsDefined(Defaults.ForeignKeyType)
                                    || i.IsDefined(Defaults.ColumnType)
                                    || i.IsDefined(Defaults.KeyType))
                    .FirstOrDefault();

            return navigationProp;
        }

        internal static PropertyInfo GetNavigationPropertyBetweenTypes(
            EntityTypeInfo aTypeInfo, EntityTypeInfo bTypeInfo,
            out PropertyInfo inverseProperty, PropertyInfo knownNavProperty = null,
            bool assertColumnAttrHasName = true)
        {
            inverseProperty = null;

            //get the nav properties
            var aNavProps = knownNavProperty != null
                ? new List<PropertyInfo> { knownNavProperty } //make sure its the only one in the collection
                : FilterNavProperties(aTypeInfo, bTypeInfo.Type);

            var bNavProps = FilterNavProperties(bTypeInfo, aTypeInfo.Type);

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
            navigationProp = GetNavPropertyFromInversePropertyAttribute
            (aTypeInfo, aNavProps, aNavPropsDict, bTypeInfo, bNavProps, bNavPropsDict,
                out inverseProperty,
                assertColumnAttrHasName);

            //6 - 9
            if (navigationProp == null)
            {
                if (knownNavProperty != null)
                    navigationProp = knownNavProperty; //just accept what was sent in AS-IS
                else
                    navigationProp = GetNavPropertyFromNonInversePropertyAttribute
                        (aTypeInfo, aNavProps, aNavPropsDict, null);

                //get the inverse too using same rules.
                inverseProperty = GetNavPropertyFromNonInversePropertyAttribute
                    (bTypeInfo, bNavProps, bNavPropsDict, navigationProp);
            }

            return navigationProp;
        }

        internal static LambdaExpression CreateNavigationPropertySelector(Type type, PropertyInfo navProperty,
            string parameter)
        {
            var parameterExpression = Expression.Parameter(type, parameter);
            var memberExpression = Expression.MakeMemberAccess(parameterExpression, navProperty);
            var lambda = Expression.Lambda(memberExpression, parameterExpression);

            return lambda;
        }

        internal static Type GetOtherNodeFromRAndKnownNode
            (EntityService entityService, Type R, Type knownNode, bool findingA, ref RelationshipDirection? direction)
        {
            var rInfo = entityService.GetEntityTypeInfo(R);

            //first determine if the known node exists in the R type
            var rNavProps = FilterNavProperties(rInfo, knownNode, false);

            if (rNavProps == null
                || rNavProps.Count == 0
                || rInfo.NavigationableProperties.Count == 1)
                //nothing we can do here.
                return null;

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

                if (rProp == null) return null;

                if (dir == null || dir == RelationshipDirection.Automatic)
                {
                    //try find direction
                    var beforeDiff = rProp.Index;
                    var afterDiff = props.Count - rProp.Index + 1;

                    if (beforeDiff < afterDiff)
                    {
                        if (findingA)
                            dir = RelationshipDirection.Outgoing;
                        else
                            dir = RelationshipDirection.Incoming;
                    }
                    else if (beforeDiff != afterDiff)
                    {
                        if (findingA)
                            dir = RelationshipDirection.Incoming;
                        else
                            dir = RelationshipDirection.Outgoing;
                    }
                }

                PropertyInfo prop = null;

                if (dir == RelationshipDirection.Outgoing)
                {
                    if (findingA)
                        //pick the later property
                        prop = rProp.Index + 1 < props.Count ? props[rProp.Index + 1] : null;
                    else
                        //pick the earlier property
                        prop = rProp.Index > 0 ? props[rProp.Index - 1] : null;
                }
                else if (dir == RelationshipDirection.Incoming)
                {
                    if (findingA)
                        //pick the earlier property
                        prop = rProp.Index > 0 ? props[rProp.Index - 1] : null;
                    else
                        //pick the later property
                        prop = rProp.Index + 1 < props.Count ? props[rProp.Index + 1] : null;
                }
                else
                {
                    //assume outgoing
                    //but attempt both approaches
                    //hopefully there are only two properties in total
                    if (findingA)
                        prop = rProp.Index + 1 < props.Count ? props[rProp.Index + 1]
                            : rProp.Index > 0 ? props[rProp.Index - 1] : null;
                    else
                        prop = rProp.Index > 0 ? props[rProp.Index - 1]
                            : rProp.Index + 1 < props.Count ? props[rProp.Index + 1] : null;
                }

                return new Tuple<PropertyInfo, RelationshipDirection?>(prop, dir);
            }

            ;

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
            var navPropsWithOtherType = FilterProperties(navProps, otherTypeInfo.Type);
            var navPropsWithType = FilterProperties(navProps, typeInfo.Type);
            //remove any nav property with bType
            //also remove any nav property with aType
            navProps = navProps.Except(navPropsWithOtherType).Except(navPropsWithType).ToList();

            //continue if we still have something
            if (navProps.Count > 0)
                foreach (var navProp in navProps)
                {
                    //check to see if this is the one
                    //first match wins
                    var propertyType = navProp.PropertyType;

                    var hasRepeated = false;

                repeat:
                    var direction = dir;

                    var retrieved = GetOtherNodeFromRAndKnownNode
                        (typeInfo.EntityService, propertyType, typeInfo.Type, findingA, ref direction);

                    if (retrieved == otherTypeInfo.Type)
                    {
                        //we found a winner
                        //only assign direction when we find a winner
                        dir = dir ?? direction;
                        return new Tuple<PropertyInfo, Type>(navProp, propertyType);
                    }

                    if (!hasRepeated && propertyType.GetTypeInfo().IsGenericType
                    ) //maybe it's a generic type we didn't consider
                    {
                        hasRepeated = true;
                        propertyType = Utils.Utilities.GetEnumerableGenericType(propertyType);

                        if (propertyType != null)
                            //we guessed right
                            //now repeat
                            goto repeat;
                    }
                }

            return null;
        }

        internal static RelationshipDirection? GetDirection(Pattern pattern)
        {
            //RELATIONSHIP DIRECTION
            //1. Direction as specified by user.
            //2. Explicit ForeignKey Attribute on only one side of the relationship indicates outgoing.
            //3. Navigation property on only one side of the relationship indicates outgoing.
            //4. Column Attribute with Name on only one side of the relationship indicates outgoing.
            //5. Implicit ForeignKey Attribute on only one side of the relationship indicates outgoing.
            //6. Key attributed properties in a relationship class arranged in descending order of their position in the class indicates outgoing.
            //   That is, the relationship arrow grows from the keyed property that appeared last in the class towards the one that appeared first.
            //   You can use an explicit column attribute order to rearrange this order, or to ensure an accurate arrangement in the first place.
            //   Column attribute order value must be 1 or greater.
            //7. Foreign Key attributed properties in a relationship class arranged in descending order of their position in the class indicates outgoing.
            //8. Navigation properties in a relationship class arranged in descending order of their position in the class indicates outgoing.
            //9. Assume outgoing.

            RelationshipDirection? dir = null;

            //get the nav props
            var aNavProp = pattern.ANavProperty;
            var bNavProp = pattern.BNavProperty;

            //2-5
            if (aNavProp != null || bNavProp != null)
            {
                //2. Explicit ForeignKey Attribute on only one side of the relationship indicates outgoing.
                //first, get all matching foreign keys
                var aFks = aNavProp != null
                    ? pattern.ATypeInfo.ForeignKeyProperties
                        .Where(fk => fk.NavigationProperty == aNavProp).ToList()
                    : null;

                var bFks = bNavProp != null
                    ? pattern.BTypeInfo.ForeignKeyProperties
                        .Where(fk => fk.NavigationProperty == bNavProp).ToList()
                    : null;

                //pick the explicitly specified fks first
                object aObject = aFks?.Where(fk => !fk.IsAttributeAutoCreated).FirstOrDefault();
                object bObject = bFks?.Where(fk => !fk.IsAttributeAutoCreated).FirstOrDefault();

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
                        ((attr = aNavProp?.GetCustomAttribute(Defaults.ColumnType) as ColumnAttribute)?.Name)
                        ? attr
                        : null;
                    bObject = !string.IsNullOrWhiteSpace
                        ((attr = bNavProp?.GetCustomAttribute(Defaults.ColumnType) as ColumnAttribute)?.Name)
                        ? attr
                        : null;
                }

                //5. Implicit ForeignKey Attribute on only one side of the relationship indicates outgoing.
                if (!foundSomething())
                {
                    //try both implicit and explicit fks now
                    aObject = aFks?.FirstOrDefault();
                    bObject = bFks?.FirstOrDefault();
                }

                if (foundSomething())
                    //we need just one object
                    //if it is for A, outgoing, else incoming.
                    dir = aObject != null ? RelationshipDirection.Outgoing : RelationshipDirection.Incoming;
            }

            //6-8
            if (dir == null && pattern.HasRType)
            {
                var knownType = pattern.HasAType ? pattern.AType : pattern.HasBType ? pattern.BType : null;

                if (knownType != null)
                {
                    //this method assign a direction if found via the rules 5 - 7 above
                    var otherType = GetOtherNodeFromRAndKnownNode
                    (pattern.EntityService, pattern.RType, knownType,
                        knownType != pattern.AType, ref dir);

                    if (otherType == null)
                        //then don't trust the direction returned
                        dir = null;
                }
            }

            //9. Assume outgoing.
            if (dir == null)
                //all else failed
                //we tried
                //this is our last resort
                dir = RelationshipDirection.Outgoing;

            return dir;
        }

        internal static void ResolveInternalUtilities(Pattern pattern)
        {
            if (pattern.Client == null
                || pattern.Serializer == null
                || pattern.Resolver == null && pattern.Converter == null
                || pattern.AnnotationsContext == null)
            {
                var queryContext = CypherUtilities.GetQueryContext(pattern.CypherQuery);
                queryContext.CurrentBuildStrategy = pattern.BuildStrategy;
                pattern.QueryContext = queryContext;
            }
        }

        internal static JObject GetFinalProperties(Pattern pattern,
            LambdaExpression properties, LambdaExpression constraints, Type type, out bool hasFunctions)
        {
            ResolveInternalUtilities(pattern);

            var lambdaExpr = properties;

            if (lambdaExpr == null && constraints != null)
                lambdaExpr = CypherUtilities.GetConstraintsAsPropertiesLambda(constraints, type);

            return CypherUtilities.GetFinalProperties(lambdaExpr, pattern.QueryContext, out hasFunctions);
        }

        internal static object GetFinalObject(Pattern pattern,
            LambdaExpression properties, LambdaExpression constraints, Type type)
        {
            if (properties != null)
                try
                {
                    return (properties as Expression<Func<object>>)?.Compile().Invoke(); //ExecuteExpression<object>();
                }
                catch
                {
                }

            return null;
        }
    }
}