using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Tests.Models;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class TestAnnotationsContext : AnnotationsContext
    {
        public TestAnnotationsContext(
            IGraphClient graphClient,
            EntityResolver resolver) : base(graphClient, resolver)
        {
        }

        public TestAnnotationsContext(
            IGraphClient graphClient,
            EntityConverter converter) : base(graphClient, converter)
        {
        }

        public virtual EntitySet<PersonNode> Persons { get; set; }
        public virtual EntitySet<DirectorNode> Directors { get; set; }
        public virtual EntitySet<MovieNode> Movies { get; set; }
        public virtual EntitySet<MovieExtraNode> MovieExtras { get; set; }
        public virtual EntitySet<ActorNode> Actors { get; set; }
        public virtual EntitySet<Address> Addresses { get; set; }
        public virtual EntitySet<AddressWithComplexType> ComplexAddresses { get; set; }
        public virtual EntitySet<Location> Locations { get; set; }
        public virtual EntitySet<AddressThirdLevel> AddressThirdLevels { get; set; }
        public virtual EntitySet<SomeComplexType> SomeComplexTypes { get; set; }
    }
}