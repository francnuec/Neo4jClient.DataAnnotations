using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Neo4jClient.DataAnnotations.Tests.Models
{
    [Table("Person")]
    public abstract class PersonNode
    {
        public string Name { get; set; }

        public int Born { get; set; }

        [JsonProperty(PropertyName = "NewAddressName")]
        public virtual Address Address { get; set; }
    }
}