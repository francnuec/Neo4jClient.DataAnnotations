using Newtonsoft.Json.Serialization;

namespace Neo4jClient.DataAnnotations.Serialization
{
    public static class SerializationExtensions
    {
        public static string GetComplexOrSimpleUnderlyingName(this JsonProperty property)
        {
            return (property as EntityJsonProperty)?.ComplexUnderlyingName ?? property.UnderlyingName;
        }

        public static string GetSimpleOrComplexPropertyName(this JsonProperty property)
        {
            return (property as EntityJsonProperty)?.SimplePropertyName ?? property.PropertyName;
        }
    }
}