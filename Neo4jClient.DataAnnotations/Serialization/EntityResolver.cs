using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;

namespace Neo4jClient.DataAnnotations.Serialization
{
    public class EntityResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var _props = base.CreateProperties(type, memberSerialization);

            if (Neo4jAnnotations.ContainsEntityType(type))
            {
                var isComplex = type.IsComplex();

                var properties = new JsonPropertyCollection(type);

                foreach (var prop in _props)
                {
                    if (isComplex)
                    {
                        prop.NullValueHandling = NullValueHandling.Include; //we need null values for complex types
                        prop.DefaultValueHandling = DefaultValueHandling.Include; //we need all properties serialized
                    }

                    properties.Add(prop);
                }

                var typeInfo = Neo4jAnnotations.GetEntityTypeInfo(type);

                //check for complextypes
                var complexTypedProperties = typeInfo.ComplexTypedProperties;

                var complexJsonProperties = new List<JsonProperty>();

                if (complexTypedProperties?.Count > 0)
                {
                    //filter to complexproperties
                    var filteredJsonProperties = properties?
                    .Select(p => new
                    {
                        JsonProperty = p,
                        PropertyInfo = complexTypedProperties.Where(pi => pi.Name == p.UnderlyingName).FirstOrDefault()
                    })
                    .Where(np => np.PropertyInfo != null)
                    .ToDictionary(np => np.JsonProperty, np => np.PropertyInfo);

                    //generate new properties with new names for the complex types
                    foreach (var complexTypedJsonProp in filteredJsonProperties)
                    {
                        //get the complexTypedProperty's own jsonproperties
                        //include derived classes
                        var derivedTypes = Neo4jAnnotations.GetDerivedEntityTypes(complexTypedJsonProp.Key.PropertyType)?
                            .Where(t => t.IsComplex()).ToList();

                        if (derivedTypes == null || derivedTypes.Count == 0)
                        {
                            Neo4jAnnotations.AddEntityType(complexTypedJsonProp.Key.PropertyType);
                            derivedTypes = new List<Type>() { complexTypedJsonProp.Key.PropertyType };
                        }

                        var childProperties = derivedTypes
                            .SelectMany(dt => (ResolveContract(dt) as JsonObjectContract)?
                                .Properties?.Where(p => !p.Ignored && p.PropertyName != Defaults.MetadataPropertyName)
                                ?? new JsonProperty[0], 
                                (dt, property) =>
                                new
                                {
                                    DerivedType = dt,
                                    Property = property
                                })
                            .Where(jp => jp.Property != null)
                            .GroupBy(jp => jp.Property.PropertyName)
                            .Select(jpg => jpg.FirstOrDefault())
                            .ToList();

                        foreach (var childProp in childProperties)
                        {
                            //add the child to this type's properties
                            try
                            {
                                var newChildProp = GetComplexTypedPropertyChild(childProp.DerivedType, complexTypedJsonProp.Key, childProp.Property);
                                properties.AddProperty(newChildProp);
                                complexJsonProperties.Add(newChildProp);
                            }
                            catch (JsonSerializationException e)
                            {
                                //for some reason member already exists and is duplicate
                            }
                        }

                        //ignore all complex typed properties
                        complexTypedJsonProp.Key.Ignored = true;
                    }
                }

                int defaultIdx = -1;
                _props = properties.OrderBy(p => p.Order ?? defaultIdx++).ToList();

                //create metadata property and add it last
                var metadataJsonProperty = CreateProperty
                    (typeof(Defaults).GetProperty("DummyMetadataProperty", 
                    BindingFlags.Static | BindingFlags.Public), memberSerialization);

                metadataJsonProperty.PropertyName = Defaults.MetadataPropertyName;
                metadataJsonProperty.ValueProvider = new MetadataValueProvider(type, complexJsonProperties);
                metadataJsonProperty.ShouldSerialize = (instance) =>
                {
                    return !(metadataJsonProperty.ValueProvider as MetadataValueProvider)
                    .BuildMetadata(instance).IsEmpty();
                };

                _props.Add(metadataJsonProperty);

                //assign and resolve these properties
                typeInfo.JsonResolver = this;
                typeInfo.JsonProperties = new List<JsonProperty>(_props);
            }

