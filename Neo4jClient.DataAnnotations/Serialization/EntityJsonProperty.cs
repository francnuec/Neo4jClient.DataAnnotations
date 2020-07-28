using Newtonsoft.Json.Serialization;

namespace Neo4jClient.DataAnnotations.Serialization
{
    public class EntityJsonProperty : JsonProperty
    {
        public EntityJsonProperty()
        {
        }

        public EntityJsonProperty(JsonProperty existingProp)
        {
            AttributeProvider = existingProp.AttributeProvider;
            Converter = existingProp.Converter;
            DeclaringType = existingProp.DeclaringType;
            DefaultValue = existingProp.DefaultValue;
            DefaultValueHandling = existingProp.DefaultValueHandling;
            GetIsSpecified = existingProp.GetIsSpecified;
            HasMemberAttribute = existingProp.HasMemberAttribute;
            Ignored = existingProp.Ignored;
            IsReference = existingProp.IsReference;
            ItemConverter = existingProp.ItemConverter;
            ItemIsReference = existingProp.ItemIsReference;
            ItemReferenceLoopHandling = existingProp.ItemReferenceLoopHandling;
            ItemTypeNameHandling = existingProp.ItemTypeNameHandling;
            MemberConverter = existingProp.MemberConverter;
            NullValueHandling = existingProp.NullValueHandling;
            ObjectCreationHandling = existingProp.ObjectCreationHandling;
            Order = existingProp.Order;
            PropertyName = existingProp.PropertyName;
            PropertyType = existingProp.PropertyType;
            Readable = existingProp.Readable;
            ReferenceLoopHandling = existingProp.ReferenceLoopHandling;
            Required = existingProp.Required;
            SetIsSpecified = existingProp.SetIsSpecified;
            ShouldDeserialize = existingProp.ShouldDeserialize;
            ShouldSerialize = existingProp.ShouldSerialize;
            TypeNameHandling = existingProp.TypeNameHandling;
            UnderlyingName = existingProp.UnderlyingName;
            ValueProvider = existingProp.ValueProvider;
            Writable = existingProp.Writable;

            ComplexUnderlyingName = (existingProp as EntityJsonProperty)?.ComplexUnderlyingName;
            SimplePropertyName = (existingProp as EntityJsonProperty)?.SimplePropertyName;
        }

        public string ComplexUnderlyingName { get; set; }

        public string SimplePropertyName { get; set; }
    }
}