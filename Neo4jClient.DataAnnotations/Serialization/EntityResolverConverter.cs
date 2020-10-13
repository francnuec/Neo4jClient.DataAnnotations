using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.DataAnnotations.Serialization
{
    /// <summary>
    ///     Converter used in using the Resolver mode for deserialization because Neo4jClient currently uses converters when
    ///     deserializing.
    /// </summary>
    public class EntityResolverConverter : JsonConverter, IHaveAnnotationsContext, IHaveEntityService
    {
        public EntityResolverConverter(EntityResolver resolver)
        {
            Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public EntityResolver Resolver { get; set; }

        public override bool CanRead => Resolver != null;

        public override bool CanWrite => false;

        public AnnotationsContext AnnotationsContext => Resolver.AnnotationsContext;

        public EntityService EntityService => AnnotationsContext.EntityService;

        public override bool CanConvert(Type objectType)
        {
            //only works when we have a resolver set
            return Resolver != null && EntityService.ContainsEntityType(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var existingResolver = serializer?.ContractResolver;
            SerializationUtilities.EnsureSerializerInstance(ref serializer);

            var entityResolverType = typeof(EntityResolver);

            if (existingResolver == null
                || !entityResolverType.IsAssignableFrom(existingResolver.GetType()))
                serializer.ContractResolver = Resolver; //we must try use this resolver here

            if (serializer.ContractResolver == null
                || !entityResolverType.IsAssignableFrom(serializer.ContractResolver.GetType()))
                throw new InvalidOperationException("EntityResolver instance is missing.");

            //temporarily remove this converter from the main serializer to avoid an unending loop
            List<Tuple<JsonConverter, int>> entityConverters;
            SerializationUtilities.RemoveThisConverter(typeof(EntityResolverConverter), serializer,
                out entityConverters);

            //now convert to JObject
            var valueJObject = serializer.Deserialize<JObject>(reader);

            SerializationUtilities.EnsureRightJObject(AnnotationsContext, ref valueJObject,
                out var valueMetadataJObject);

            objectType = SerializationUtilities.GetRightObjectType(objectType, valueMetadataJObject, EntityService);

            var value = existingValue ?? Utils.Utilities.CreateInstance(objectType);

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

            serializer.ContractResolver = existingResolver ?? serializer.ContractResolver;

            return value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}