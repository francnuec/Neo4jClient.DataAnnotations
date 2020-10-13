using System.Collections.Generic;
using Neo4jClient.DataAnnotations.Utils;
using Newtonsoft.Json;

namespace Neo4jClient.DataAnnotations.Serialization
{
    public class Metadata
    {
        [JsonProperty(PropertyName = Defaults.MetadataNullPropertiesPropertyName)]
        public List<string> NullProperties { get; set; } = new List<string>();

        public bool IsEmpty()
        {
            return NullProperties.Count == 0;
        }
    }
}