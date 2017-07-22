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

            var actual = query.GetPattern(PatternBuildStrategy.WithParams, pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic));

            var expected = "(greysAnatomy:Series { greysAnatomy })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { ellenPompeo })" +
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

            var actual = query.GetPattern(PatternBuildStrategy.WithParamsForValues, pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic));

            var expected = "(greysAnatomy:Series { Title: {greysAnatomy}.Title, Year: {greysAnatomy}.Year })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: {ellenPompeo}.Name, Born: {ellenPompeo}.Born, Roles: {ellenPompeo}.Roles })" +
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

            var expected = "MATCH (greysAnatomy:Series { Title: {greysAnatomy}.Title, Year: {greysAnatomy}.Year })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: {ellenPompeo}.Name, Born: {ellenPompeo}.Born, Roles: {ellenPompeo}.Roles })" +
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

            var expected = "OPTIONAL MATCH (greysAnatomy:Series { Title: {greysAnatomy}.Title, Year: {greysAnatomy}.Year })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: {ellenPompeo}.Name, Born: {ellenPompeo}.Born, Roles: {ellenPompeo}.Roles })" +
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

            var expected = "MERGE (greysAnatomy:Series { Title: {greysAnatomy}.Title, Year: {greysAnatomy}.Year })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: {ellenPompeo}.Name, Born: {ellenPompeo}.Born, Roles: {ellenPompeo}.Roles })" +
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
            var expected = "CREATE (greysAnatomy:Series { greysAnatomy })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { ellenPompeo })" +
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
            var expected = "CREATE UNIQUE (greysAnatomy:Series { greysAnatomy })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { ellenPompeo })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }
    }
}
