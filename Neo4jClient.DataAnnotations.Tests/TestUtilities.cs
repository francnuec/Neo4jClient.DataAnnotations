using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Tests.Models;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;
using NSubstitute;
using Newtonsoft.Json.Serialization;
using Neo4jClient.DataAnnotations.Cypher;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class TestUtilities
    {
        public static EntityResolver Resolver { get; } = new EntityResolver();

        public static EntityConverter Converter { get; } = new EntityConverter();

        public static JsonSerializerSettings SerializerSettingsWithConverter = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() { Converter },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        public static JsonSerializerSettings SerializerSettingsWithResolver = new JsonSerializerSettings()
        {
            //Converters = new List<JsonConverter>() { new EntityConverter() },
            ContractResolver = Resolver,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        public static Func<object, string> SerializeWithResolver = (entity) => JsonConvert.SerializeObject(entity, SerializerSettingsWithResolver);

        public static Func<object, string> SerializeWithConverter = (entity) => JsonConvert.SerializeObject(entity, SerializerSettingsWithConverter);

        public static Func<string, Type, object> DeserializeWithResolver = (value, type) => JsonConvert.DeserializeObject(value, type, SerializerSettingsWithResolver);

        public static Func<string, Type, object> DeserializeWithConverter = (value, type) => JsonConvert.DeserializeObject(value, type, SerializerSettingsWithConverter);

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

        public static ActorNode NormalAddressActor = new ActorNode<int>()
        {
            Name = "Ellen Pompeo",
            Born = 1969,
            Address = new Address()
            {
                City = "Los Angeles",
                State = "California",
                Country = "US"
            }
        };

        public static void AddEntityTypes()
        {
            var entityTypes = new Type[] { typeof(PersonNode), typeof(DirectorNode), typeof(MovieNode), typeof(MovieExtraNode),
                typeof(ActorNode), typeof(Address), typeof(AddressWithComplexType), typeof(Location), typeof(AddressThirdLevel), typeof(SomeComplexType) };

            foreach (var entityType in entityTypes)
                Neo4jAnnotations.AddEntityType(entityType);
        }

        public static List<object[]> SerializerData { get; } = new List<object[]>()
        {
            new object[] { "ConverterSerializer", GraphClient.DefaultJsonContractResolver,
                new List<JsonConverter>(GraphClient.DefaultJsonConverters)
                {
                    Converter
                }},
            new object[] { "ResolverSerializer", TestUtilities.Resolver,
                new List<JsonConverter>(GraphClient.DefaultJsonConverters)},
        };

        public static void TestFinalPropertiesForEquality(Func<object, string> serializer,
            Dictionary<string, dynamic> expected, JObject finalProperties)
        {
            Assert.NotNull(finalProperties);

            foreach (var prop in finalProperties.Properties())
            {
                Assert.Contains(prop.Name, expected.Keys);

                var expectedValue = expected[prop.Name];

                Assert.NotNull(expectedValue);

                var propValueStr = serializer(prop.Value);

                if (expectedValue is string)
                    Assert.Equal(expectedValue, propValueStr);
                else
                    Assert.Equal(serializer(expectedValue), propValueStr);
            }
        }

        public static ICypherFluentQuery GetCypherQuery(DefaultContractResolver resolver, List<JsonConverter> converters,
            out IRawGraphClient client, out CustomJsonSerializer serializer)
        {
            client = Substitute.For<IRawGraphClient>();

            serializer = new CustomJsonSerializer() { JsonConverters = converters, JsonContractResolver = resolver };
            client.Serializer.Returns(serializer);
            client.JsonContractResolver.Returns(resolver);

            return new CypherFluentQuery(client);
        }

        public static IPath BuildTestPath(IPathBuilder P)
        {
            return P
            .Pattern<MovieNode, MovieActorRelationship, ActorNode>("greysAnatomy", "acted_in", "ellenPompeo")
            .Label(new[] { "Series" }, new[] { "STARRED_IN" }, new[] { "Female" }, true, false, false)
            .Prop(() => new MovieNode() //could have used constrain, but would be good to test both methods
            {
                Title = "Grey's Anatomy",
                Year = 2017
            })
            .Hop(1) //not necessary, but for tests
            .Constrain(null, null, (actor) =>
                actor.Name == "Ellen Pompeo"
                && actor.Born == Params.Get<ActorNode>("shondaRhimes").Born
                && actor.Roles == new string[] { "Meredith Grey" });
        }
    }
}
