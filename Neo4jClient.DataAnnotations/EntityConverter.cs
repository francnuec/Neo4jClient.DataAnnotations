using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Neo4jClient.DataAnnotations
{
    public class EntityConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return Neo4jAnnotations.ContainsEntityType(objectType);
        }

        public override bool CanRead => false;

        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                var valueType = value.GetType();

                var entityInfo = Neo4jAnnotations.GetEntityTypeInfo(valueType);

                //set necessarily things
                InitializeEntityInfo(entityInfo, serializer);

                //temporarily remove this converter from the main serializer to avoid an unending loop
                List<Tuple<JsonConverter, int>> entityConverters;
                RemoveThisConverter(serializer, out entityConverters);

                //now convert to JObject
                var valueJObject = JObject.FromObject(value, serializer);

                //restore the removed converters
                RestoreThisConverter(serializer, entityConverters);

                HandleComplexTypedPropsWrite(writer, serializer, value, valueJObject, entityInfo);

                //remove all object tokens that may still remain
                RemoveJsonObjectProperties(valueJObject, entityInfo);

                RemoveThisConverter(serializer, out entityConverters);

                //finally serialize
                serializer.Serialize(writer, valueJObject);

                RestoreThisConverter(serializer, entityConverters);
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }

        private void InitializeEntityInfo(EntityTypeInfo entityInfo, JsonSerializer serializer)
        {
            entityInfo.WithJsonResolver(serializer.ContractResolver as DefaultContractResolver);
        }

        private void RemoveJsonObjectProperties(JObject valueJObject, EntityTypeInfo entityInfo)
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

        private void RemoveThisConverter(JsonSerializer serializer, out List<Tuple<JsonConverter, int>> entityConverters)
        {
            entityConverters = new List<Tuple<JsonConverter, int>>();

            var thisConverterType = typeof(EntityConverter);

            for (int i = 0, l = serializer.Converters.Count; i < l; i++)
            {
                var converter = serializer.Converters[i];
                if (converter.GetType() == thisConverterType)
                {
                    entityConverters.Add(new Tuple<JsonConverter, int>(converter, i));
                }
            }

            foreach (var converter in entityConverters)
            {
                serializer.Converters.Remove(converter.Item1);
            }
        }

        private void RestoreThisConverter(JsonSerializer serializer, List<Tuple<JsonConverter, int>> entityConverters,
            bool clearList = true)
        {
            foreach (var converter in entityConverters)
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

            if (clearList)
                entityConverters.Clear();
        }

        private void HandleComplexTypedPropsWrite(JsonWriter writer, JsonSerializer serializer,
            object value, JObject valueJObject, EntityTypeInfo entityInfo)
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
                    if (propJsonNameMap.Value == prop)
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
                        throw new InvalidOperationException(string.Format(Messages.NestedComplexTypesError, prop.PropertyType.Name));
                    }

                    //add to the parent object itself.
                    var newProp = new JProperty(propJsonName + "_" + propChild.Name, propChild.Value);
                    currentProp.AddAfterSelf(newProp);
                    currentProp = newProp;

                    entityInfo.JsonNamePropertyMap[newProp.Name] = propValueInfo.JsonNamePropertyMap[propChild.Name];
                }

                if (temp != null)
                {
                    temp.Remove();
                }
            }
        }
    }
}
