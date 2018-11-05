﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO;

namespace Neo4jClient.DataAnnotations.Serialization
{
    public class EntityConverter : JsonConverter, IHaveAnnotationsContext
    {
        public EntityConverter()
        {
        }

        public override bool CanConvert(Type objectType)
        {
            return AnnotationsContext.EntityResolver == null &&
                EntityService.ContainsEntityType(objectType);
        }

        public override bool CanRead => AnnotationsContext.EntityResolver == null; //resolver takes precedence by default

        public override bool CanWrite => AnnotationsContext.EntityResolver == null;

        public virtual IAnnotationsContext AnnotationsContext { get; internal set; }

        public IEntityService EntityService => AnnotationsContext.EntityService;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            SerializationUtilities.EnsureSerializerInstance(ref serializer);

            var value = existingValue ?? Utils.Utilities.CreateInstance(objectType);

            if (value != null)
            {
                var valueType = objectType;

                var entityInfo = EntityService.GetEntityTypeInfo(valueType);

                //set necessarily things
                InitializeEntityInfo(entityInfo, serializer);

                var thisConverterType = typeof(EntityConverter);

                //temporarily remove this converter from the main serializer to avoid an unending loop
                List<Tuple<JsonConverter, int>> entityConverters;
                SerializationUtilities.RemoveThisConverter(thisConverterType, serializer, out entityConverters);

                //now convert to JObject
                var valueJObject = serializer.Deserialize<JObject>(reader);

                SerializationUtilities.EnsureRightJObject(ref valueJObject);

                if (valueJObject == null || valueJObject.Type == JTokenType.Null)
                    return null;

                //get the metadata
                var metadataPropValue = valueJObject[Defaults.MetadataPropertyName];
                Metadata metadata = null;

                if (metadataPropValue != null && metadataPropValue.Type != JTokenType.Null)
                {
                    valueJObject.Remove(Defaults.MetadataPropertyName);

                    metadata = SerializationUtilities.DeserializeMetadata(metadataPropValue.ToObject<string>());

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
                SerializationUtilities.RestoreThisConverter(serializer, entityConverters);

                HandleComplexTypedPropsRead(EntityService, serializer, value, valueJObject, entityInfo);

                ////remove all object tokens that may still remain
                //RemoveJsonObjectProperties(valueJObject, entityInfo);

                SerializationUtilities.RemoveThisConverter(thisConverterType, serializer, out entityConverters);

                //finally deserialize
                serializer.Populate(new JTokenReader(valueJObject), value);

                SerializationUtilities.RestoreThisConverter(serializer, entityConverters);
            }
            else
            {
                value = serializer.Deserialize(reader, objectType);
            }

            return value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            SerializationUtilities.EnsureSerializerInstance(ref serializer);

            if (value != null)
            {
                var valueType = value.GetType();

                var entityInfo = EntityService.GetEntityTypeInfo(valueType);

                //set necessarily things
                InitializeEntityInfo(entityInfo, serializer);

                var thisConverterType = typeof(EntityConverter);

                //temporarily remove this converter from the main serializer to avoid an unending loop
                List<Tuple<JsonConverter, int>> entityConverters;
                SerializationUtilities.RemoveThisConverter(thisConverterType, serializer, out entityConverters);

                //now convert to JObject
                var valueJObject = JObject.FromObject(value, serializer);

                //restore the removed converters
                SerializationUtilities.RestoreThisConverter(serializer, entityConverters);

                var nullValueHandling = serializer.NullValueHandling;
                var defaultValueHandling = serializer.DefaultValueHandling;
                serializer.NullValueHandling = NullValueHandling.Include; //we need null values for complex types
                serializer.DefaultValueHandling = DefaultValueHandling.Include; //we need all properties for complex types

                var metadata = new Metadata();

                HandleComplexTypedPropsWrite(EntityService, serializer, value, valueJObject, entityInfo, metadata);

                //remove all object tokens that may still remain
                RemoveJsonObjectProperties(valueJObject, entityInfo);

                SerializationUtilities.RemoveThisConverter(thisConverterType, serializer, out entityConverters);

                if (!metadata.IsEmpty())
                {
                    //serialize the metadata to the object
                    valueJObject.Add(Defaults.MetadataPropertyName, SerializationUtilities.SerializeMetadata(metadata));
                }

                //finally serialize object
                serializer.Serialize(writer, valueJObject);

                //restore the handlings
                serializer.NullValueHandling = nullValueHandling;
                serializer.DefaultValueHandling = defaultValueHandling;

                SerializationUtilities.RestoreThisConverter(serializer, entityConverters);
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
                    if (entityInfo.JsonNamePropertyMap.FirstOrDefault(p => p.Key.Json == child.Name).Value is MemberInfo memberInfo) //TryGetValue(child.Name, out var memberInfo))
                    {
                        var propInfo = memberInfo as PropertyInfo;
                        if (!Utils.Utilities.IsScalarType(propInfo?.PropertyType, entityInfo.EntityService))
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

        private static void HandleComplexTypedPropsRead
            (IEntityService entityService, JsonSerializer serializer,
            object value, JObject valueJObject, EntityTypeInfo entityInfo)
        {
            var complexTypedProps = entityInfo.ComplexTypedProperties;

            foreach (var prop in complexTypedProps)
            {
                var propValueType = prop.PropertyType;

                MemberName propJsonName = new MemberName(prop.Name, prop.Name);

                foreach (var propJsonNameMap in entityInfo.JsonNamePropertyMap)
                {
                    if (propJsonNameMap.Value.IsEquivalentTo(prop))
                    {
                        propJsonName = propJsonNameMap.Key; //use the assigned name from contract
                        break;
                    }
                }

                var childProps = valueJObject.Properties().Where(jp => jp.Name.StartsWith(propJsonName.Json)
                //an actual complex type property should not be found on the entity
                && !entityInfo.JsonNamePropertyMap.Any(map => map.Key.Json == jp.Name
                && entityInfo.AllProperties.Contains(map.Value))
                ).ToArray();

                //create new JObject from childProps
                var propValueJObject = new JObject();

                foreach (var childProp in childProps)
                {
                    propValueJObject.Add(childProp.Name.Remove(0, propJsonName.Json.Length + 1), childProp.Value);
                }

                //first ascertain we handle this type
                entityService.AddEntityType(propValueType);

                //now find the entity info that has all the jsonproperties. that would be the real property type
                EntityTypeInfo propValueInfo = null;
                var derivedInfos = entityService.GetDerivedEntityTypeInfos(propValueType, getFromEntityTypesToo: true);

                foreach (var derivedInfo in derivedInfos)
                {
                    InitializeEntityInfo(derivedInfo, serializer);

                    var sepIndex = -1;
                    if (propValueJObject.Properties().All(jp => derivedInfo.JsonNamePropertyMap.Any(p => p.Key.Json == jp.Name)//.ContainsKey(jp.Name)
                    || ((sepIndex = jp.Name.IndexOf(Defaults.ComplexTypeNameSeparator)) > 0
                    && derivedInfo.JsonNamePropertyMap.Any(p => p.Key.Json == jp.Name.Substring(0, sepIndex))))) //.ContainsKey(jp.Name.Substring(0, sepIndex)))))
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
                    var childNamePair = propValueInfo.JsonNamePropertyMap.FirstOrDefault(pair => pair.Key.Json == childProp.Name.Substring(propJsonName.Json.Length + 1));
                    var newChildName = new MemberName($"{propJsonName.Actual}{Defaults.ComplexTypeNameSeparator}{childNamePair.Key.Actual}", childProp.Name);
                    entityInfo.JsonNamePropertyMap[newChildName] = childNamePair.Value; //propValueInfo.JsonNamePropertyMap[childProp.Name.Substring(propJsonName.Json.Length + 1)];

                    childProp.Remove();
                }
            }
        }

        private static void HandleComplexTypedPropsWrite
            (IEntityService entityService, JsonSerializer serializer,
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
                MemberName propJsonName = new MemberName(prop.Name, prop.Name);

                foreach (var propJsonNameMap in entityInfo.JsonNamePropertyMap)
                {
                    if (propJsonNameMap.Value.IsEquivalentTo(prop))
                    {
                        propJsonName = propJsonNameMap.Key; //use the assigned name from contract
                        break;
                    }

                    propBefore = valueJObject.Property(propJsonNameMap.Key?.Json) ?? propBefore;
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
                entityService.AddEntityType(propValueType);

                //then serialize the value
                var propValueJObject = JObject.FromObject(propValue, serializer);
                var propValueInfo = entityService.GetEntityTypeInfo(propValueType);

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
                    var propNamePair = propValueInfo.JsonNamePropertyMap.FirstOrDefault(pair => pair.Key.Json == propChild.Name);
                    var newPropName = new MemberName($"{propJsonName.Actual}{Defaults.ComplexTypeNameSeparator}{propNamePair.Key.Actual}",
                        $"{propJsonName.Json}{Defaults.ComplexTypeNameSeparator}{propChild.Name}");

                    var newProp = new JProperty(newPropName.Json, //propJsonName.Json + Defaults.ComplexTypeNameSeparator + propChild.Name,
                        propChild.Value);
                    currentProp.AddAfterSelf(newProp);
                    currentProp = newProp;

                    entityInfo.JsonNamePropertyMap[newPropName] = propNamePair.Value; //propValueInfo.JsonNamePropertyMap[propChild.Name];

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
