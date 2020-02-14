using Newtonsoft.Json;
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

        public virtual AnnotationsContext AnnotationsContext { get; internal set; }

        public EntityService EntityService => AnnotationsContext.EntityService;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            SerializationUtilities.EnsureSerializerInstance(ref serializer);

            var valueType = objectType;

            var entityInfo = EntityService.GetEntityTypeInfo(valueType);

            //set necessary things
            InitializeEntityInfo(entityInfo, serializer);

            //temporarily remove this converter from the main serializer to avoid an unending loop
            List<Tuple<JsonConverter, int>> entityConverters;
            SerializationUtilities.RemoveThisConverter(typeof(EntityConverter), serializer, out entityConverters);

            //now convert to JObject
            var valueJObject = serializer.Deserialize<JObject>(reader);

            SerializationUtilities.EnsureRightJObject(AnnotationsContext, ref valueJObject, out var valueMetadataJObject);

            valueType = SerializationUtilities.GetRightObjectType(valueType, valueMetadataJObject, EntityService); //this ensures we have the right type to deserialize into

            if (valueType != objectType)
            {
                //reinitialize entity info
                entityInfo = EntityService.GetEntityTypeInfo(valueType);
                //set necessarily things
                InitializeEntityInfo(entityInfo, serializer);
            }

            var value = existingValue ?? Utils.Utilities.CreateInstance(valueType);

            if (value != null)
            {
                if (valueJObject == null || valueJObject.Type == JTokenType.Null)
                    return null;

                //finally deserialize
                serializer.Populate(new JTokenReader(valueJObject), value);

                SerializationUtilities.RestoreThisConverter(serializer, entityConverters);
            }
            else
            {
                SerializationUtilities.RestoreThisConverter(serializer, entityConverters);
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

                //set necessary things
                InitializeEntityInfo(entityInfo, serializer);

                //temporarily remove this converter from the main serializer to avoid an unending loop
                List<Tuple<JsonConverter, int>> entityConverters;
                SerializationUtilities.RemoveThisConverter(typeof(EntityConverter), serializer, out entityConverters);

                var nullValueHandling = serializer.NullValueHandling;
                var defaultValueHandling = serializer.DefaultValueHandling;
                serializer.NullValueHandling = NullValueHandling.Include; //we need null values for complex types
                serializer.DefaultValueHandling = DefaultValueHandling.Include; //we need all properties for complex types

                //finally serialize object
                serializer.Serialize(writer, value);

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

        private void InitializeEntityInfo(EntityTypeInfo entityInfo, JsonSerializer serializer)
        {
            entityInfo.ResolveJsonPropertiesUsing(serializer.ContractResolver as DefaultContractResolver);
        }
    }
}
