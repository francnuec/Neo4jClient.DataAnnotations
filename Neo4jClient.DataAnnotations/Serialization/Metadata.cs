using Newtonsoft.Json;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Text;

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
