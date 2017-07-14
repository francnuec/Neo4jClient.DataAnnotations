using Neo4jClient.DataAnnotations.Tests.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class TestUtilities
    {
        public static EntityResolver Resolver { get; } = new EntityResolver();

        public static JsonSerializerSettings SerializerSettingsWithResolver = new JsonSerializerSettings()
        {
            //Converters = new List<JsonConverter>() { new EntityConverter() },
            ContractResolver = Resolver,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        public static Func<object, string> SerializerWithResolver = (entity) => JsonConvert.SerializeObject(entity, SerializerSettingsWithResolver);

        public static ActorNode Actor = new ActorNode<int>()
        {
            Name = "Ellen Pompeo",
            Born = 1969,
            Address = new AddressWithComplexType()
            {
                City = "Los Angeles",
                State = "California",
                Country = "US",
                Location = new Location()
                {
                    Latitude = 34.0522,
                    Longitude = -118.2437
                }
            }
        };

        public static void AddEntityTypes()
        {
            var entityTypes = new Type[] { typeof(PersonNode), typeof(DirectorNode), typeof(MovieNode), typeof(MovieExtraNode),
                typeof(ActorNode), typeof(Address), typeof(AddressWithComplexType), typeof(Location), typeof(AddressThirdLevel), typeof(someComplexType) };

            foreach (var entityType in entityTypes)
                Neo4jAnnotations.AddEntityType(entityType);
        }
    }
}
