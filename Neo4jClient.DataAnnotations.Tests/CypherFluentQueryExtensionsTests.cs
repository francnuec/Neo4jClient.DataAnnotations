using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Tests.Models;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class CypherFluentQueryExtensionsTests
    {
        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void PatternNoParamsStrategy_GetPattern(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.GetPattern(pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic));

            var expected = "(greysAnatomy:Series { Title: \"Grey's Anatomy\", Year: 2017 })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void PatternWithParamsStrategy_GetPattern(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.GetPattern(PropertiesBuildStrategy.WithParams, pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic));

            var expected = "(greysAnatomy:Series $greysAnatomy)" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void PatternWithParamsForValuesStrategy_GetPattern(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.GetPattern(PropertiesBuildStrategy.WithParamsForValues, pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic));

            var expected = "(greysAnatomy:Series { Title: $greysAnatomy.Title, Year: $greysAnatomy.Year })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void Match(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.Match(pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic)).Query.QueryText;

            var expected = "MATCH (greysAnatomy:Series { Title: $greysAnatomy.Title, Year: $greysAnatomy.Year })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void OptionalMatch(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.OptionalMatch(pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic)).Query.QueryText;

            var expected = "OPTIONAL MATCH (greysAnatomy:Series { Title: $greysAnatomy.Title, Year: $greysAnatomy.Year })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void Merge(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.Merge(pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic)).Query.QueryText;

            var expected = "MERGE (greysAnatomy:Series { Title: $greysAnatomy.Title, Year: $greysAnatomy.Year })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void Create(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.Create(pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic)).Query.QueryText;

            //This scenario is probably unlikely in a real neo4j situation, but for tests sakes.
            var expected = "CREATE (greysAnatomy:Series $greysAnatomy)" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void CreateUnique(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.CreateUnique(pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic)).Query.QueryText;

            //This scenario is probably unlikely in a real neo4j situation, but for tests sakes.
            var expected = "CREATE UNIQUE (greysAnatomy:Series $greysAnatomy)" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void SetPredicate(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.Set((ActorNode actor) =>
                actor.Name == "Ellen Pompeo"
                && actor.Born == Vars.Get<ActorNode>("shondaRhimes").Born
                && actor.Roles == new string[] { "Meredith Grey" }).Query.QueryText;

            var expected = "SET actor.Name = \"Ellen Pompeo\", actor.Born = shondaRhimes.Born, actor.Roles = [\r\n  \"Meredith Grey\"\r\n]";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void SetPredicateWithParams(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.Set((ActorNode actor) =>
                actor.Name == "Ellen Pompeo"
                && actor.Born == 1969
                && actor.Roles == new string[] { "Meredith Grey" },
                PropertiesBuildStrategy.WithParams, out var setParam).Query.QueryText;

            //When using Set predicate, WithParams strategy is the same as WithParamsForValues (except of course, an inline variable was encountered, then it becomes NoParams)
            var expected = $"SET actor.Name = ${setParam}.Name, actor.Born = ${setParam}.Born, actor.Roles = ${setParam}.Roles";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void SetProperties(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.Set("movie", () => new MovieNode()
            {
                Title = "Grey's Anatomy",
                Year = 2017
            }, out var setParam).Query.QueryText;

            var expected = $"SET movie = ${setParam}";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void SetPropertiesAdd(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.Set("movie", () => new MovieNode()
            {
                Title = "Grey's Anatomy",
                Year = 2017
            }, add: true, setParameter: out var setParam).Query.QueryText;

            var expected = $"SET movie += ${setParam}";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void SetPropertiesWithParamsForValues(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.Set("movie", () => new MovieNode()
            {
                Title = "Grey's Anatomy",
                Year = 2017
            }, PropertiesBuildStrategy.WithParamsForValues, out var setParam).Query.QueryText;

            var expected = $"SET movie = {{ Title: ${setParam}.Title, Year: ${setParam}.Year }}";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void SetPropertiesAddNoParams(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.Set("movie", () => new MovieNode()
            {
                Title = "Grey's Anatomy",
                Year = 2017
            }, true, PropertiesBuildStrategy.NoParams).Query.QueryText;

            var expected = $"SET movie += {{ Title: \"Grey's Anatomy\", Year: 2017 }}";

            Assert.Equal(expected, actual);
        }
    }
}
