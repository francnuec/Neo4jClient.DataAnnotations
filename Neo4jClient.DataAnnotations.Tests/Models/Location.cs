using System.ComponentModel.DataAnnotations.Schema;

namespace Neo4jClient.DataAnnotations.Tests.Models
{
    [ComplexType]
    public class Location
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}