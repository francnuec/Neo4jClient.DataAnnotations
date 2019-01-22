using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Neo4jClient.DataAnnotations.Serialization
{
    public static class SerializationUtilities
    {
        public static string SerializeMetadata(Metadata metadata)
        {
            //only distinct values
            metadata.NullProperties = metadata.NullProperties.Distinct().ToList();
            return JsonConvert.SerializeObject(metadata);
        }

        public static Metadata DeserializeMetadata(string metadataJson)
        {
            return JsonConvert.DeserializeObject<Metadata>(metadataJson);
        }

        internal static void EnsureRightJObject(ref JObject valueJObject)
        {
            //the neo4jclient guys really messed things up here
            //so use heuristics to determine if we are passing the right data or not, and then get the right data
            //this is for deserialization only

            //example json received
            /*
             {
              "extensions": {},
              "metadata": {
                "id": 176,
                "labels": [
                  "IdentityUser"
                ]
              },
              "paged_traverse": "http://localhost:7474/db/data/node/176/paged/traverse/{returnType}{?pageSize,leaseTime}",
              "outgoing_relationships": "http://localhost:7474/db/data/node/176/relationships/out",
              "outgoing_typed_relationships": "http://localhost:7474/db/data/node/176/relationships/out/{-list|&|types}",
              "labels": "http://localhost:7474/db/data/node/176/labels",
              "create_relationship": "http://localhost:7474/db/data/node/176/relationships",
              "traverse": "http://localhost:7474/db/data/node/176/traverse/{returnType}",
              "all_relationships": "http://localhost:7474/db/data/node/176/relationships/all",
              "all_typed_relationships": "http://localhost:7474/db/data/node/176/relationships/all/{-list|&|types}",
              "property": "http://localhost:7474/db/data/node/176/properties/{key}",
              "self": "http://localhost:7474/db/data/node/176",
              "incoming_relationships": "http://localhost:7474/db/data/node/176/relationships/in",
              "properties": "http://localhost:7474/db/data/node/176/properties",
              "incoming_typed_relationships": "http://localhost:7474/db/data/node/176/relationships/in/{-list|&|types}",
              "data": {
                actual data ...
              }
            } 
             */

            var expectedProps = new Dictionary<string, JTokenType>()
            {
                //{ "data", JTokenType.Object },
                { "metadata", JTokenType.Object },
                { "self", JTokenType.String },
            };

            var jObject = valueJObject;

            if (expectedProps.All(prop => jObject[prop.Key]?.Type == prop.Value))
            {
                //hopefully we are right
                //replace the jObject with "data"
                valueJObject = jObject["data"] as JObject;
            }
        }

        internal static void EnsureSerializerInstance(ref JsonSerializer serializer)
        {
            if (serializer == null)
            {
                //this is strange, but the Neo4jClient folks forgot to pass a serializer to this method
                serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings()
                {
                    Converters = GraphClient.DefaultJsonConverters?.Reverse().ToList(),
                    ContractResolver = GraphClient.DefaultJsonContractResolver,
                    ObjectCreationHandling = ObjectCreationHandling.Auto,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                });
            }
        }

        internal static void RemoveThisConverter(Type converterType, JsonSerializer serializer, out List<Tuple<JsonConverter, int>> convertersRemoved)
        {
            convertersRemoved = new List<Tuple<JsonConverter, int>>();

            for (int i = 0, l = serializer.Converters.Count; i < l; i++)
            {
                var converter = serializer.Converters[i];
                if (converterType.IsAssignableFrom(converter.GetType()))
                {
                    convertersRemoved.Add(new Tuple<JsonConverter, int>(converter, i));
                }
            }

            foreach (var converter in convertersRemoved)
            {
                serializer.Converters.Remove(converter.Item1);
            }
        }

        internal static void RestoreThisConverter(JsonSerializer serializer, List<Tuple<JsonConverter, int>> convertersRemoved,
            bool clearConvertersRemovedList = true)
        {
            foreach (var converter in convertersRemoved)
            {
                try
                {
                    serializer.Converters.Insert(converter.Item2, converter.Item1);
                }
                catch
                {
                    serializer.Converters.Add(converter.Item1);
                }
            }

            if (clearConvertersRemovedList)
                convertersRemoved.Clear();
        }

        internal static bool ResolveEntityProperties(IList<JsonProperty> jsonProperties, 
            Type entityType, EntityTypeInfo entityTypeInfo,
            EntityService entityService, DefaultContractResolver resolver,
            Func<PropertyInfo, JsonProperty> createPropertyFunc)
        {
            if (entityService.ContainsEntityType(entityType))
            {
                if (jsonProperties.Any(jp => jp.PropertyName == Defaults.MetadataPropertyName
                    && jp.UnderlyingName == Defaults.DummyMetadataPropertyInfo.Name))
                    return false; //if we ever find the metadata property there, assume it has been resolved.

                var _properties = new JsonPropertyCollection(entityType);

                var isComplex = entityType.IsComplex();
                var propertiesToIgnore = entityTypeInfo.PropertiesToIgnore.ToArray();

                foreach (var jsonProp in jsonProperties)
                {
                    if (isComplex)
                    {
                        jsonProp.NullValueHandling = NullValueHandling.Include; //we need null values for complex types
                        jsonProp.DefaultValueHandling = DefaultValueHandling.Include; //we need all properties serialized
                    }

                    if (!jsonProp.Ignored
                        && propertiesToIgnore.Any(np => np.Name == jsonProp.UnderlyingName))
                    {
                        jsonProp.Ignored = true;
                    }

                    _properties.Add(jsonProp);
                }

                lock (entityTypeInfo)
                {
                    //check for complextypes
                    var complexTypedProperties = entityTypeInfo.ComplexTypedProperties;

                    var complexJsonProperties = new List<JsonProperty>();

                    if (complexTypedProperties?.Count > 0)
                    {
                        //filter to complexproperties
                        var filteredJsonProperties = _properties?
                        .Select(p => new
                        {
                            JsonProperty = p,
                            PropertyInfo = complexTypedProperties.Where(pi => pi.Name == p.UnderlyingName).FirstOrDefault()
                        })
                        .Where(np => np.PropertyInfo != null)
                        .ToDictionary(np => np.JsonProperty, np => np.PropertyInfo);

                        Func<Type, IList<JsonProperty>> getResolvedPropertiesFunc = (t) =>
                        {
                            var contract = resolver.ResolveContract(t) as JsonObjectContract;

                            if (contract.Properties?.Count > 0)
                            {
                                ResolveEntityProperties
                                    (contract.Properties, t, entityService.GetEntityTypeInfo(t),
                                    entityService, resolver, createPropertyFunc);
                            }

                            return contract.Properties;
                        };

                        //generate new properties with new names for the complex types
                        foreach (var complexTypedJsonProp in filteredJsonProperties)
                        {
                            //get the complexTypedProperty's own jsonproperties
                            //include derived classes
                            var derivedTypes = entityService.GetDerivedEntityTypes(complexTypedJsonProp.Key.PropertyType)?
                                .Where(t => t.IsComplex()).ToList();

                            if (derivedTypes == null || derivedTypes.Count == 0)
                            {
                                entityService.AddEntityType(complexTypedJsonProp.Key.PropertyType);
                                derivedTypes = new List<Type>() { complexTypedJsonProp.Key.PropertyType };
                            }

                            var childProperties = derivedTypes
                                .SelectMany(dt => 
                                    getResolvedPropertiesFunc(dt)?.Where
                                        (p => /*!p.Ignored &&*/ p.PropertyName != Defaults.MetadataPropertyName) ?? new JsonProperty[0],
                                    (dt, property) => new
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
                                    var newChildProp = GetComplexTypedPropertyChild
                                        (childProp.DerivedType, complexTypedJsonProp.Key,
                                        complexTypedJsonProp.Value, childProp.Property);

                                    _properties.AddProperty(newChildProp);
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

                    int nextIdx = -1;

                    //clear and re-add these properties
                    jsonProperties.Clear();

                    var orderedProperties = _properties.OrderBy(p => p.Order ?? nextIdx++);

                    foreach (var prop in orderedProperties)
                    {
                        jsonProperties.Add(prop);
                    }

                    //create metadata property and add it last
                    var metadataJsonProperty = createPropertyFunc(Defaults.DummyMetadataPropertyInfo);
                    metadataJsonProperty.PropertyName = Defaults.MetadataPropertyName;
                    metadataJsonProperty.ValueProvider = new MetadataValueProvider(entityType, complexJsonProperties);
                    metadataJsonProperty.ShouldSerialize = (instance) =>
                    {
                        return !(metadataJsonProperty.ValueProvider as MetadataValueProvider)
                        .BuildMetadata(instance).IsEmpty();
                    };
                    metadataJsonProperty.Order = int.MaxValue; //try to make it the last serialized

                    jsonProperties.Add(metadataJsonProperty);

                    //assign and resolve these properties
                    entityTypeInfo.JsonProperties = new List<JsonProperty>(jsonProperties);

                    return true;
                }
            }

            return false;
        }

        private static EntityJsonProperty GetJsonPropertyDuplicate(JsonProperty prop)
        {
            var newProp = new EntityJsonProperty(prop);
            return newProp;
        }

        private static JsonProperty GetComplexTypedPropertyChild(Type complexType,
            JsonProperty complexTypedJsonProperty, PropertyInfo complexTypedPropertyInfo, JsonProperty child)
        {
            var newChild = GetJsonPropertyDuplicate(child);

            if (complexTypedJsonProperty.Ignored)
                newChild.Ignored = true; //this ensures that when a complex field is ignored, all its children are ignored too.

            //set complex name
            newChild.SimplePropertyName = child.PropertyName;
            newChild.PropertyName = $"{complexTypedJsonProperty.PropertyName}{Defaults.ComplexTypeNameSeparator}{child.PropertyName}";
            newChild.ComplexUnderlyingName = $"{complexTypedJsonProperty.GetComplexOrSimpleUnderlyingName()}{Defaults.ComplexTypeNameSeparator}{child.UnderlyingName}";

            //set new value provider
            newChild.ValueProvider = new ComplexTypedPropertyValueProvider
                (complexTypedJsonProperty.UnderlyingName, complexTypedJsonProperty.PropertyType,
                complexTypedJsonProperty.DeclaringType, complexTypedJsonProperty.ValueProvider,
                child.PropertyType, child.ValueProvider);

            //do this to avoid enclosing the jsonproperties in the closures.
            var parentActualName = complexTypedJsonProperty.UnderlyingName;
            var parentType = (child.ValueProvider as ComplexTypedPropertyValueProvider)?.DeclaringType ?? child.DeclaringType;
            var parentReflectedType = complexType;

            var childShouldSerialize = child.ShouldSerialize;
            //var childShouldDeserialize = child.ShouldDeserialize;

            //set a shouldserialize and shoulddeserialize
            newChild.ShouldSerialize = (entity) =>
            {
                var propertyInfo = entity.GetType().GetProperty(parentActualName);
                var parentValue = propertyInfo.GetValue(entity);

                Utils.Utilities.CheckIfComplexTypeInstanceIsNull(parentValue, parentActualName, propertyInfo.DeclaringType);

                var parentValueType = parentValue.GetType();

                var isAssignable = parentType.IsGenericAssignableFrom(parentValueType);
                isAssignable = isAssignable || parentReflectedType == parentValueType;

                return isAssignable && (childShouldSerialize == null || childShouldSerialize(parentValue) == true);
            };

            return newChild;
        }
    }
}
