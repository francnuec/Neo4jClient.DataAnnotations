using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Tests.Models;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class PatternTests
    {
        [Fact]
        public void ABTypes_ABSelector()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath(null, query);

            //(A:Movie)-[r]-(B:Director)
            var pattern = builder.Pattern<MovieNode, DirectorNode>("A", "B").Pattern as Pattern;

            var abSelector = pattern.ABSelector;
            Assert.NotNull(abSelector);
            Assert.Equal("A", abSelector.Parameters[0].Name);

            var propInfo = Utilities.GetPropertyInfo(abSelector, typeof(MovieNode), typeof(DirectorNode));
            Assert.NotNull(propInfo);
            Assert.Equal(typeof(DirectorNode), propInfo.PropertyType);
            Assert.Equal("Director", propInfo.Name);
        }

        [Fact]
        public void ARSelector_RBSelector()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath(null, query);

            var pattern = builder.Pattern<MovieNode, MovieActorRelationship, ActorNode>((A) => A.Actors).Pattern as Pattern;

            var rbSelector = pattern.RBSelector;
            Assert.NotNull(rbSelector);

            var propInfo = Utilities.GetPropertyInfo(rbSelector, typeof(MovieActorRelationship), typeof(ActorNode));
            Assert.NotNull(propInfo);
            Assert.Equal(typeof(ActorNode), propInfo.PropertyType);
            Assert.Equal("Actor", propInfo.Name);
        }

        [Fact]
        public void ARBTypes_ARandRBSelectors()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath(null, query);

            var pattern = builder.Pattern<MovieNode, MovieActorRelationship, ActorNode>("Lagos").Pattern as Pattern;

            var arSelector = pattern.ARSelector;
            Assert.NotNull(arSelector);
            Assert.Equal("Lagos", arSelector.Parameters[0].Name);

            var rPropInfo = Utilities.GetPropertyInfo(arSelector, typeof(MovieNode), typeof(MovieActorRelationship));
            Assert.NotNull(rPropInfo);
            Assert.Equal(typeof(ICollection<MovieActorRelationship>), rPropInfo.PropertyType);
            Assert.Equal("Actors", rPropInfo.Name);

            var rbSelector = pattern.RBSelector;
            Assert.NotNull(rbSelector);

            var bPropInfo = Utilities.GetPropertyInfo(rbSelector, typeof(MovieActorRelationship), typeof(ActorNode));
            Assert.NotNull(bPropInfo);
            Assert.Equal(typeof(ActorNode), bPropInfo.PropertyType);
            Assert.Equal("Actor", bPropInfo.Name);
        }

        [Fact]
        public void ABTypes_RType()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath(null, query);

            var pattern = builder.Pattern<ActorNode, MovieNode>("Ambode", "Lagos").Pattern as Pattern;

            var rType = pattern.RType;
            Assert.NotNull(rType);
            Assert.Equal(typeof(MovieActorRelationship), rType);

            var dir = pattern.Direction;
            Assert.NotNull(dir);
            Assert.Equal(RelationshipDirection.Outgoing, dir.Value);

            //******

            var arSelector = pattern.ARSelector;
            Assert.NotNull(arSelector);
            Assert.Equal("Ambode", arSelector.Parameters[0].Name);

            var rPropInfo = Utilities.GetPropertyInfo(arSelector, typeof(ActorNode), typeof(MovieActorRelationship));
            Assert.NotNull(rPropInfo);
            Assert.Equal(typeof(ICollection<MovieActorRelationship>), rPropInfo.PropertyType);
            Assert.Equal("Movies", rPropInfo.Name);

            var rbSelector = pattern.RBSelector;
            Assert.NotNull(rbSelector);

            var bPropInfo = Utilities.GetPropertyInfo(rbSelector, typeof(MovieActorRelationship), typeof(MovieNode));
            Assert.NotNull(bPropInfo);
            Assert.Equal(typeof(MovieNode), bPropInfo.PropertyType);
            Assert.Equal("Movie", bPropInfo.Name);
        }

        [Fact]
        public void RBTypes_AType()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath(null, query);

            var pattern = builder.Pattern<WriterNode, MovieNode>("Tinubu", "Lagos")
                .Extend<MovieActorRelationship, ActorNode>("Ambode", RelationshipDirection.Incoming)
                .Pattern as Pattern;

            var aType = pattern.AType;
            Assert.NotNull(aType);
            Assert.Equal(typeof(MovieNode), aType);

            var dir = pattern.Direction;
            Assert.NotNull(dir);
            Assert.Equal(RelationshipDirection.Incoming, dir.Value);

            //******

            var arSelector = pattern.ARSelector;
            Assert.NotNull(arSelector);
            Assert.Equal("Lagos", arSelector.Parameters[0].Name);
        }

        [Fact]
        public void ARTypes_BType()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath(null, query);

            var pattern = builder.Pattern<MovieNode, MovieActorRelationship, CypherObject>("Lagos", "rel", null).Pattern as Pattern;

            var bType = pattern.BType;
            Assert.NotNull(bType);
            Assert.Equal(typeof(ActorNode), bType);

            var dir = pattern.Direction;
            Assert.NotNull(dir);
            Assert.Equal(RelationshipDirection.Incoming, dir.Value);

            //******

            var rbSelector = pattern.RBSelector;
            Assert.NotNull(rbSelector);
            Assert.Equal("rel", rbSelector.Parameters[0].Name);
        }

        [Fact]
        public void TableAttribute_RTypes()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath(null, query);

            var pattern = builder.Pattern<MovieNode, MovieActorRelationship, CypherObject>("Lagos", "rel", null).Pattern as Pattern;

            var rTypes = pattern.RTypes;

            Assert.NotNull(rTypes);
            Assert.Equal(1, rTypes.Count());
            Assert.Equal("ACTED_IN", rTypes.Single());
        }

        [Fact]
        public void ForeignKeyAttribute_RTypes()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath(null, query);

            var pattern = builder.Pattern<MovieNode, WriterNode>("Lagos").Pattern as Pattern;

            var rTypes = pattern.RTypes;

            Assert.NotNull(rTypes);
            Assert.Equal(1, rTypes.Count());
            Assert.Equal("WROTE", rTypes.Single());
        }

        [Fact]
        public void ColumnAttributeWithName_RTypes()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath(null, query);

            var pattern = builder.Pattern<MovieNode, DirectorNode>("Lagos").Pattern as Pattern;

            var rTypes = pattern.RTypes;

            Assert.NotNull(rTypes);
            Assert.Equal(1, rTypes.Count());
            Assert.Equal("DIRECTED", rTypes.Single());
        }

        [Fact]
        public void NoNameAttributes_RTypes()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath(null, query);

            var pattern = builder.Pattern<MovieNode, MovieExtraNode>("Lagos", RelationshipDirection.Incoming).Pattern as Pattern;

            var rTypes = pattern.RTypes;

            Assert.NotNull(rTypes);
            Assert.Equal(1, rTypes.Count());
            Assert.Equal("Movies", rTypes.Single());
        }

        [Fact]
        public void UserSetRTypes_RTypes()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath(null, query);

            var testTypes = new List<string>() { "Test", "Relationship" };

            var pattern = builder.Pattern<MovieNode, MovieExtraNode>("Lagos", RelationshipDirection.Incoming)
                .Label(null, testTypes, null)
                .Pattern as Pattern;

            var rTypes = pattern.RTypes;

            Assert.NotNull(rTypes);
            Assert.Equal(3, rTypes.Count());

            testTypes.Add("Movies");
            Assert.Equal(testTypes, rTypes);
        }

        [Fact]
        public void UserSetExactRTypes_RTypes()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath(null, query);

            var testTypes = new List<string>() { "Test", "Relationship" };

            var pattern = builder.Pattern<MovieNode, MovieExtraNode>("Lagos", RelationshipDirection.Incoming)
                .Label(null, testTypes, null, replaceA: false, replaceR: true, replaceB: false) //replaceR ensures that no other values except the ones supplied is used.
                .Pattern as Pattern;

            var rTypes = pattern.RTypes;

            Assert.NotNull(rTypes);
            Assert.Equal(2, rTypes.Count());
            Assert.Equal(testTypes, rTypes);
        }


        public static List<object[]> SerializerData { get; } = new List<object[]>()
        {
            new object[] { "ConverterSerializer", GraphClient.DefaultJsonContractResolver,
                new List<JsonConverter>(GraphClient.DefaultJsonConverters)
                {
                    TestUtilities.Converter
                }},
            new object[] { "ResolverSerializer", TestUtilities.Resolver,
                new List<JsonConverter>(GraphClient.DefaultJsonConverters)},
        };

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(PatternTests))]
        public void Properties_FinalProperties(string serializerName, DefaultContractResolver resolver, List<JsonConverter> converters)
        {
            TestUtilities.AddEntityTypes();

            var client = Substitute.For<IRawGraphClient>();

            var serializer = new CustomJsonSerializer() { JsonConverters = converters, JsonContractResolver = resolver };
            client.Serializer.Returns(serializer);
            client.JsonContractResolver.Returns(resolver);

            var query = new CypherFluentQuery(client);

            var builder = new DummyPath(null, query);

            var pattern = builder
                .Pattern<ActorNode>("Ambode")
                .Prop(() => new
                {
                    Name = "Ellen Pompeo",
                    Born = Params.Get<ActorNode>("shondaRhimes").Born,
                    Roles = new string[] { "Meredith Grey" },
                    Age = 47.ToString()
                })
                .Pattern as Pattern;

            var aProperties = pattern.AProperties;
            Assert.NotNull(aProperties);

            var aFinalProperties = pattern.AFinalProperties;
            Assert.NotNull(aFinalProperties);

            var expected = new Dictionary<string, dynamic>()
            {
                { "Name", "\"Ellen Pompeo\"" },
                { "Born", "shondaRhimes.Born" },
                { "Roles", "[\r\n  \"Meredith Grey\"\r\n]" },
                { "Age", "\"47\"" }, //because we assigned a 47 string and not a 47 int.
            };

            TestUtilities.TestFinalPropertiesForEquality((instance) => serializer.Serialize(instance), expected, aFinalProperties);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(PatternTests))]
        public void Constraints_FinalProperties(string serializerName, DefaultContractResolver resolver, List<JsonConverter> converters)
        {
            TestUtilities.AddEntityTypes();

            var client = Substitute.For<IRawGraphClient>();

            var serializer = new CustomJsonSerializer() { JsonConverters = converters, JsonContractResolver = resolver };
            client.Serializer.Returns(serializer);
            client.JsonContractResolver.Returns(resolver);

            var query = new CypherFluentQuery(client);

            var builder = new DummyPath(null, query);

            var pattern = builder
                .Pattern<ActorNode>("Ambode")
                .Constrain((actor) =>
                    actor.Name == "Ellen Pompeo"
                    && actor.Born == Params.Get<ActorNode>("shondaRhimes").Born
                    && actor.Roles == new string[] { "Meredith Grey" })
                .Pattern as Pattern;

            var aConstraints = pattern.AConstraints;
            Assert.NotNull(aConstraints);

            var aFinalProperties = pattern.AFinalProperties;
            Assert.NotNull(aFinalProperties);

            var expected = new Dictionary<string, dynamic>()
            {
                { "Name", "\"Ellen Pompeo\"" },
                { "Born", "shondaRhimes.Born" },
                { "Roles", "[\r\n  \"Meredith Grey\"\r\n]" }
            };

            TestUtilities.TestFinalPropertiesForEquality((instance) => serializer.Serialize(instance), expected, aFinalProperties);

            var str = pattern.Build();
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(PatternTests))]
        public void Build_NoParamsStrategy(string serializerName, DefaultContractResolver resolver, List<JsonConverter> converters)
        {
            TestUtilities.AddEntityTypes();

            var client = Substitute.For<IRawGraphClient>();

            var serializer = new CustomJsonSerializer() { JsonConverters = converters, JsonContractResolver = resolver };
            client.Serializer.Returns(serializer);
            client.JsonContractResolver.Returns(resolver);

            var query = new CypherFluentQuery(client);

            var builder = new DummyPath(null, query);

            var path = builder
                .Pattern<MovieNode, MovieActorRelationship, ActorNode>("greysAnatomy", "acted_in", "ellenPompeo")
                .Label(A: new[] { "Series" }, R: new[] { "STARRED_IN" }, B: new[] { "Female" }, replaceA: true, replaceR: false, replaceB: false)
                .Prop(() => new MovieNode() //could have used constrain, but would be good to test both methods
                {
                    Title = "Grey's Anatomy",
                    Year = 2017
                })
                .Hop(1) //not necessary, but for tests
                .Constrain(null, null, (actor) =>
                    actor.Name == "Ellen Pompeo"
                    && actor.Born == Params.Get<ActorNode>("shondaRhimes").Born
                    && actor.Roles == new string[] { "Meredith Grey" })
                .Extend(RelationshipDirection.Outgoing)
                as Path;

            var expectedMain = "(greysAnatomy:Series { Title: \"Grey's Anatomy\", Year: 2017 })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })";
            var expectedExt = "-->()";

            var actualMain = path.Patterns[0].Build();
            var actualExt = path.Patterns[1].Build();

            Assert.Equal(expectedMain, actualMain);

            Assert.Equal(expectedExt, actualExt);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(PatternTests))]
        public void Build_WithParamsStrategy(string serializerName, DefaultContractResolver resolver, List<JsonConverter> converters)
        {
            TestUtilities.AddEntityTypes();

            var client = Substitute.For<IRawGraphClient>();

            var serializer = new CustomJsonSerializer() { JsonConverters = converters, JsonContractResolver = resolver };
            client.Serializer.Returns(serializer);
            client.JsonContractResolver.Returns(resolver);

            var query = new CypherFluentQuery(client);

            var builder = new DummyPath(null, query);

            var path = builder
                .Pattern<MovieNode, MovieActorRelationship, ActorNode>("greysAnatomy", "acted_in", "ellenPompeo")
                .Label(A: new[] { "Series" }, R: new[] { "STARRED_IN" }, B: new[] { "Female" }, replaceA: true, replaceR: false, replaceB: false)
                .Prop(() => new MovieNode() //could have used constrain, but would be good to test both methods
                {
                    Title = "Grey's Anatomy",
                    Year = 2017
                })
                .Hop(1) //not necessary, but for tests
                .Constrain(null, null, (actor) =>
                    actor.Name == "Ellen Pompeo"
                    && actor.Born == Params.Get<ActorNode>("shondaRhimes").Born
                    && actor.Roles == new string[] { "Meredith Grey" })
                .Extend(RelationshipDirection.Outgoing)
                as Path;

            path.Patterns.ForEach(p => p.BuildStrategy = PatternBuildStrategy.WithParams);

            var expectedMain = "(greysAnatomy:Series { greysAnatomy })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { ellenPompeo })";
            var expectedExt = "-->()";

            var actualMain = path.Patterns[0].Build();
            var actualExt = path.Patterns[1].Build();

            Assert.Equal(expectedMain, actualMain);

            Assert.Equal(expectedExt, actualExt);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(PatternTests))]
        public void Build_WithParamsForValuesStrategy(string serializerName, DefaultContractResolver resolver, List<JsonConverter> converters)
        {
            TestUtilities.AddEntityTypes();

            var client = Substitute.For<IRawGraphClient>();

            var serializer = new CustomJsonSerializer() { JsonConverters = converters, JsonContractResolver = resolver };
            client.Serializer.Returns(serializer);
            client.JsonContractResolver.Returns(resolver);

            var query = new CypherFluentQuery(client);

            var builder = new DummyPath(null, query);

            var path = builder
                .Pattern<MovieNode, MovieActorRelationship, ActorNode>("greysAnatomy", "acted_in", "ellenPompeo")
                .Label(A: new[] { "Series" }, R: new[] { "STARRED_IN" }, B: new[] { "Female" }, replaceA: true, replaceR: false, replaceB: false)
                .Prop(() => new MovieNode() //could have used constrain, but would be good to test both methods
                {
                    Title = "Grey's Anatomy",
                    Year = 2017
                })
                .Hop(1) //not necessary, but for tests
                .Constrain(null, null, (actor) =>
                    actor.Name == "Ellen Pompeo"
                    && actor.Born == Params.Get<ActorNode>("shondaRhimes").Born
                    && actor.Roles == new string[] { "Meredith Grey" })
                .Extend(RelationshipDirection.Outgoing)
                as Path;

            path.Patterns.ForEach(p => p.BuildStrategy = PatternBuildStrategy.WithParamsForValues);

            var expectedMain = "(greysAnatomy:Series { Title: {greysAnatomy}.Title, Year: {greysAnatomy}.Year })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: {ellenPompeo}.Name, Born: {ellenPompeo}.Born, Roles: {ellenPompeo}.Roles })";
            var expectedExt = "-->()";

            var actualMain = path.Patterns[0].Build();
            var actualExt = path.Patterns[1].Build();

            Assert.Equal(expectedMain, actualMain);

            Assert.Equal(expectedExt, actualExt);
        }
    }
}
