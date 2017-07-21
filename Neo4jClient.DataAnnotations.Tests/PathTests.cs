using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Tests.Models;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Reflection;
using System.Linq.Expressions;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class PathTests
    {
        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void PatternNoParamsStrategy_Build(string serializerName, DefaultContractResolver resolver, List<JsonConverter> converters)
        {
            TestUtilities.AddEntityTypes();

            var query = TestUtilities.GetCypherQuery(resolver, converters, out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var pathBuilder = new PathBuilder(query, pathExpr);
            var path = pathBuilder.Path as Path;

            var expected = "(greysAnatomy:Series { Title: \"Grey's Anatomy\", Year: 2017 })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })" +
                "-->()";

            var actual = path.Build();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void PatternWithParamsStrategy_Build(string serializerName, DefaultContractResolver resolver, List<JsonConverter> converters)
        {
            TestUtilities.AddEntityTypes();

            var query = TestUtilities.GetCypherQuery(resolver, converters, out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var pathBuilder = new PathBuilder(query, pathExpr);
            pathBuilder.PatternBuildStrategy = PatternBuildStrategy.WithParams;

            var path = pathBuilder.Path as Path;

            var expected = "(greysAnatomy:Series { greysAnatomy })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { ellenPompeo })" +
                "-->()";

            var actual = path.Build();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void PatternWithParamsForValuesStrategy_Build(string serializerName, DefaultContractResolver resolver, List<JsonConverter> converters)
        {
            TestUtilities.AddEntityTypes();

            var query = TestUtilities.GetCypherQuery(resolver, converters, out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var pathBuilder = new PathBuilder(query, pathExpr);
            pathBuilder.PatternBuildStrategy = PatternBuildStrategy.WithParamsForValues;

            var path = pathBuilder.Path as Path;

            var expected = "(greysAnatomy:Series { Title: {greysAnatomy}.Title, Year: {greysAnatomy}.Year })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: {ellenPompeo}.Name, Born: {ellenPompeo}.Born, Roles: {ellenPompeo}.Roles })" +
                "-->()";

            var actual = path.Build();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void AssignPathParameter_Build(string serializerName, DefaultContractResolver resolver, List<JsonConverter> converters)
        {
            TestUtilities.AddEntityTypes();

            var query = TestUtilities.GetCypherQuery(resolver, converters, out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing)
                .Assign(); //assigns path parameter

            var pathBuilder = new PathBuilder(query, pathExpr);

            var expected = "P=" + 
                "(greysAnatomy:Series { Title: \"Grey's Anatomy\", Year: 2017 })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })" +
                "-->()";

            var actual = pathBuilder.Build();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void FindShortestPath_Build(string serializerName, DefaultContractResolver resolver, List<JsonConverter> converters)
        {
            TestUtilities.AddEntityTypes();

            var query = TestUtilities.GetCypherQuery(resolver, converters, out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Shortest(); //adds the shortestPath function and consequently assigns path parameter

            var pathBuilder = new PathBuilder(query, pathExpr);

            var expected = "P=shortestPath(" + 
                "(greysAnatomy:Series { Title: \"Grey's Anatomy\", Year: 2017 })" +
                "<-[acted_in:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })" +
                ")";

            var actual = pathBuilder.Build();

            Assert.Equal(expected, actual);
        }
    }
}
