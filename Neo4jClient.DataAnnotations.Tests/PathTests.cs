using System;
using System.Linq.Expressions;
using Neo4jClient.DataAnnotations.Cypher;
using Xunit;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class PathTests
    {
        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void PatternNoParamsStrategy_Build(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var pathBuilder = new PathBuilder(query, pathExpr);
            var path = pathBuilder.Path as Path;

            var expected = "(greysAnatomy:Series { Title: \"Grey's Anatomy\", Year: 2017 })" +
                           "<-[:STARRED_IN|ACTED_IN*1]-" +
                           "(ellenPompeo:Female:Actor:Person { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [" +
                           Environment.NewLine + "  \"Meredith Grey\"" + Environment.NewLine + "] })" +
                           "-->()";

            var actual = path.Build(ref query);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void PatternWithParamsStrategy_Build(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestPathMixed(P)
                .Extend(RelationshipDirection.Outgoing);

            var pathBuilder = new PathBuilder(query, pathExpr);
            pathBuilder.PatternBuildStrategy = PropertiesBuildStrategy.WithParams;

            var path = pathBuilder.Path as Path;

            var expected = "(greysAnatomy:Series $greysAnatomy)" +
                           "<-[:STARRED_IN|ACTED_IN*1]-" +
                           "(ellenPompeo:Female:Actor:Person { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
                           "-->()";

            var actual = path.Build(ref query);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void PatternWithParamsForValuesStrategy_Build(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var pathBuilder = new PathBuilder(query, pathExpr);
            pathBuilder.PatternBuildStrategy = PropertiesBuildStrategy.WithParamsForValues;

            var path = pathBuilder.Path as Path;

            var expected = "(greysAnatomy:Series { Title: $greysAnatomy.Title, Year: $greysAnatomy.Year })" +
                           "<-[:STARRED_IN|ACTED_IN*1]-" +
                           "(ellenPompeo:Female:Actor:Person { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
                           "-->()";

            var actual = path.Build(ref query);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void AssignPathParameter_Build(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing)
                .Assign(); //assigns path parameter

            var pathBuilder = new PathBuilder(query, pathExpr);

            var expected = "P=" +
                           "(greysAnatomy:Series { Title: \"Grey's Anatomy\", Year: 2017 })" +
                           "<-[:STARRED_IN|ACTED_IN*1]-" +
                           "(ellenPompeo:Female:Actor:Person { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [" +
                           Environment.NewLine + "  \"Meredith Grey\"" + Environment.NewLine + "] })" +
                           "-->()";

            var actual = pathBuilder.Build(ref query);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void FindShortestPath_Build(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestPath(P)
                .Shortest(); //adds the shortestPath function and consequently assigns path parameter

            var pathBuilder = new PathBuilder(query, pathExpr);

            var expected = "P=shortestPath(" +
                           "(greysAnatomy:Series { Title: \"Grey's Anatomy\", Year: 2017 })" +
                           "<-[:STARRED_IN|ACTED_IN*1]-" +
                           "(ellenPompeo:Female:Actor:Person { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [" +
                           Environment.NewLine + "  \"Meredith Grey\"" + Environment.NewLine + "] })" +
                           ")";

            var actual = pathBuilder.Build(ref query);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void PatternAlreadyBound_Build(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestAlreadyBoundPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var pathBuilder = new PathBuilder(query, pathExpr);
            pathBuilder.PatternBuildStrategy = PropertiesBuildStrategy.WithParams;

            var path = pathBuilder.Path as Path;

            var expected = "(greysAnatomy)" +
                           "<-[:STARRED_IN|ACTED_IN*1]-" +
                           "(ellenPompeo)" +
                           "-->()";

            var actual = path.Build(ref query);

            Assert.Equal(expected, actual);
        }
    }
}