            return _props;
        }

        protected virtual JsonProperty GetJsonPropertyDuplicate(JsonProperty prop)
        {
            var newProp = new JsonProperty()
            {
                AttributeProvider = prop.AttributeProvider,
                Converter = prop.Converter,
                DeclaringType = prop.DeclaringType,
                DefaultValue = prop.DefaultValue,
                DefaultValueHandling = prop.DefaultValueHandling,
                GetIsSpecified = prop.GetIsSpecified,
                HasMemberAttribute = prop.HasMemberAttribute,
                Ignored = prop.Ignored,
                IsReference = prop.IsReference,
                ItemConverter = prop.ItemConverter,
                ItemIsReference = prop.ItemIsReference,
                ItemReferenceLoopHandling = prop.ItemReferenceLoopHandling,
                ItemTypeNameHandling = prop.ItemTypeNameHandling,
                MemberConverter = prop.MemberConverter,
                NullValueHandling = prop.NullValueHandling,
                ObjectCreationHandling = prop.ObjectCreationHandling,
                Order = prop.Order,
                PropertyName = prop.PropertyName,
                PropertyType = prop.PropertyType,
                Readable = prop.Readable,
                ReferenceLoopHandling = prop.ReferenceLoopHandling,
                Required = prop.Required,
                SetIsSpecified = prop.SetIsSpecified,
                ShouldDeserialize = prop.ShouldDeserialize,
                ShouldSerialize = prop.ShouldSerialize,
                TypeNameHandling = prop.TypeNameHandling,
                UnderlyingName = prop.UnderlyingName,
                ValueProvider = prop.ValueProvider,
                Writable = prop.Writable

            };

            return newProp;
        }

        protected virtual JsonProperty GetComplexTypedPropertyChild(Type complexType, 
            JsonProperty complexTypedProperty, JsonProperty child)
        {
            var newChild = GetJsonPropertyDuplicate(child);

            //set complex name
            newChild.PropertyName = $"{complexTypedProperty.PropertyName}{Defaults.ComplexTypeNameSeparator}{child.PropertyName}";

            //set new value provider
            newChild.ValueProvider = new ComplexTypedPropertyValueProvider
                (complexTypedProperty.UnderlyingName, complexTypedProperty.PropertyType, 
                complexTypedProperty.DeclaringType, complexTypedProperty.ValueProvider,
                child.PropertyType, child.ValueProvider);

            //do this to avoid enclosing the jsonproperties in the closures.
            var parentActualName = complexTypedProperty.UnderlyingName;
            var parentType = (child.ValueProvider as ComplexTypedPropertyValueProvider)?.DeclaringType ?? child.DeclaringType;
            var parentReflectedType = complexType;

            var childShouldSerialize = child.ShouldSerialize;
            var childShouldDeserialize = child.ShouldDeserialize;

            //set a shouldserialize and shoulddeserialize
            newChild.ShouldSerialize = (entity) =>
            {
                var propertyInfo = entity.GetType().GetProperty(parentActualName);
                var parentValue = propertyInfo.GetValue(entity);
                Utilities.CheckIfComplexTypeInstanceIsNull(parentValue, parentActualName, propertyInfo.DeclaringType);

                var parentValueType = parentValue.GetType();

                var isAssignable = parentType.IsGenericAssignableFrom(parentValueType);
                isAssignable = isAssignable || parentReflectedType == parentValueType;

                return isAssignable && (childShouldSerialize == null || childShouldSerialize(parentValue) == true);
            };

            return newChild;
        }
    }
}
