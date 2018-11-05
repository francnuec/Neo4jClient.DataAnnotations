using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Tests.Models;
using NSubstitute;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Neo4jClient.DataAnnotations.Serialization;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class PatternTests
    {
        [Fact]
        public void ABTypes_ABSelector()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            //(A:Movie)-[r]-(B:Director)
            var pattern = builder.Pattern<MovieNode, DirectorNode>("A", "B").Pattern as Pattern;

            var abSelector = pattern.ABSelector;
            Assert.NotNull(abSelector);
            Assert.Equal("A", abSelector.Parameters[0].Name);

            var propInfo = Utils.Utilities.GetPropertyInfoFrom(abSelector.Body, typeof(MovieNode), typeof(DirectorNode));
            Assert.NotNull(propInfo);
            Assert.Equal(typeof(DirectorNode), propInfo.PropertyType);
            Assert.Equal("Director", propInfo.Name);
        }

        [Fact]
        public void ARSelector_RBSelector()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var pattern = builder.Pattern<MovieNode, MovieActorRelationship, ActorNode>((A) => A.Actors).Pattern as Pattern;

            var rbSelector = pattern.RBSelector;
            Assert.NotNull(rbSelector);

            var propInfo = Utils.Utilities.GetPropertyInfoFrom(rbSelector.Body, typeof(MovieActorRelationship), typeof(ActorNode));
            Assert.NotNull(propInfo);
            Assert.Equal(typeof(ActorNode), propInfo.PropertyType);
            Assert.Equal("Actor", propInfo.Name);
        }

        [Fact]
        public void ARBTypes_ARandRBSelectors()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var pattern = builder.Pattern<MovieNode, MovieActorRelationship, ActorNode>("Lagos").Pattern as Pattern;

            var arSelector = pattern.ARSelector;
            Assert.NotNull(arSelector);
            Assert.Equal("Lagos", arSelector.Parameters[0].Name);

            var rPropInfo = Utils.Utilities.GetPropertyInfoFrom(arSelector.Body, typeof(MovieNode), typeof(MovieActorRelationship));
            Assert.NotNull(rPropInfo);
            Assert.Equal(typeof(ICollection<MovieActorRelationship>), rPropInfo.PropertyType);
            Assert.Equal("Actors", rPropInfo.Name);

            var rbSelector = pattern.RBSelector;
            Assert.NotNull(rbSelector);

            var bPropInfo = Utils.Utilities.GetPropertyInfoFrom(rbSelector.Body, typeof(MovieActorRelationship), typeof(ActorNode));
            Assert.NotNull(bPropInfo);
            Assert.Equal(typeof(ActorNode), bPropInfo.PropertyType);
            Assert.Equal("Actor", bPropInfo.Name);
        }

        [Fact]
        public void ABTypes_RType()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

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

            var rPropInfo = Utils.Utilities.GetPropertyInfoFrom(arSelector.Body, typeof(ActorNode), typeof(MovieActorRelationship));
            Assert.NotNull(rPropInfo);
            Assert.Equal(typeof(ICollection<MovieActorRelationship>), rPropInfo.PropertyType);
            Assert.Equal("Movies", rPropInfo.Name);

            var rbSelector = pattern.RBSelector;
            Assert.NotNull(rbSelector);

            var bPropInfo = Utils.Utilities.GetPropertyInfoFrom(rbSelector.Body, typeof(MovieActorRelationship), typeof(MovieNode));
            Assert.NotNull(bPropInfo);
            Assert.Equal(typeof(MovieNode), bPropInfo.PropertyType);
            Assert.Equal("Movie", bPropInfo.Name);
        }

        [Fact]
        public void RBTypes_AType()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

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
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

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
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var pattern = builder.Pattern<MovieNode, MovieActorRelationship, CypherObject>("Lagos", "rel", null).Pattern as Pattern;

            var rTypes = pattern.RTypes;

            Assert.NotNull(rTypes);
            Assert.Equal(1, rTypes.Count());
            Assert.Equal("ACTED_IN", rTypes.Single());
        }

        [Fact]
        public void ForeignKeyAttribute_RTypes()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var pattern = builder.Pattern<MovieNode, WriterNode>("Lagos").Pattern as Pattern;

            var rTypes = pattern.RTypes;

            Assert.NotNull(rTypes);
            Assert.Equal(1, rTypes.Count());
            Assert.Equal("WROTE", rTypes.Single());
        }

        [Fact]
        public void ColumnAttributeWithName_RTypes()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var pattern = builder.Pattern<MovieNode, DirectorNode>("Lagos").Pattern as Pattern;

            var rTypes = pattern.RTypes;

            Assert.NotNull(rTypes);
            Assert.Equal(1, rTypes.Count());
            Assert.Equal("DIRECTED", rTypes.Single());
        }

        [Fact]
        public void NoNameAttributes_RTypes()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var pattern = builder.Pattern<MovieNode, MovieExtraNode>("Lagos", RelationshipDirection.Incoming).Pattern as Pattern;

            var rTypes = pattern.RTypes;

            Assert.NotNull(rTypes);
            Assert.Equal(1, rTypes.Count());
            Assert.Equal("Movies", rTypes.Single());
        }

        [Fact]
        public void UserSetRTypes_RTypes()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

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
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var testTypes = new List<string>() { "Test", "Relationship" };

            var pattern = builder.Pattern<MovieNode, MovieExtraNode>("Lagos", RelationshipDirection.Incoming)
                .Label(null, testTypes, null, replaceA: false, replaceR: true, replaceB: false) //replaceR ensures that no other values except the ones supplied is used.
                .Pattern as Pattern;

            var rTypes = pattern.RTypes;

            Assert.NotNull(rTypes);
            Assert.Equal(2, rTypes.Count());
            Assert.Equal(testTypes, rTypes);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void Properties_FinalProperties(string testContextName, TestContext testContext)
        {
            var client = testContext.AnnotationsContext.GraphClient;
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var pattern = builder
                .Pattern<ActorNode>("Ambode")
                .Prop(() => new
                {
                    Name = "Ellen Pompeo",
                    Born = Vars.Get<ActorNode>("shondaRhimes").Born,
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

            TestUtilities.TestFinalPropertiesForEquality((instance) => client.Serializer.Serialize(instance), expected, aFinalProperties);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void Constraints_FinalProperties(string testContextName, TestContext testContext)
        {
            var client = testContext.AnnotationsContext.GraphClient;
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var pattern = builder
                .Pattern<ActorNode>("Ambode")
                .Constrain((actor) =>
                    actor.Name == "Ellen Pompeo"
                    && actor.Born == Vars.Get<ActorNode>("shondaRhimes").Born
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

            TestUtilities.TestFinalPropertiesForEquality((instance) => client.Serializer.Serialize(instance), expected, aFinalProperties);

            var str = pattern.Build(ref query);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void NoParamsStrategy_Build(string testContextName, TestContext testContext)
        {
            var client = testContext.AnnotationsContext.GraphClient;
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var path = TestUtilities.BuildTestPath(builder)
                .Extend(RelationshipDirection.Outgoing)
                as Path;

            var expectedMain = "(greysAnatomy:Series { Title: \"Grey's Anatomy\", Year: 2017 })" +
                "<-[:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor:Person { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })";
            var expectedExt = "-->()";

            var actualMain = path.Patterns[0].Build(ref query);
            var actualExt = path.Patterns[1].Build(ref query);

            Assert.Equal(expectedMain, actualMain);

            Assert.Equal(expectedExt, actualExt);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void WithParamsStrategy_Build(string testContextName, TestContext testContext)
        {
            var client = testContext.AnnotationsContext.GraphClient;
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var path = TestUtilities.BuildTestPathMixed(builder)
                .Extend(RelationshipDirection.Outgoing)
                as Path;

            path.Patterns.ForEach(p => p.BuildStrategy = PropertiesBuildStrategy.WithParams);

            var expectedMain = "(greysAnatomy:Series $greysAnatomy)" +
                "<-[:STARRED_IN|ACTED_IN*1]-" +
                //becase of the inner shondaRhimes variable, it would use the WithParamsForValues strategy instead for the ellenPompeo props.
                "(ellenPompeo:Female:Actor:Person { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })";
            var expectedExt = "-->()";

            var actualMain = path.Patterns[0].Build(ref query);
            var actualExt = path.Patterns[1].Build(ref query);

            Assert.Equal(expectedMain, actualMain);

            Assert.Equal(expectedExt, actualExt);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void WithParamsForValuesStrategy_Build(string testContextName, TestContext testContext)
        {
            var client = testContext.AnnotationsContext.GraphClient;
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var path = TestUtilities.BuildTestPath(builder)
                .Extend(RelationshipDirection.Outgoing)
                as Path;

            path.Patterns.ForEach(p => p.BuildStrategy = PropertiesBuildStrategy.WithParamsForValues);

            var expectedMain = "(greysAnatomy:Series { Title: $greysAnatomy.Title, Year: $greysAnatomy.Year })" +
                "<-[:STARRED_IN|ACTED_IN*1]-" +
                //becase of the inner shondaRhimes variable, it would use the WithParamsForValues strategy instead for the ellenPompeo props.
                "(ellenPompeo:Female:Actor:Person { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })";
            var expectedExt = "-->()";

            var actualMain = path.Patterns[0].Build(ref query);
            var actualExt = path.Patterns[1].Build(ref query);

            Assert.Equal(expectedMain, actualMain);

            Assert.Equal(expectedExt, actualExt);
        }
    }
}
