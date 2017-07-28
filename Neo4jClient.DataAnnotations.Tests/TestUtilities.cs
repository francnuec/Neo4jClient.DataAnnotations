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

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass)]

namespace Neo4jClient.DataAnnotations.Tests
{
    public class TestUtilities
    {
        public static EntityResolver Resolver { get; } = new EntityResolver();

        public static EntityConverter Converter { get; } = new EntityConverter();

        public static ResolverDummyConverter DummyConverter { get; } = new ResolverDummyConverter();

        public static JsonSerializerSettings SerializerSettingsWithConverter = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() { Converter },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        public static JsonSerializerSettings SerializerSettingsWithResolver = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() { DummyConverter },
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

        public static Type[] EntityTypes = new Type[]
        {
            typeof(PersonNode), typeof(DirectorNode), typeof(MovieNode), typeof(MovieExtraNode),
            typeof(ActorNode), typeof(Address), typeof(AddressWithComplexType), typeof(Location),
            typeof(AddressThirdLevel), typeof(SomeComplexType)
        };

        //public static void AddEntityTypes()
        //{
        //    foreach (var entityType in EntityTypes)
        //    {
        //        Neo4jAnnotations.AddEntityType(entityType);
        //    }
        //}

        public static void RegisterEntityTypes(EntityResolver resolver, EntityConverter converter)
        {
            if (resolver != null)
            {
                Neo4jAnnotations.RegisterWithResolver(EntityTypes, resolver);
            }
            else
            {
                Neo4jAnnotations.RegisterWithConverter(EntityTypes, converter);
            }
        }

        public static List<object[]> SerializerData { get; } = new List<object[]>()
        {
            new object[] { "Converter", null, Converter },
            new object[] { "Resolver", Resolver, null},
        };

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

        public static ICypherFluentQuery GetCypherQuery(out IRawGraphClient clientSubstitute, out CustomJsonSerializer serializer)
        {
            clientSubstitute = Substitute.For<IRawGraphClient>();

            serializer = new CustomJsonSerializer() { JsonConverters = GraphClient.DefaultJsonConverters,
                JsonContractResolver = GraphClient.DefaultJsonContractResolver };
            clientSubstitute.Serializer.Returns(serializer);
            clientSubstitute.JsonContractResolver.Returns(GraphClient.DefaultJsonContractResolver);

            return new CypherFluentQuery(clientSubstitute);
        }

        public static IPath BuildTestPathMixed(IPathBuilder P)
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
                && actor.Born == Vars.Get<ActorNode>("shondaRhimes").Born
                && actor.Roles == new string[] { "Meredith Grey" });
        }

        public static IPath BuildTestPath(IPathBuilder P)
        {
            return P
            .Pattern<MovieNode, MovieActorRelationship, ActorNode>("greysAnatomy", "acted_in", "ellenPompeo")
            .Label(new[] { "Series" }, new[] { "STARRED_IN" }, new[] { "Female" }, true, false, false)
            .Constrain((movie) => movie.Title == "Grey's Anatomy" && movie.Year == 2017, null, (actor) =>
                actor.Name == "Ellen Pompeo"
                && actor.Born == Vars.Get<ActorNode>("shondaRhimes").Born
                && actor.Roles == new string[] { "Meredith Grey" })
            .Hop(1) //not necessary, but for tests
            ;
        }
    }
}
