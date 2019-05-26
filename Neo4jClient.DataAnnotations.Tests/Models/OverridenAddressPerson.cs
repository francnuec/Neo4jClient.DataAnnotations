using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations.Tests.Models
{
    public class InheritedAddressMemberNamePerson : PersonNode
    {
        public override Address Address { get => base.Address; set => base.Address = value; }
    }

    public class OverridenAddressMemberNamePerson : PersonNode
    {
        [JsonProperty(PropertyName = "NewNewAddressName")]
        public override Address Address { get => base.Address; set => base.Address = value; }
    }
}
