using System;
using System.Linq.Expressions;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Cypher.Functions;
using Neo4jClient.DataAnnotations.Tests.Models;
using Xunit;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class CypherFluentQueryExtensionsTests
    {
        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void WithExpression(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            query = query.WithExpression((ActorNode actor) => actor.Address.City, out var actual);

            var expected = "actor.NewAddressName_City";

            Assert.Equal(expected, actual[0]);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void WithExpression_NotMemberAccess(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            query = query.WithExpression((ActorNode actor) => actor.Address.City, out var actual, false);

            var expected = "NewAddressName_City";

            Assert.Equal(expected, actual[0]);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void WithExpression_Multiple(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            query = query.WithExpression((ActorNode actor) => new { actor.Name, actor.Born, actor.Address.City },
                out var actual);

            var expected = new[] { "actor.Name", "actor.Born", "actor.NewAddressName_City" };

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void PatternNoParamsStrategy_WithPattern(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            query = query.UsingBuildStrategy(PropertiesBuildStrategy.NoParams)
                .WithPattern(out var actual, pathExpr, p => p.Pattern("a", "b", RelationshipDirection.Automatic));

            var expected = "(greysAnatomy:Series { Title: \"Grey's Anatomy\", Year: 2017 })" +
                           "<-[:STARRED_IN|ACTED_IN*1]-" +
                           "(ellenPompeo:Female:Actor:Person { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [" +
                           Environment.NewLine + "  \"Meredith Grey\"" + Environment.NewLine + "] })" +
                           "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void PatternWithParamsStrategy_WithPattern(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestPathMixed(P)
                .Extend(RelationshipDirection.Outgoing);

            query = query.UsingBuildStrategy(PropertiesBuildStrategy.WithParams)
                .WithPattern(out var actual, pathExpr, p => p.Pattern("a", "b", RelationshipDirection.Automatic));

            var expected = "(greysAnatomy:Series $greysAnatomy)" +
                           "<-[:STARRED_IN|ACTED_IN*1]-" +
                           "(ellenPompeo:Female:Actor:Person { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
                           "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void PatternWithParamsForValuesStrategy_WithPattern(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            query = query.WithPattern(out var actual, pathExpr,
                p => p.Pattern("a", "b", RelationshipDirection.Automatic));

            var expected = "(greysAnatomy:Series { Title: $greysAnatomy.Title, Year: $greysAnatomy.Year })" +
                           "<-[:STARRED_IN|ACTED_IN*1]-" +
                           "(ellenPompeo:Female:Actor:Person { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
                           "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void Match(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.Match(pathExpr, p => p.Pattern("a", "b", RelationshipDirection.Automatic)).Query
                .QueryText;

            var expected = "MATCH (greysAnatomy:Series { Title: $greysAnatomy.Title, Year: $greysAnatomy.Year })" +
                           "<-[:STARRED_IN|ACTED_IN*1]-" +
                           "(ellenPompeo:Female:Actor:Person { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
                           "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void OptionalMatch(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.OptionalMatch(pathExpr, p => p.Pattern("a", "b", RelationshipDirection.Automatic)).Query
                .QueryText;

            var expected =
                "OPTIONAL MATCH (greysAnatomy:Series { Title: $greysAnatomy.Title, Year: $greysAnatomy.Year })" +
                "<-[:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor:Person { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void Merge(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.Merge(pathExpr, p => p.Pattern("a", "b", RelationshipDirection.Automatic)).Query
                .QueryText;

            var expected = "MERGE (greysAnatomy:Series { Title: $greysAnatomy.Title, Year: $greysAnatomy.Year })" +
                           "<-[:STARRED_IN|ACTED_IN*1]-" +
                           "(ellenPompeo:Female:Actor:Person { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
                           "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void Create(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestPathMixed(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.Create(pathExpr, p => p.Pattern("a", "b", RelationshipDirection.Automatic)).Query
                .QueryText;

            //This scenario is probably unlikely in a real neo4j situation, but for tests sakes.
            var expected = "CREATE (greysAnatomy:Series $greysAnatomy)" +
                           "<-[:STARRED_IN|ACTED_IN*1]-" +
                           "(ellenPompeo:Female:Actor:Person { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
                           "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void CreateUnique(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = P => TestUtilities.BuildTestPathMixed(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.CreateUnique(pathExpr, p => p.Pattern("a", "b", RelationshipDirection.Automatic)).Query
                .QueryText;

            //This scenario is probably unlikely in a real neo4j situation, but for tests sakes.
            var expected = "CREATE UNIQUE (greysAnatomy:Series $greysAnatomy)" +
                           "<-[:STARRED_IN|ACTED_IN*1]-" +
                           "(ellenPompeo:Female:Actor:Person { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
                           "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void SetPredicate(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query.Set((ActorNode actor) =>
                actor.Name == "Ellen Pompeo"
                && actor.Born == CypherVariables.Get<ActorNode>("shondaRhimes").Born
                && actor.Roles == new[] { "Meredith Grey" }, out var setParam).Query.QueryText;

            var expected =
                $"SET actor.Name = ${setParam}.Name, actor.Born = shondaRhimes.Born, actor.Roles = ${setParam}.Roles";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void SetPredicateWithParams(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query.UsingBuildStrategy(PropertiesBuildStrategy.WithParams)
                .Set((ActorNode actor) =>
                    actor.Name == "Ellen Pompeo"
                    && actor.Born == 1969
                    && actor.Roles == new[] { "Meredith Grey" }, out var setParam).Query.QueryText;

            //When using Set predicate, WithParams strategy is the same as WithParamsForValues
            var expected =
                $"SET actor.Name = ${setParam}.Name, actor.Born = ${setParam}.Born, actor.Roles = ${setParam}.Roles";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void SetPredicateWithParamsWithComplexParameterAssigment(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query.UsingBuildStrategy(PropertiesBuildStrategy.WithParams)
                .Set((ActorNode actor) =>
                    actor.Name == "Ellen Pompeo"
                    && actor.Born == actor.Born + 1 //((int?)null ?? 1969) + 1
                    && actor.Roles == new[] { "Meredith Grey" }, out var setParam).Query.QueryText;

            //When using Set predicate, WithParams strategy is the same as WithParamsForValues
            var expected =
                $"SET actor.Name = ${setParam}.Name, actor.Born = actor.Born + $p1, actor.Roles = ${setParam}.Roles";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void SetPredicateWithParamsWithComplexVarsGetAssigment(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query.UsingBuildStrategy(PropertiesBuildStrategy.WithParams)
                .Set((ActorNode actor) =>
                    actor.Name == "Ellen Pompeo"
                    && actor.Born == CypherVariables.Get<ActorNode>("actor").Born + 1 //((int?)null ?? 1969) + 1
                    && actor.Roles == new[] { "Meredith Grey" }, out var setParam).Query.QueryText;

            //When using Set predicate, WithParams strategy is the same as WithParamsForValues
            var expected =
                $"SET actor.Name = ${setParam}.Name, actor.Born = actor.Born + $p1, actor.Roles = ${setParam}.Roles";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void SetProperties(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query.Set("movie", () => new MovieNode
            {
                Title = "Grey's Anatomy",
                Year = 2017
            }, out var setParam).Query.QueryText;

            var expected = $"SET movie = ${setParam}";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void SetPropertiesWithParamsForValues(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query.UsingBuildStrategy(PropertiesBuildStrategy.WithParamsForValues)
                .Set("movie", () => new MovieNode
                {
                    Title = "Grey's Anatomy",
                    Year = 2017
                }, out var setParam).Query.QueryText;

            var expected = $"SET movie = {{ Title: ${setParam}.Title, Year: ${setParam}.Year }}";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void SetAdd(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query.SetAdd((MovieNode movie) => movie.Title == "Grey's Anatomy"
                                                           && movie.Year == 2017, out var setParam).Query.QueryText;

            var expected = $"SET movie += ${setParam}";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void SetAddNoParams(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query.UsingBuildStrategy(PropertiesBuildStrategy.NoParams)
                .SetAdd((MovieNode m) => m.Title == "Grey's Anatomy"
                                         && m.Year == 2017, "movie").Query.QueryText;

            var expected = "SET movie += { Title: \"Grey's Anatomy\", Year: 2017 }";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void Index(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query.CreateIndex((ActorNode actor) => actor.Address.AddressLine).Query.QueryText;

            var expected = "CREATE INDEX ON :Actor(NewAddressName_AddressLine)";

            Assert.Equal(expected, actual);

            //test DROP too
            actual = query.DropIndex((ActorNode actor) => actor.Address.AddressLine).Query.QueryText;
            expected = "DROP INDEX ON :Actor(NewAddressName_AddressLine)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void CompositeIndex(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query
                .CreateIndex((ActorNode actor) => new { (actor.Address as AddressWithComplexType).Location, actor.Name })
                .Query.QueryText;

            var expected =
                "CREATE INDEX ON :Actor(NewAddressName_Location_Latitude, NewAddressName_Location_Longitude, Name)";

            Assert.Equal(expected, actual);

            //test DROP too
            actual = query
                .DropIndex((ActorNode actor) => new { (actor.Address as AddressWithComplexType).Location, actor.Name })
                .Query.QueryText;
            expected =
                "DROP INDEX ON :Actor(NewAddressName_Location_Latitude, NewAddressName_Location_Longitude, Name)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void UniqueConstraint(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query.CreateUniqueConstraint((ActorNode actor) => actor.Address.AddressLine).Query.QueryText;

            var expected = "CREATE CONSTRAINT ON (actor:Actor) ASSERT actor.NewAddressName_AddressLine IS UNIQUE";

            Assert.Equal(expected, actual);

            //test DROP too
            actual = query.DropUniqueConstraint((ActorNode actor) => actor.Address.AddressLine).Query.QueryText;
            expected = "DROP CONSTRAINT ON (actor:Actor) ASSERT actor.NewAddressName_AddressLine IS UNIQUE";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void ExistsConstraint(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query.CreateExistsConstraint((ActorNode actor) => actor.Address.AddressLine).Query.QueryText;

            var expected = "CREATE CONSTRAINT ON (actor:Actor) ASSERT exists(actor.NewAddressName_AddressLine)";

            Assert.Equal(expected, actual);

            //test DROP too
            actual = query.DropExistsConstraint((ActorNode actor) => actor.Address.AddressLine).Query.QueryText;
            expected = "DROP CONSTRAINT ON (actor:Actor) ASSERT exists(actor.NewAddressName_AddressLine)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void RelationshipExistsConstraint(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query.CreateExistsConstraint((MovieActorRelationship rel) => rel.Roles, true).Query.QueryText;

            var expected = "CREATE CONSTRAINT ON ()-[rel:ACTED_IN]-() ASSERT exists(rel.Roles)";

            Assert.Equal(expected, actual);

            //test DROP too
            actual = query.DropExistsConstraint((MovieActorRelationship rel) => rel.Roles, true).Query.QueryText;
            expected = "DROP CONSTRAINT ON ()-[rel:ACTED_IN]-() ASSERT exists(rel.Roles)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void KeyConstraint(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var actual = query.CreateKeyConstraint((ActorNode actor) =>
                new { (actor.Address as AddressWithComplexType).Location, actor.Name }).Query.QueryText;

            var expected =
                "CREATE CONSTRAINT ON (actor:Actor) ASSERT (actor.NewAddressName_Location_Latitude, actor.NewAddressName_Location_Longitude, actor.Name) IS NODE KEY";

            Assert.Equal(expected, actual);

            //test DROP too
            actual = query.DropKeyConstraint((ActorNode actor) =>
                new { (actor.Address as AddressWithComplexType).Location, actor.Name }).Query.QueryText;
            expected =
                "DROP CONSTRAINT ON (actor:Actor) ASSERT (actor.NewAddressName_Location_Latitude, actor.NewAddressName_Location_Longitude, actor.Name) IS NODE KEY";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void OrderBy_ThenByDesc(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .OrderBy<MovieNode>(movie => movie.Year)
                .ThenByDescending<MovieNode>(movie => movie.Title)
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "ORDER BY movie.Year, movie.Title DESC";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void OrderByDesc_ThenBy(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .OrderByDescending<MovieNode>(movie => movie.Year)
                .ThenBy<MovieNode>(movie => movie.Title)
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "ORDER BY movie.Year DESC, movie.Title";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void OrderBy_AnnotatedThenByDesc(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .OrderBy((MovieNode movie) => movie.Year)
                .AsOrderedAnnotatedQuery()
                .ThenByDescending(movie => movie.Id())
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "ORDER BY movie.Year, id(movie) DESC";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void OrderByDesc_AnnotatedThenBy(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .OrderByDescending((MovieNode movie) => movie.Year)
                .AsOrderedAnnotatedQuery()
                .ThenBy(movie => movie.Id())
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "ORDER BY movie.Year DESC, id(movie)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void RemoveProperty(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .RemoveProperty((MovieNode movie) => movie.Year)
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "REMOVE movie.Year";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void RemoveProperties(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .RemoveProperty((MovieNode movie) => new { movie.Year, movie.Title }, "m")
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "REMOVE m.Year, m.Title";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void RemoveLabel(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .RemoveLabel<MovieNode>("m")
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "REMOVE m:Movie";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void RemoveMultipleLabelTypes(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .RemoveMultipleLabels("a", typeof(ActorNode), typeof(MovieExtraNode))
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "REMOVE a:Actor:Extra";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void RemoveMultipleLabels(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .RemoveMultipleLabels("a", "Actor", "Extra")
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "REMOVE a:Actor:Extra";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void RemoveAllLabels(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .RemoveAllLabels<ActorNode>("actor")
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "REMOVE actor:Actor:Person";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void With(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .With((MovieNode movie) => new
                {
                    CypherFunctions.Star,
                    title = movie.Title,
                    movie.Year,
                    mCollected = movie.Distinct().Collect()
                })
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "WITH *, movie.Title AS title, movie.Year AS Year, collect(DISTINCT movie) AS mCollected";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void WithMemberAccess(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .With<MovieNode>(movie => movie.Title)
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "WITH movie.Title";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void WithMultipleParams(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .With((MovieNode movie, ActorNode actor, object something) => new
                {
                    title = movie.Title,
                    movie.Year,
                    lg = (actor.Address as AddressThirdLevel).Location.Longitude,
                    (actor.Address as AddressThirdLevel).ComplexProperty.Property,
                    something_count = something.Count()
                })
                .Query;

            var actual = cypherQuery.QueryText;

            var expected =
                "WITH movie.Title AS title, movie.Year AS Year, actor.NewAddressName_Location_Longitude AS lg"
                + ", actor.NewAddressName_ComplexProperty_Property AS Property, count(something) AS something_count";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void Return(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .Return((MovieNode movie) => new
                {
                    title = movie.Title,
                    movie.Year
                })
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "RETURN movie.Title AS title, movie.Year AS Year";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void ReturnMultipleParams(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .Return((MovieNode movie, ActorNode actor, object something) => new
                {
                    title = movie.Title,
                    movie.Year,
                    lg = (actor.Address as AddressThirdLevel).Location.Longitude,
                    (actor.Address as AddressThirdLevel).ComplexProperty.Property,
                    something_count = something.Count()
                })
                .Query;

            var actual = cypherQuery.QueryText;

            var expected =
                "RETURN movie.Title AS title, movie.Year AS Year, actor.NewAddressName_Location_Longitude AS lg"
                + ", actor.NewAddressName_ComplexProperty_Property AS Property, count(something) AS something_count";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void ReturnDistinct(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .ReturnDistinct((MovieNode movie) => new
                {
                    title = movie.Title,
                    movie.Year
                })
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "RETURN DISTINCT movie.Title AS title, movie.Year AS Year";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void ReturnDistinctMultipleParams(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query
                .ReturnDistinct((MovieNode movie, ActorNode actor, object something) => new
                {
                    title = movie.Title,
                    movie.Year,
                    lg = (actor.Address as AddressThirdLevel).Location.Longitude,
                    Property = (actor.Address as AddressThirdLevel).ComplexProperty.Property._(),
                    something_count = something.Count(),
                    constant = "constant"
                })
                .Query;

            var actual = cypherQuery.QueryText;

            var expected =
                "RETURN DISTINCT movie.Title AS title, movie.Year AS Year, actor.NewAddressName_Location_Longitude AS lg"
                + ", actor.NewAddressName_ComplexProperty_Property AS Property, count(something) AS something_count"
                + ", $p0 AS constant";

            Assert.Equal(expected, actual);
        }
    }
}