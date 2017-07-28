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
    public class ResolverDummyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return Defaults.EntityResolver != null //only works when we have a resolver set
                && Neo4jAnnotations.ContainsEntityType(objectType);
        }

        public override bool CanRead => Defaults.EntityResolver != null;

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var existingResolver = serializer?.ContractResolver;
            Utilities.EnsureSerializerInstance(ref serializer);

            var entityResolverType = typeof(EntityResolver);

            if (existingResolver == null 
                || !entityResolverType.IsAssignableFrom(existingResolver.GetType()))
                serializer.ContractResolver = Defaults.EntityResolver; //we must try use this resolver here

            if (serializer.ContractResolver == null 
                || !entityResolverType.IsAssignableFrom(serializer.ContractResolver.GetType()))
                throw new InvalidOperationException("EntityResolver instance is missing.");

            var value = existingValue ?? Utilities.CreateInstance(objectType);

            if (value != null)
            {
                var valueType = objectType;

                //temporarily remove this converter from the main serializer to avoid an unending loop
                List<Tuple<JsonConverter, int>> entityConverters;
                Utilities.RemoveThisConverter(typeof(ResolverDummyConverter), serializer, out entityConverters);

                //now convert to JObject
                var valueJObject = serializer.Deserialize<JObject>(reader);

                Utilities.EnsureRightJObject(ref valueJObject);

                if (valueJObject == null || valueJObject.Type == JTokenType.Null)
                    return null;

                //finally deserialize
                serializer.Populate(new JTokenReader(valueJObject), value);

                Utilities.RestoreThisConverter(serializer, entityConverters);
            }
            else
            {
                value = serializer.Deserialize(reader, objectType);
            }

            serializer.ContractResolver = existingResolver ?? serializer.ContractResolver;

            return value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
