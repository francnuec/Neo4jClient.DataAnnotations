using Newtonsoft.Json.Serialization;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;

namespace Neo4jClient.DataAnnotations.Serialization
{
    public class EntityResolver : DefaultContractResolver, IHaveAnnotationsContext
    {
        public EntityResolver()
        {
            DeserializeConverter = new EntityResolverConverter(this);
        }

        public virtual AnnotationsContext AnnotationsContext { get; internal set; }

        public virtual EntityResolverConverter DeserializeConverter { get; }

        public EntityService EntityService => AnnotationsContext.EntityService;

        //protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        //{
        //    var _props = base.CreateProperties(type, memberSerialization);

        //    if (EntityService.ContainsEntityType(type))
        //    {
        //        var isComplex = type.IsComplex();

        //        var properties = new JsonPropertyCollection(type);

        //        foreach (var prop in _props)
        //        {
        //            if (isComplex)
        //            {
        //                prop.NullValueHandling = NullValueHandling.Include; //we need null values for complex types
        //                prop.DefaultValueHandling = DefaultValueHandling.Include; //we need all properties serialized
        //            }

        //            properties.Add(prop);
        //        }

        //        var typeInfo = EntityService.GetEntityTypeInfo(type);

        //        lock (typeInfo)
        //        {
        //            //check for complextypes
        //            var complexTypedProperties = typeInfo.ComplexTypedProperties;

        //            var complexJsonProperties = new List<JsonProperty>();

        //            if (complexTypedProperties?.Count > 0)
        //            {
        //                //filter to complexproperties
        //                var filteredJsonProperties = properties?
        //                .Select(p => new
        //                {
        //                    JsonProperty = p,
        //                    PropertyInfo = complexTypedProperties.Where(pi => pi.Name == p.UnderlyingName).FirstOrDefault()
        //                })
        //                .Where(np => np.PropertyInfo != null)
        //                .ToDictionary(np => np.JsonProperty, np => np.PropertyInfo);

        //                //generate new properties with new names for the complex types
        //                foreach (var complexTypedJsonProp in filteredJsonProperties)
        //                {
        //                    //get the complexTypedProperty's own jsonproperties
        //                    //include derived classes
        //                    var derivedTypes = EntityService.GetDerivedEntityTypes(complexTypedJsonProp.Key.PropertyType)?
        //                        .Where(t => t.IsComplex()).ToList();

        //                    if (derivedTypes == null || derivedTypes.Count == 0)
        //                    {
        //                        EntityService.AddEntityType(complexTypedJsonProp.Key.PropertyType);
        //                        derivedTypes = new List<Type>() { complexTypedJsonProp.Key.PropertyType };
        //                    }

        //                    var childProperties = derivedTypes
        //                        .SelectMany(dt => (ResolveContract(dt) as JsonObjectContract)?
        //                            .Properties?.Where(p => !p.Ignored && p.PropertyName != Defaults.MetadataPropertyName)
        //                            ?? new JsonProperty[0],
        //                            (dt, property) =>
        //                            new
        //                            {
        //                                DerivedType = dt,
        //                                Property = property
        //                            })
        //                        .Where(jp => jp.Property != null)
        //                        .GroupBy(jp => jp.Property.PropertyName)
        //                        .Select(jpg => jpg.FirstOrDefault())
        //                        .ToList();

        //                    foreach (var childProp in childProperties)
        //                    {
        //                        //add the child to this type's properties
        //                        try
        //                        {
        //                            var newChildProp = GetComplexTypedPropertyChild
        //                                (childProp.DerivedType, complexTypedJsonProp.Key,
        //                                complexTypedJsonProp.Value, childProp.Property);

        //                            properties.AddProperty(newChildProp);
        //                            complexJsonProperties.Add(newChildProp);
        //                        }
        //                        catch (JsonSerializationException e)
        //                        {
        //                            //for some reason member already exists and is duplicate
        //                        }
        //                    }

        //                    //ignore all complex typed properties
        //                    complexTypedJsonProp.Key.Ignored = true;
        //                }
        //            }

