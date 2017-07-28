using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO;

namespace Neo4jClient.DataAnnotations.Serialization
{
    public class EntityConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return Defaults.EntityResolver == null &&
                Neo4jAnnotations.ContainsEntityType(objectType);
        }

        public override bool CanRead => Defaults.EntityResolver == null;

        public override bool CanWrite => Defaults.EntityResolver == null;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Utilities.EnsureSerializerInstance(ref serializer);

            var value = existingValue ?? Utilities.CreateInstance(objectType);

            if (value != null)
            {
                var valueType = objectType;

                var entityInfo = Neo4jAnnotations.GetEntityTypeInfo(valueType);

                //set necessarily things
                InitializeEntityInfo(entityInfo, serializer);

                var thisConverterType = typeof(EntityConverter);

                //temporarily remove this converter from the main serializer to avoid an unending loop
                List<Tuple<JsonConverter, int>> entityConverters;
                Utilities.RemoveThisConverter(thisConverterType, serializer, out entityConverters);

                //now convert to JObject
                var valueJObject = serializer.Deserialize<JObject>(reader);

                Utilities.EnsureRightJObject(ref valueJObject);

                if (valueJObject == null || valueJObject.Type == JTokenType.Null)
                    return null;

                //get the metadata
                var metadataPropValue = valueJObject[Defaults.MetadataPropertyName];
                Metadata metadata = null;

                if (metadataPropValue != null && metadataPropValue.Type != JTokenType.Null)
                {
                    valueJObject.Remove(Defaults.MetadataPropertyName);

                    metadata = Utilities.DeserializeMetadata(metadataPropValue.ToObject<string>());

                    if (metadata?.NullProperties?.Count > 0)
                    {
                        //add all the null properties to the object so it can deserialize them too
                        foreach (var nullProp in metadata.NullProperties)
                        {
                            var existingProp = valueJObject.Property(nullProp);
                            //should be null, but in any case, be careful not to replace any existing values
                            if (existingProp == null)
                            {
                                valueJObject.Add(nullProp, null);
                            }
                        }
                    }
                }

                //restore the removed converters
                Utilities.RestoreThisConverter(serializer, entityConverters);

                HandleComplexTypedPropsRead(serializer, value, valueJObject, entityInfo);

                ////remove all object tokens that may still remain
                //RemoveJsonObjectProperties(valueJObject, entityInfo);

                Utilities.RemoveThisConverter(thisConverterType, serializer, out entityConverters);

                //finally deserialize
                serializer.Populate(new JTokenReader(valueJObject), value);

                Utilities.RestoreThisConverter(serializer, entityConverters);
            }
            else
            {
                value = serializer.Deserialize(reader, objectType);
            }

            return value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Utilities.EnsureSerializerInstance(ref serializer);

            if (value != null)
            {
                var valueType = value.GetType();

                var entityInfo = Neo4jAnnotations.GetEntityTypeInfo(valueType);

                //set necessarily things
                InitializeEntityInfo(entityInfo, serializer);

                var thisConverterType = typeof(EntityConverter);

                //temporarily remove this converter from the main serializer to avoid an unending loop
                List<Tuple<JsonConverter, int>> entityConverters;
                Utilities.RemoveThisConverter(thisConverterType, serializer, out entityConverters);

                //now convert to JObject
                var valueJObject = JObject.FromObject(value, serializer);

                //restore the removed converters
                Utilities.RestoreThisConverter(serializer, entityConverters);

                var nullValueHandling = serializer.NullValueHandling;
                var defaultValueHandling = serializer.DefaultValueHandling;
                serializer.NullValueHandling = NullValueHandling.Include; //we need null values for complex types
                serializer.DefaultValueHandling = DefaultValueHandling.Include; //we need all properties for complex types

                var metadata = new Metadata();

                HandleComplexTypedPropsWrite(serializer, value, valueJObject, entityInfo, metadata);

                //remove all object tokens that may still remain
                RemoveJsonObjectProperties(valueJObject, entityInfo);

                Utilities.RemoveThisConverter(thisConverterType, serializer, out entityConverters);

                if (!metadata.IsEmpty())
                {
                    //serialize the metadata to the object
                    valueJObject.Add(Defaults.MetadataPropertyName, Utilities.SerializeMetadata(metadata));
                }

                //finally serialize object
                serializer.Serialize(writer, valueJObject);

                //restore the handlings
                serializer.NullValueHandling = nullValueHandling;
                serializer.DefaultValueHandling = defaultValueHandling;

                Utilities.RestoreThisConverter(serializer, entityConverters);
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }

        private static void InitializeEntityInfo(EntityTypeInfo entityInfo, JsonSerializer serializer)
        {
            entityInfo.WithJsonResolver(serializer.ContractResolver as DefaultContractResolver);
        }

        private static void RemoveJsonObjectProperties(JObject valueJObject, EntityTypeInfo entityInfo)
        {
            var stringType = typeof(string);

            var props = valueJObject.Children<JProperty>().ToList();
            foreach (var child in props)
            {
                bool remove = false;

                if (child.Value.Type == JTokenType.Object)
                {
                    remove = true;
                }
                else if (child.Value.Type == JTokenType.Null || child.Value.Type == JTokenType.None)
                {
                    //determine is the property is scalar.
                    //if not scalar, remove.
                    if (entityInfo.JsonNamePropertyMap.TryGetValue(child.Name, out var memberInfo))
                    {
                        var propInfo = memberInfo as PropertyInfo;
                        if (!Utilities.IsTypeScalar(propInfo?.PropertyType))
                        {
                            remove = true;
                        }
                    }
                }

                if (remove)
                {
                    child.Remove();
                    //entityInfo.IgnoredJsonProperties.Add(child.Name);
                }
                //else
                //{
                //    //remove it in case it was added erroneously
                //    entityInfo.IgnoredJsonProperties.Remove(child.Name);
                //}
            }
        }

        private static void HandleComplexTypedPropsRead(JsonSerializer serializer,
            object value, JObject valueJObject, EntityTypeInfo entityInfo)
        {
            var complexTypedProps = entityInfo.ComplexTypedProperties;

            foreach (var prop in complexTypedProps)
            {
                var propValueType = prop.PropertyType;

                string propJsonName = prop.Name;

                foreach (var propJsonNameMap in entityInfo.JsonNamePropertyMap)
                {
                    if (propJsonNameMap.Value.IsEquivalentTo(prop))
                    {
                        propJsonName = propJsonNameMap.Key; //use the assigned name from contract
                        break;
                    }
                }

                var childProps = valueJObject.Properties().Where(jp => jp.Name.StartsWith(propJsonName)
                //an actual complex type property should not be found on the entity
                && !entityInfo.JsonNamePropertyMap.Any(map => map.Key == jp.Name
                && entityInfo.AllProperties.Contains(map.Value))
                ).ToArray();

                //create new JObject from childProps
                var propValueJObject = new JObject();

                foreach (var childProp in childProps)
                {
                    propValueJObject.Add(childProp.Name.Remove(0, propJsonName.Length + 1), childProp.Value);
                }

                //first ascertain we handle this type
                Neo4jAnnotations.AddEntityType(propValueType);

                //now find the entity info that has all the jsonproperties. that would be the real property type
                EntityTypeInfo propValueInfo = null;
                var derivedInfos = Neo4jAnnotations.GetDerivedEntityTypeInfos(propValueType, getFromEntityTypesToo: true);

                foreach (var derivedInfo in derivedInfos)
                {
                    InitializeEntityInfo(derivedInfo, serializer);

                    var sepIndex = -1;
                    if (propValueJObject.Properties().All(jp => derivedInfo.JsonNamePropertyMap.ContainsKey(jp.Name)
                    || ((sepIndex = jp.Name.IndexOf(Defaults.ComplexTypeNameSeparator)) > 0
                    && derivedInfo.JsonNamePropertyMap.ContainsKey(jp.Name.Substring(0, sepIndex)))))
                    {
                        //found it
                        propValueInfo = derivedInfo;
                        break; //we need only the first guy to match
                    }
                }

                if (propValueInfo == null)
                {
                    //BIG PROBLEM
                    //we couldn't find matching type for the complex property
                    throw new InvalidOperationException(string.Format(Messages.ComplexTypedPropertyMatchingTypeNotFoundError, prop.Name, prop.DeclaringType.Name));
                }

                propValueType = propValueInfo.Type;

                //deserialize the jobject
                var propValue = propValueJObject.ToObject(propValueType, serializer);

                if (propValue == null)
                {
                    //Complex types cannot be null. A value must be provided always.
                    throw new InvalidOperationException(string.Format(Messages.NullComplexTypePropertyError, prop.Name, prop.DeclaringType.Name));
                }

                //set the value
                prop.SetValue(value, propValue);

                //remove the childprops from entity
                foreach (var childProp in childProps)
                {
                    //this check was defered till now because we needed the right property type
                    if (childProp.Value.Type == JTokenType.Object)
                    {
                        //don't allow object nesting within complextypes
                        throw new InvalidOperationException(string.Format(Messages.NestedComplexTypesError, propValueType.Name));
                    }

                    //add to the jsonmap first
                    entityInfo.JsonNamePropertyMap[childProp.Name] = propValueInfo.JsonNamePropertyMap[childProp.Name.Substring(propJsonName.Length + 1)];

                    childProp.Remove();
                }
            }
        }

        private static void HandleComplexTypedPropsWrite(JsonSerializer serializer,
            object value, JObject valueJObject, EntityTypeInfo entityInfo, Metadata metadata)
        {
            var complexTypedProps = entityInfo.ComplexTypedProperties;

            foreach (var prop in complexTypedProps)
            {
                var propValue = prop.GetValue(value);

                if (propValue == null)
                {
                    //Complex types cannot be null. A value must be provided always.
                    throw new InvalidOperationException(string.Format(Messages.NullComplexTypePropertyError, prop.Name, prop.DeclaringType.Name));
                }

                var propValueType = propValue.GetType();

                JProperty propBefore = null;
                string propJsonName = prop.Name;

                foreach (var propJsonNameMap in entityInfo.JsonNamePropertyMap)
                {
                    if (propJsonNameMap.Value.IsEquivalentTo(prop))
                    {
                        propJsonName = propJsonNameMap.Key; //use the assigned name from contract
                        break;
                    }

                    propBefore = valueJObject.Property(propJsonNameMap.Key) ?? propBefore;
                }

                JProperty currentProp = propBefore, temp = null;

                if (currentProp == null)
                {
                    //will remove this later
                    temp = new JProperty("temp", "");
                    valueJObject.AddFirst(temp);
                    currentProp = temp;
                }

                //first ascertain we handle this type
                Neo4jAnnotations.AddEntityType(propValueType);

                //then serialize the value
                var propValueJObject = JObject.FromObject(propValue, serializer);
                var propValueInfo = Neo4jAnnotations.GetEntityTypeInfo(propValueType);

                foreach (var propChild in propValueJObject.Children<JProperty>())
                {
                    if (propChild.Value.Type == JTokenType.Object)
                    {
                        //don't allow object nesting within complextypes
                        throw new InvalidOperationException(string.Format(Messages.NestedComplexTypesError, propValueType.Name));
                    }

                    if (propChild.Name == Defaults.MetadataPropertyName)
                    {
                        //skip the metadata property
                        //test the properties direct
                        continue;
                    }

                    //add to the parent object itself.
                    var newProp = new JProperty(propJsonName + Defaults.ComplexTypeNameSeparator + propChild.Name, propChild.Value);
                    currentProp.AddAfterSelf(newProp);
                    currentProp = newProp;

                    entityInfo.JsonNamePropertyMap[newProp.Name] = propValueInfo.JsonNamePropertyMap[propChild.Name];

                    if (newProp.Value == null || newProp.Value.Type == JTokenType.Null)
                    {
                        //we need to store this in metadata because neo4j does not store null properties.
                        metadata.NullProperties.Add(newProp.Name);
                    }
                }

                if (temp != null)
                {
                    temp.Remove();
                }
            }
        }
    }
}
