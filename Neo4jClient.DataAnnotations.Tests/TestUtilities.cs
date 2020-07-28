using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Tests.Models;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using NSubstitute;
using Neo4jClient.DataAnnotations.Cypher;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass)]

namespace Neo4jClient.DataAnnotations.Tests
{
    public static class TestUtilities
    {
        public static Type[] EntityTypes = new Type[]
        {
            typeof(PersonNode), typeof(DirectorNode), typeof(MovieNode), typeof(MovieExtraNode),
            typeof(ActorNode), typeof(Address), typeof(AddressWithComplexType), typeof(Location),
            typeof(AddressThirdLevel), typeof(SomeComplexType)
        };

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

        public static List<object[]> TestConverterContextData => new List<object[]>()
        {
            new object[] { "Converter", new ConverterTestContext() },
            new object[] { "BoltConverter", new ConverterTestContext().WithBoltClient() },
        };

        public static List<object[]> TestResolverContextData => new List<object[]>()
        {
            new object[] { "Resolver", new ResolverTestContext() },
            new object[] { "BoltResolver", new ResolverTestContext().WithBoltClient() },
        };

        public static List<object[]> TestContextData => TestConverterContextData.Union(TestResolverContextData).ToList();

        public static void TestFinalPropertiesForEquality(Func<object, string> serializer,
            Dictionary<string, dynamic> expected, JObject finalProperties)
        {
            Assert.NotNull(finalProperties);

            foreach (var prop in finalProperties.Properties())
            {
                if (prop.Name == Defaults.MetadataPropertyName)
                    continue; //accept this one.

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

        public static IPath BuildTestPathMixed(IPathBuilder P)
        {
            return P
            .Pattern<MovieNode, MovieActorRelationship, ActorNode>("greysAnatomy", "ellenPompeo")
            .Label(new[] { "Series" }, new[] { "STARRED_IN" }, new[] { "Female" }, true, false, false)
            .Prop(() => new MovieNode() //could have used constrain, but would be good to test both methods
            {
                Title = "Grey's Anatomy",
                Year = 2017
            })
            .Hop(1) //not necessary, but for tests
            .Constrain(null, null, (actor) =>
                actor.Name == "Ellen Pompeo"
                && actor.Born == CypherVariables.Get<ActorNode>("shondaRhimes").Born
                && actor.Roles == new string[] { "Meredith Grey" });
        }

        public static IPath BuildTestPath(IPathBuilder P)
        {
            return P
            .Pattern<MovieNode, MovieActorRelationship, ActorNode>("greysAnatomy", "ellenPompeo")
            .Label(new[] { "Series" }, new[] { "STARRED_IN" }, new[] { "Female" }, true, false, false)
            .Constrain((movie) => movie.Title == "Grey's Anatomy" && movie.Year == 2017, null, (actor) =>
                actor.Name == "Ellen Pompeo"
                && actor.Born == CypherVariables.Get<ActorNode>("shondaRhimes").Born
                && actor.Roles == new string[] { "Meredith Grey" })
            .Hop(1) //not necessary, but for tests
            ;
        }

        public static IPath BuildTestAlreadyBoundPath(IPathBuilder P)
        {
            return P
            .Pattern<MovieNode, MovieActorRelationship, ActorNode>("greysAnatomy", "ellenPompeo")
            .Label(new[] { "Series" }, new[] { "STARRED_IN" }, new[] { "Female" }, true, false, false)
            .Constrain((movie) => movie.Title == "Grey's Anatomy" && movie.Year == 2017, null, (actor) =>
                actor.Name == "Ellen Pompeo"
                && actor.Born == CypherVariables.Get<ActorNode>("shondaRhimes").Born
                && actor.Roles == new string[] { "Meredith Grey" })
            .Hop(1) //not necessary, but for tests
            .AlreadyBound(true, true, true)
            ;
        }

        public static TestContext WithBoltClient(this TestContext context)
        {
            var client = context.Client;

            if (!(client is IBoltGraphClient))
            {
                var boltClient = Substitute.For<ITestBoltGraphClient>();

                boltClient.Driver.Returns(Substitute.For<Neo4j.Driver.IDriver>());

                boltClient.JsonConverters.Returns(GraphClient.DefaultJsonConverters?.ToList() ?? new List<JsonConverter>());
                boltClient.JsonContractResolver.Returns(GraphClient.DefaultJsonContractResolver);

                client = boltClient as IRawGraphClient;
                client.Serializer.Returns((info) =>
                {
                    return new CustomJsonSerializer()
                    {
                        JsonContractResolver = boltClient.JsonContractResolver,
                        JsonConverters = boltClient.JsonConverters
                    };
                });

                client.IsConnected.Returns(true);
                client.ServerVersion.Returns(new Version(3, 1));
                client.CypherCapabilities.Returns(CypherCapabilities.Cypher30);

                context.Client = client;
            }

            return context;
        }
    }

    public interface ITestBoltGraphClient : IBoltGraphClient, IRawGraphClient
    {
        public Neo4j.Driver.IDriver Driver { get; set; }
    }
}
