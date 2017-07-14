using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.DataAnnotations
{
    public class EntityTypeInfo
    {
        public EntityTypeInfo(Type type)
        {
            Type = type;
        }

        public Type Type { get; }

        public List<string> Labels
        {
            get
            {
                var labels = LabelsWithTypeNameCatch;

                if(labels.Count == 1)
                {
                    //check if the single label is the type name
                    if(labels[0] == Type.Name)
                    {
                        return new List<string>();
                    }
                }

                return labels;
            }
        }

        private List<string> labelsWithTypeNameCatch;

        public List<string> LabelsWithTypeNameCatch
        {
            get
            {
                return labelsWithTypeNameCatch ?? (labelsWithTypeNameCatch = Utilities.GetLabels(Type, useTypeNameIfEmpty: true));
            }
        }

        private List<PropertyInfo> allProperties;

        public List<PropertyInfo> AllProperties
        {
            get
            {
                return allProperties ?? 
                    (allProperties = Type.GetProperties
                    (Defaults.MemberSearchBindingFlags)?.ToList() 
                    ?? new List<PropertyInfo>());
            }
        }

        private Dictionary<string, Dictionary<PropertyInfo, IEnumerable<Attribute>>> attrTypeToPropsToAttrs
            = new Dictionary<string, Dictionary<PropertyInfo, IEnumerable<Attribute>>>();

        public Dictionary<PropertyInfo, IEnumerable<Attribute>> GetPropertiesWithAttribute(Type attributeType, bool inherit = false)
        {
            Dictionary<PropertyInfo, IEnumerable<Attribute>> props;
            if (!attrTypeToPropsToAttrs.TryGetValue(attributeType.FullName, out props))
            {
                props = new Dictionary<PropertyInfo, IEnumerable<Attribute>>();

                var properties = AllProperties;
                foreach(var property in properties)
                {
                    var attrs = property.GetCustomAttributes(attributeType, inherit);

                    if(attrs != null && attrs.Count() > 0)
                    {
                        props[property] = attrs;
                    }
                }

                attrTypeToPropsToAttrs[attributeType.FullName] = props;
            }

            return props;
        }

        private List<PropertyInfo> complexTypedProps;

        public List<PropertyInfo> ComplexTypedProperties
        {
            get
            {
                if (complexTypedProps != null)
                    return complexTypedProps;

                var typeOfComplexType = typeof(ComplexTypeAttribute);

                complexTypedProps = AllProperties
                    .Where(p => p.PropertyType.GetTypeInfo().IsDefined(typeOfComplexType))
                    .ToList();

                return complexTypedProps;
            }
        }

        private Dictionary<string, MemberInfo> jsonNamePropertyMap;

        public Dictionary<string, MemberInfo> JsonNamePropertyMap
        {
            get { return jsonNamePropertyMap ?? (jsonNamePropertyMap = new Dictionary<string, MemberInfo>()); }
            private set { jsonNamePropertyMap = value; }
        }

        //public List<string> IgnoredJsonProperties { get; private set; } = new List<string>();

        private List<ForeignKeyProperty> foreignKeyProperties;

        public List<ForeignKeyProperty> ForeignKeyProperties
        {
            get
            {
                //apply conventions to get actual foreignkeys

                if (foreignKeyProperties != null)
                    return foreignKeyProperties;

                foreignKeyProperties = new List<ForeignKeyProperty>();

                var propsConsidered = new List<string>();

                var foreignKeyedProps = GetPropertiesWithAttribute(typeof(ForeignKeyAttribute));

                foreach (var foreignKeyedProp in foreignKeyedProps)
                {
                    var foreignKeyAttribute = foreignKeyedProp.Value.FirstOrDefault() as ForeignKeyAttribute;

                    if (foreignKeyAttribute == null)
                        continue;

                    var foreignKeyProperty = new ForeignKeyProperty()
                    {
                        Attribute = foreignKeyAttribute
                    };

                    AssignForeignKey(foreignKeyProperty, foreignKeyedProp.Key);
                    propsConsidered.Add(foreignKeyedProp.Key.Name);

                    //determine if the foreignkeyattribute name points to another property on this object
                    var otherPropertyInfo = AllProperties.Where(p => p.Name == foreignKeyAttribute.Name).SingleOrDefault();

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

                var propertiesNotConsidered = AllProperties.Where(p => !propsConsidered.Contains(p.Name)).ToList();

                foreach (var propInfo in propertiesNotConsidered)
                {
                    if ((propInfo.Name.EndsWith("Id") || propInfo.Name.ToLower().EndsWith("_id"))
                        && Utilities.IsTypeScalar(propInfo.PropertyType)) //it must be scalar to be considered
                    {
                        var fkName = propInfo.Name.Substring(0, propInfo.Name.Length - 2).TrimEnd('_');

                        //check if this name belongs to a nav property already chosen as foreign key above
                        //but whose defined foreignkey does not point to its scalar property.
                        ForeignKeyProperty foreignKeyProperty = foreignKeyProperties
                            .Where(fk => fk.NavigationProperty?.Name == fkName).SingleOrDefault();

                        if (foreignKeyProperty == null)
                        {
                            var foreignKeyAttribute = new ForeignKeyAttribute(fkName);

                            foreignKeyProperty = new ForeignKeyProperty()
                            {
                                Attribute = foreignKeyAttribute,
                                IsAttributeAutoCreated = true
                            };

                            foreignKeyProperties.Add(foreignKeyProperty);

                            //determine if the foreignkeyattribute name points to another property on this object
                            var otherPropertyInfo = propertiesNotConsidered.Where(p => p.Name == fkName).SingleOrDefault();

                            if (otherPropertyInfo != null)
                            {
                                //assign the nav property
                                AssignForeignKey(foreignKeyProperty, otherPropertyInfo);
                            }
                        }

                        //assign the scalar property
                        AssignForeignKey(foreignKeyProperty, propInfo);
                    }
                }

                return foreignKeyProperties;
            }
        }

        private void AssignForeignKey(ForeignKeyProperty property, PropertyInfo propertyInfo)
        {
            //check if its scalar
            if (Utilities.IsTypeScalar(propertyInfo.PropertyType))
            {
                //only assign if scalar property hasn't been earlier on assigned.
                //if it has been assigned, skip. it may mean the propertyinfo has nothing to do with foreignkeys and was just a coincedence.
                property.ScalarProperty = property.ScalarProperty ?? propertyInfo; 
            }
            else
            {
                //its navigation prop
                property.NavigationProperty = propertyInfo;
            }

            property.IsAttributePointingToProperty = property.NavigationProperty?.Name == property.Attribute?.Name
                || property.ScalarProperty?.Name == property.Attribute?.Name;
        }

        private List<PropertyInfo> navigationProps;

        public List<PropertyInfo> NavigationableProperties
        {
            get
            {
                return navigationProps ?? (navigationProps = 
                    AllProperties
                    .Where(p => !Utilities.IsTypeScalar(p.PropertyType))
                    .Except(ComplexTypedProperties)
                    .ToList());
            }
        }

        private Dictionary<Type, List<ForeignKeyProperty>> attributedForeignKeys 
            = new Dictionary<Type, List<ForeignKeyProperty>>();

        /// <summary>
        /// Gets properties that have specific attribute on either the scalar or nav property of a foreign key.
        /// </summary>
        public List<ForeignKeyProperty> GetForeignKeysWithAttribute(Type attributeType)
        {
            List<ForeignKeyProperty> props;

            if (!attributedForeignKeys.TryGetValue(attributeType, out props))
            {
                props = ForeignKeyProperties.Where(fk => fk.ScalarProperty?.IsDefined(attributeType) == true
                || fk.NavigationProperty?.IsDefined(attributeType) == true)?.ToList() ?? new List<ForeignKeyProperty>();

                attributedForeignKeys[attributeType] = props;
            }

            return props;
        }

        public List<ForeignKeyProperty> ColumnAttributeFKs
        {
            get { return GetForeignKeysWithAttribute(Defaults.ColumnType); }
        }

        public DefaultContractResolver JsonResolver { get; internal set; }

        private JsonObjectContract JsonContract { get; set; }

        private List<JsonProperty> _jsonProps = null;

        public List<JsonProperty> JsonProperties
        {
            get
            {
                return _jsonProps;
            }
            internal set
            {
                if (value == null || _jsonProps == null || value.Count != _jsonProps.Count 
                    || value.Any(np => !_jsonProps.Contains(np)))
                {
                    //something has changed
                    _jsonProps = value;
                    ResolveJsonProperties();
                }
            }
        }

        public void WithJsonResolver(DefaultContractResolver resolver)
        {
            if (JsonResolver != resolver)
            {
                var contract = resolver?.ResolveContract(Type) as JsonObjectContract;

                if (contract == null || contract.Properties == null || contract.Properties.Count == 0)
                {
                    throw new InvalidOperationException(string.Format(Messages.NoContractResolvedError, Type.FullName));
                }

                JsonResolver = resolver;
                JsonContract = contract;

                JsonProperties = new List<JsonProperty>(contract.Properties);
            }
        }

        internal void ResolveJsonProperties()
        {
            //map the properties
            MapJsonProperties();

            //ignore properties that have been marked with relevant attributes
            IgnoreJsonProperties();
        }

        internal void MapJsonProperties()
        {
            JsonNamePropertyMap = JsonProperties?
                    .Select(p => new
                    {
                        JsonProperty = p,
                        MemberInfo = p.DeclaringType.GetMember(p.UnderlyingName, Defaults.MemberSearchBindingFlags)
                        .Where(m => m.IsEquivalentTo(p.UnderlyingName, p.DeclaringType, p.PropertyType)).FirstOrDefault()
                    })
                    .Where(np => np.MemberInfo != null)
                    .ToDictionary(np => np.JsonProperty.PropertyName, np => np.MemberInfo);
        }

        internal void IgnoreJsonProperties()
        {
            if (JsonProperties != null)
            {
                //ignore all nav properties
                var navProps = NavigationableProperties;
                var allPropsToIgnore = new List<PropertyInfo>(navProps);

                foreach (var property in JsonProperties)
                {
                    if (allPropsToIgnore.Any(p => p.Name == property.UnderlyingName))
                    {
                        property.Ignored = true;
                        //IgnoredJsonProperties.Add(property.PropertyName);
                    }
                    else
                    {
                        //IgnoredJsonProperties.Remove(property.PropertyName);
                    }
                }

                //IgnoredJsonProperties = IgnoredJsonProperties.Distinct().ToList();
            }
        }
    }
}
