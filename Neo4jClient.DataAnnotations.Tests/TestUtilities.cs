using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Tests.Models;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Neo4jClient.DataAnnotations.Utils;
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
        public static Type[] EntityTypes = new Type[]
        {
            typeof(PersonNode), typeof(DirectorNode), typeof(MovieNode), typeof(MovieExtraNode),
            typeof(ActorNode), typeof(Address), typeof(AddressWithComplexType), typeof(Location),
            typeof(AddressThirdLevel), typeof(SomeComplexType)
        };

        //private static IEntityService entityService;
        //public static IEntityService EntityService
        //{
        //    get
        //    {
        //        if (entityService == null)
        //        {
        //            entityService = AnnotationsContext.CreateNewEntityService();
        //            entityService.AddEntityTypes(EntityTypes);
        //        }

        //        return entityService;
        //    }
        //}

        //public static IAnnotationsEntityService ResolverEntityService { get; } = EntityService; //new AnnotationsEntityService(EntityTypes);

        //public static IAnnotationsEntityService ConverterEntityService { get; } = EntityService; //new AnnotationsEntityService(EntityTypes);

        ////public static IAnnotationsContext ResolverContext { get; }

        ////public static IAnnotationsContext ConverterContext { get; }

        //private static EntityResolver Resolver => new EntityResolver();

        //private static EntityConverter Converter => new EntityConverter();

        //public static ResolverDummyConverter DummyConverter = Resolver.DummyConverter;

        //public static Func<object, string> SerializeWithResolver = (entity) => JsonConvert.SerializeObject(entity, SerializerSettingsWithResolver);

        //public static Func<object, string> SerializeWithConverter = (entity) => JsonConvert.SerializeObject(entity, SerializerSettingsWithConverter);

        //public static Func<string, Type, object> DeserializeWithResolver = (value, type) => JsonConvert.DeserializeObject(value, type, SerializerSettingsWithResolver);

        //public static Func<string, Type, object> DeserializeWithConverter = (value, type) => JsonConvert.DeserializeObject(value, type, SerializerSettingsWithConverter);

        //public static JsonSerializerSettings SerializerSettingsWithConverter(EntityConverter converter)
        //{
        //    converter = converter ?? Converter;

        //    return new JsonSerializerSettings()
        //    {
        //        Converters = new List<JsonConverter>() { Converter },
        //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        //    };
        //}

        //public static JsonSerializerSettings SerializerSettingsWithResolver(EntityResolver resolver)
        //{
        //    resolver = resolver ?? Resolver;

        //    return new JsonSerializerSettings()
        //    {
        //        Converters = new List<JsonConverter>() { resolver.DummyConverter },
        //        ContractResolver = resolver,
        //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        //    };
        //}

        //public static Func<object, string> GetResolverSerializer(EntityResolver resolver, out JsonSerializerSettings settings)
        //{
        //    var _settings = SerializerSettingsWithResolver(resolver);
        //    settings = _settings;
        //    Func<object, string> serializer = (entity) => JsonConvert.SerializeObject(entity, _settings);
        //    return serializer;
        //}

        //public static Func<object, string> GetConverterSerializer(EntityConverter converter, out JsonSerializerSettings settings)
        //{
        //    var _settings = SerializerSettingsWithConverter(converter);
        //    settings = _settings;
        //    Func<object, string> serializer = (entity) => JsonConvert.SerializeObject(entity, _settings);
        //    return serializer;
        //}

        //public static Func<string, Type, object> GetResolverDeserializer(EntityResolver resolver, out JsonSerializerSettings settings)
        //{
        //    var _settings = SerializerSettingsWithResolver(resolver);
        //    settings = _settings;
        //    Func<string, Type, object> serializer = (value, type) => JsonConvert.DeserializeObject(value, type, _settings);
        //    return serializer;
        //}

        //public static Func<string, Type, object> GetConverterDeserializer(EntityConverter converter, out JsonSerializerSettings settings)
        //{
        //    var _settings = SerializerSettingsWithConverter(converter);
        //    settings = _settings;
        //    Func<string, Type, object> serializer = (value, type) => JsonConvert.DeserializeObject(value, type, _settings);
        //    return serializer;
        //}

        //public static IAnnotationsContext RegisterEntityTypes(EntityResolver resolver, EntityConverter converter,
        //    out IRawGraphClient client/*, out CustomJsonSerializer serializer*/, out ICypherFluentQuery query)
        //{
        //    query = GetCypherQuery(out client/*, out serializer*/);
        //    return RegisterEntityTypes(client, resolver, converter);
        //}

        //public static IAnnotationsContext RegisterEntityTypes(IGraphClient graphClient, EntityResolver resolver, EntityConverter converter)
        //{
        //    //EntityService.AddEntityTypes(EntityTypes);
        //    return new AnnotationsContext(resolver != null ? ResolverEntityService : ConverterEntityService, graphClient, resolver, converter);
        //}

        //public static IAnnotationsContext AttachResolver(IGraphClient graphClient, IEnumerable<Type> types)
        //{
        //    return AttachResolver(graphClient, types, Resolver);
        //}

        //public static IAnnotationsContext AttachResolver(IGraphClient graphClient, IEnumerable<Type> types, EntityResolver resolver)
        //{
        //    var context = RegisterEntityTypes(graphClient, resolver, null);

        //    if (types != EntityTypes)
        //        ResolverEntityService.AddEntityTypes(types);

        //    return context;
        //}

        //public static IAnnotationsContext AttachConverter(IGraphClient graphClient, IEnumerable<Type> types)
        //{
        //    return AttachConverter(graphClient, types, Converter);
        //}

        //public static IAnnotationsContext AttachConverter(IGraphClient graphClient, IEnumerable<Type> types, EntityConverter converter)
        //{
        //    var context = RegisterEntityTypes(graphClient, null, converter);

        //    if (types != EntityTypes)
        //        ResolverEntityService.AddEntityTypes(types);

        //    return context;
        //}

        //public static ICypherFluentQuery GetCypherQuery(out IRawGraphClient clientSubstitute/*, out CustomJsonSerializer serializer*/)
        //{
        //    var substitute = clientSubstitute = Substitute.For<IRawGraphClient>();

        //    //serializer = new CustomJsonSerializer()
        //    //{
        //    //    JsonConverters = GraphClient.DefaultJsonConverters.Append(Converter).ToList(),
        //    //    JsonContractResolver = Resolver //GraphClient.DefaultJsonContractResolver
        //    //};

        //    //clientSubstitute.Serializer.Returns(serializer);
        //    //clientSubstitute.JsonConverters.Returns(serializer.JsonConverters);
        //    //clientSubstitute.JsonContractResolver.Returns(serializer.JsonContractResolver); //GraphClient.DefaultJsonContractResolver);

        //    clientSubstitute.JsonConverters.Returns(GraphClient.DefaultJsonConverters.ToList());
        //    clientSubstitute.JsonContractResolver.Returns(GraphClient.DefaultJsonContractResolver);

        //    clientSubstitute.Serializer.Returns((info) =>
        //    {
        //        return new CustomJsonSerializer()
        //        {
        //            JsonContractResolver = substitute.JsonContractResolver,
        //            JsonConverters = substitute.JsonConverters
        //        };
        //    });



        //    return new CypherFluentQuery(clientSubstitute);
        //}

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

        public static List<object[]> TestContextData => new List<object[]>()
        {
            new object[] { "Converter", new ConverterTestContext() },
            new object[] { "Resolver", new ResolverTestContext() },
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
    }
}
