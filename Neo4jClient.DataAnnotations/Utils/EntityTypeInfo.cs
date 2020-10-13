using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Neo4jClient.DataAnnotations.Serialization;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.DataAnnotations.Utils
{
    public sealed class EntityTypeInfo : IHaveEntityService
    {
        //private JsonObjectContract JsonContract { get; set; }
        private List<JsonProperty> _jsonProps;
        private List<PropertyInfo> allProperties;

        private readonly ConcurrentDictionary<Type, List<ForeignKeyProperty>> attributedForeignKeys
            = new ConcurrentDictionary<Type, List<ForeignKeyProperty>>();

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<PropertyInfo, IEnumerable<Attribute>>>
            attrTypeToPropsToAttrs
                = new ConcurrentDictionary<string, ConcurrentDictionary<PropertyInfo, IEnumerable<Attribute>>>();

        private List<PropertyInfo> complexTypedProps;
        private List<ForeignKeyProperty> foreignKeyProperties;
        private Dictionary<MemberName, MemberInfo> jsonNamePropertyMap;
        private List<string> labelsWithTypeNameCatch;
        private List<PropertyInfo> navigationProps;
        private List<PropertyInfo> notMappedProps;

        public EntityTypeInfo(Type type, EntityService service)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            EntityService = service ?? throw new ArgumentNullException(nameof(service));
        }

        private static MethodInfo CreatePropertyMethodInfo { get; }
            = typeof(DefaultContractResolver).GetMethod("CreateProperty",
                BindingFlags.NonPublic | BindingFlags.Instance);

        public Type Type { get; }

        public List<string> Labels
        {
            get
            {
                var labels = LabelsWithTypeNameCatch;

                if (labels.Count == 1
                    && labels[0] == Type.Name //check if the single label is the type name
                )
                    return new List<string>();

                return labels;
            }
        }

        public List<string> LabelsWithTypeNameCatch
        {
            get
            {
                if (labelsWithTypeNameCatch == null)
                    lock (this)
                    {
                        if (labelsWithTypeNameCatch == null) labelsWithTypeNameCatch = Utilities.GetLabels(Type);
                    }

                return labelsWithTypeNameCatch;
            }
        }

        public List<PropertyInfo> AllProperties
        {
            get
            {
                lock (this)
                {
                    if (allProperties == null)
                        allProperties = Type.GetProperties
                                            (Defaults.MemberSearchBindingFlags)?.ToList()
                                        ?? new List<PropertyInfo>();
                }

                return allProperties;
            }
        }

        public List<PropertyInfo> NavigationableProperties
        {
            get
            {
                lock (this)
                {
                    if (navigationProps == null)
                        navigationProps = AllProperties
                            .Where(p => !Utilities.IsScalarType(p.PropertyType, EntityService))
                            .Except(ComplexTypedProperties)
                            .ToList();
                }

                return navigationProps;
            }
        }

        public List<PropertyInfo> ComplexTypedProperties
        {
            get
            {
                lock (this)
                {
                    if (complexTypedProps == null)
                        complexTypedProps = AllProperties
                            .Where(p => p.PropertyType.IsComplex())
                            .ToList();
                }

                return complexTypedProps;
            }
        }

        public List<ForeignKeyProperty> ForeignKeyProperties
        {
            get
            {
                //apply conventions to get actual foreignkeys

                if (foreignKeyProperties != null)
                    return foreignKeyProperties;

                lock (this)
                {
                    if (foreignKeyProperties == null)
                    {
                        foreignKeyProperties = new List<ForeignKeyProperty>();

                        var propsConsidered = new List<string>();

                        var foreignKeyedProps = GetPropertiesWithAttribute(typeof(ForeignKeyAttribute));

                        foreach (var foreignKeyedProp in foreignKeyedProps)
                        {
                            var foreignKeyAttribute = foreignKeyedProp.Value.FirstOrDefault() as ForeignKeyAttribute;

                            if (foreignKeyAttribute == null)
                                continue;

                            var foreignKeyProperty = new ForeignKeyProperty
                            {
                                Attribute = foreignKeyAttribute
                            };

                            AssignForeignKey(foreignKeyProperty, foreignKeyedProp.Key);
                            propsConsidered.Add(foreignKeyedProp.Key.Name);

                            //determine if the foreignkeyattribute name points to another property on this object
                            var otherPropertyInfo = AllProperties.Where(p => p.Name == foreignKeyAttribute.Name)
                                .SingleOrDefault();

                            if (otherPropertyInfo != null)
                            {
                                AssignForeignKey(foreignKeyProperty, otherPropertyInfo);
                                propsConsidered.Add(otherPropertyInfo.Name);
                            }

                            foreignKeyProperties.Add(foreignKeyProperty);
                        }

                        //check other properties on this entity by using conventions
                        //any property name that ends with "Id" or "_id" is expected to be the scalar foreign key
                        //and the name of its navigation property should equal the rest of the property name string with "Id" removed.

                        var propertiesNotConsidered =
                            AllProperties.Where(p => !propsConsidered.Contains(p.Name)).ToList();

                        foreach (var propInfo in propertiesNotConsidered)
                        {
                            string fkName = null;

                            if ((propInfo.Name.EndsWith("Id") || propInfo.Name.ToLower().EndsWith("_id"))
                                && Utilities.IsScalarType(propInfo.PropertyType,
                                    EntityService) //it must be scalar to be considered
                                && !string.IsNullOrWhiteSpace(fkName =
                                    propInfo.Name.Substring(0, propInfo.Name.Length - 2).TrimEnd('_')))
                            {
                                //check if this name belongs to a nav property already chosen as foreign key above
                                //but whose defined foreignkey does not point to its scalar property.
                                var foreignKeyProperty = foreignKeyProperties
                                    .Where(fk => fk.NavigationProperty?.Name == fkName).SingleOrDefault();

                                if (foreignKeyProperty == null)
                                {
                                    var foreignKeyAttribute = new ForeignKeyAttribute(fkName);

                                    foreignKeyProperty = new ForeignKeyProperty
                                    {
                                        Attribute = foreignKeyAttribute,
                                        IsAttributeAutoCreated = true
                                    };

                                    foreignKeyProperties.Add(foreignKeyProperty);

                                    //determine if the foreignkeyattribute name points to another property on this object
                                    var otherPropertyInfo = propertiesNotConsidered.Where(p => p.Name == fkName)
                                        .SingleOrDefault();

                                    if (otherPropertyInfo != null)
                                        //assign the nav property
                                        AssignForeignKey(foreignKeyProperty, otherPropertyInfo);
                                }

                                //assign the scalar property
                                AssignForeignKey(foreignKeyProperty, propInfo);
                            }
                        }
                    }
                }

                return foreignKeyProperties;
            }
        }

        public List<PropertyInfo> NotMappedProperties
        {
            get
            {
                lock (this)
                {
                    if (notMappedProps == null)
                    {
                        var attr = Defaults.NotMappedType;

                        notMappedProps = AllProperties
                            .Where(p => p.IsDefined(attr))
                            .ToList();
                    }
                }

                return notMappedProps;
            }
        }


        internal List<JsonProperty> JsonProperties
        {
            get => _jsonProps;
            set
            {
                lock (this)
                {
                    if (value == null || _jsonProps == null || value.Count != _jsonProps.Count
                        || value.Any(np => !_jsonProps.Contains(np)))
                    {
                        //something has changed
                        _jsonProps = value;
                        ProcessJsonProperties();
                    }
                }
            }
        }

        internal IEnumerable<PropertyInfo> PropertiesToIgnore => NavigationableProperties.Union(NotMappedProperties);

        internal Dictionary<MemberName, MemberInfo> JsonNamePropertyMap
        {
            get
            {
                lock (this)
                {
                    if (jsonNamePropertyMap == null) jsonNamePropertyMap = new Dictionary<MemberName, MemberInfo>();
                }

                return jsonNamePropertyMap;
            }
            private set
            {
                lock (this)
                {
                    jsonNamePropertyMap = value;
                }
            }
        }

        internal List<ForeignKeyProperty> ColumnAttributeFKs => GetForeignKeysWithAttribute(Defaults.ColumnType);

        public EntityService EntityService { get; }

        //internal DefaultContractResolver JsonResolver { get; set; }

        /// <summary>
        ///     Gets properties that have specific attribute on either the scalar or nav property of a foreign key.
        /// </summary>
        internal List<ForeignKeyProperty> GetForeignKeysWithAttribute(Type attributeType)
        {
            List<ForeignKeyProperty> props;

            if (!attributedForeignKeys.TryGetValue(attributeType, out props))
            {
                props = ForeignKeyProperties.Where(fk => fk.ScalarProperty?.IsDefined(attributeType) == true
                                                         || fk.NavigationProperty?.IsDefined(attributeType) == true)
                    ?.ToList() ?? new List<ForeignKeyProperty>();

                attributedForeignKeys[attributeType] = props;
            }

            return props;
        }

        internal void AssignForeignKey(ForeignKeyProperty property, PropertyInfo propertyInfo)
        {
            //check if its scalar
            if (Utilities.IsScalarType(propertyInfo.PropertyType, EntityService))
                //only assign if scalar property hasn't been earlier on assigned.
                //if it has been assigned, skip. it may mean the propertyinfo has nothing to do with foreignkeys and was just a coincedence.
                property.ScalarProperty = property.ScalarProperty ?? propertyInfo;
            else
                //its navigation prop
                property.NavigationProperty = propertyInfo;

            property.IsAttributePointingToProperty = property.NavigationProperty?.Name == property.Attribute?.Name
                                                     || property.ScalarProperty?.Name == property.Attribute?.Name;
        }

        internal ConcurrentDictionary<PropertyInfo, IEnumerable<Attribute>> GetPropertiesWithAttribute(
            Type attributeType, bool inherit = false)
        {
            ConcurrentDictionary<PropertyInfo, IEnumerable<Attribute>> props;
            if (!attrTypeToPropsToAttrs.TryGetValue(attributeType.FullName, out props))
            {
                props = new ConcurrentDictionary<PropertyInfo, IEnumerable<Attribute>>();

                var properties = AllProperties;
                foreach (var property in properties)
                {
                    var attrs = property.GetCustomAttributes(attributeType, inherit)?.Cast<Attribute>();

                    if (attrs != null && attrs.Count() > 0) props[property] = attrs;
                }

                attrTypeToPropsToAttrs[attributeType.FullName] = props;
            }

            return props;
        }

        //internal void WithJsonResolver(DefaultContractResolver resolver)
        //{
        //    lock (this)
        //    {
        //        if (JsonResolver != resolver && (resolver == null || JsonResolver?.GetType() != resolver.GetType()))
        //        {
        //            var contract = resolver?.ResolveContract(Type) as JsonObjectContract;

        //            if (contract == null || contract.Properties == null || contract.Properties.Count == 0)
        //            {
        //                throw new InvalidOperationException(string.Format(Messages.NoContractResolvedError, Type.FullName));
        //            }

        //            JsonResolver = resolver;
        //            JsonContract = contract;

        //            JsonProperties = new List<JsonProperty>(contract.Properties);
        //        }
        //    }
        //}

        internal void ResolveJsonPropertiesUsing(DefaultContractResolver resolver)
        {
            lock (this)
            {
                var contract = resolver?.ResolveContract(Type) as JsonObjectContract;

                if (contract == null || contract.Properties == null) //|| contract.Properties.Count == 0)
                    throw new InvalidOperationException(string.Format(Messages.NoContractResolvedError, Type.FullName));

                SerializationUtilities.ResolveEntityProperties
                (contract.Properties, Type, this, EntityService, resolver,
                    propInfo => CreatePropertyMethodInfo.Invoke(resolver,
                        new object[] { propInfo, contract.MemberSerialization }) as JsonProperty);

                if (JsonProperties?.Count == 0) JsonProperties = new List<JsonProperty>(contract.Properties);
            }
        }

        internal void ProcessJsonProperties()
        {
            lock (this)
            {
                //map the properties
                MapJsonProperties();

                ////ignore properties that have been marked with relevant attributes
                //IgnoreJsonProperties();
            }
        }

        internal void MapJsonProperties()
        {
            JsonNamePropertyMap = JsonProperties?
                .Select(p => new
                {
                    JsonProperty = p,
                    MemberInfo = p.DeclaringType.GetMember(p.UnderlyingName, Defaults.MemberSearchBindingFlags)
                        .Where(m => m.IsEquivalentTo(p.UnderlyingName, p.DeclaringType, p.PropertyType))
                        .FirstOrDefault()
                })
                .Where(np => np.MemberInfo != null)
                .ToDictionary(np => new MemberName(
                    np.JsonProperty.UnderlyingName,
                    np.JsonProperty.GetComplexOrSimpleUnderlyingName(),
                    np.JsonProperty.GetSimpleOrComplexPropertyName(),
                    np.JsonProperty.PropertyName), np => np.MemberInfo);
        }

        internal void IgnoreJsonProperties()
        {
            if (JsonProperties != null)
            {
                //ignore all nav properties
                var allPropsToIgnore = PropertiesToIgnore.ToArray();

                foreach (var property in JsonProperties)
                    if (allPropsToIgnore.Any(p => p.Name == property.UnderlyingName))
                    {
                        property.Ignored = true;
                        //IgnoredJsonProperties.Add(property.PropertyName);
                    }

                //IgnoredJsonProperties = IgnoredJsonProperties.Distinct().ToList();
            }
        }

        public override string ToString()
        {
            try
            {
                return $"Entity: {Type}";
            }
            catch
            {
            }

            return base.ToString();
        }
    }
}