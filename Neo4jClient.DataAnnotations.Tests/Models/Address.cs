using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Neo4jClient.DataAnnotations.Tests.Models
{
    /// <summary>
    /// <see cref="ComplexTypeAttribute"/> allows us to use classes with Neo4j without declaring another entity to represent them.
    /// This <see cref="Address"/> example is a crude way to show how complex types can work.
    /// Any class not marked with the <see cref="ComplexTypeAttribute"/> is assumed to be an entity.
    /// </summary>
    [ComplexType]
    public class Address
    {
        public string AddressLine { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }
    }

    [ComplexType]
    public class AddressWithComplexType : Address
    {
        public Location Location { get; set; }
    }

    [ComplexType]
    public class AddressThirdLevel : AddressWithComplexType
    {
        public someComplexType complex { get; set; }

        public string own { get; set; }
    }

    [ComplexType]
    public class someComplexType
    {
        public int prop { get; set; }
    }
}