        //            int defaultIdx = -1;
        //            _props = properties.OrderBy(p => p.Order ?? defaultIdx++).ToList();

        //            //create metadata property and add it last
        //            var metadataJsonProperty = CreateProperty(Defaults.DummyMetadataPropertyInfo, memberSerialization);
        //            metadataJsonProperty.PropertyName = Defaults.MetadataPropertyName;
        //            metadataJsonProperty.ValueProvider = new MetadataValueProvider(type, complexJsonProperties);
        //            metadataJsonProperty.ShouldSerialize = (instance) =>
        //            {
        //                return !(metadataJsonProperty.ValueProvider as MetadataValueProvider)
        //                .BuildMetadata(instance).IsEmpty();
        //            };

        //            _props.Add(metadataJsonProperty);

        //            //assign and resolve these properties
        //            typeInfo.JsonResolver = this;
        //            typeInfo.JsonProperties = new List<JsonProperty>(_props);
        //        }
        //    }

        //    return _props;
        //}

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);

            SerializationUtilities.ResolveEntityProperties(props, type, EntityService.GetEntityTypeInfo(type), EntityService, this, 
                (propInfo) => CreateProperty(propInfo, memberSerialization));

            return props;
        }

        //protected virtual EntityJsonProperty GetJsonPropertyDuplicate(JsonProperty prop)
        //{
        //    var newProp = new EntityJsonProperty(prop);
        //    return newProp;
        //}

        //protected virtual JsonProperty GetComplexTypedPropertyChild(Type complexType, 
        //    JsonProperty complexTypedJsonProperty, PropertyInfo complexTypedPropertyInfo, JsonProperty child)
        //{
        //    var newChild = GetJsonPropertyDuplicate(child);

        //    if (complexTypedJsonProperty.Ignored)
        //        newChild.Ignored = true; //this ensures that when a complex field is ignored, all its children are ignored too.

        //    //set complex name
        //    newChild.SimplePropertyName = child.PropertyName;
        //    newChild.PropertyName = $"{complexTypedJsonProperty.PropertyName}{Defaults.ComplexTypeNameSeparator}{child.PropertyName}";
        //    newChild.ComplexUnderlyingName = $"{complexTypedJsonProperty.GetComplexOrSimpleUnderlyingName()}{Defaults.ComplexTypeNameSeparator}{child.UnderlyingName}";

        //    //set new value provider
        //    newChild.ValueProvider = new ComplexTypedPropertyValueProvider
        //        (complexTypedJsonProperty.UnderlyingName, complexTypedJsonProperty.PropertyType, 
        //        complexTypedJsonProperty.DeclaringType, complexTypedJsonProperty.ValueProvider,
        //        child.PropertyType, child.ValueProvider);

        //    //do this to avoid enclosing the jsonproperties in the closures.
        //    var parentActualName = complexTypedJsonProperty.UnderlyingName;
        //    var parentType = (child.ValueProvider as ComplexTypedPropertyValueProvider)?.DeclaringType ?? child.DeclaringType;
        //    var parentReflectedType = complexType;

        //    var childShouldSerialize = child.ShouldSerialize;
        //    //var childShouldDeserialize = child.ShouldDeserialize;

        //    //set a shouldserialize and shoulddeserialize
        //    newChild.ShouldSerialize = (entity) =>
        //    {
        //        var propertyInfo = entity.GetType().GetProperty(parentActualName);
        //        var parentValue = propertyInfo.GetValue(entity);

        //        Utils.Utilities.CheckIfComplexTypeInstanceIsNull(parentValue, parentActualName, propertyInfo.DeclaringType);

        //        var parentValueType = parentValue.GetType();

        //        var isAssignable = parentType.IsGenericAssignableFrom(parentValueType);
        //        isAssignable = isAssignable || parentReflectedType == parentValueType;

        //        return isAssignable && (childShouldSerialize == null || childShouldSerialize(parentValue) == true);
        //    };

        //    return newChild;
        //}
    }
}
