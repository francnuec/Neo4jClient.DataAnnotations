using Neo4jClient.Cypher;
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
        //[Theory]
        //[MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        //public void PatternNoParamsStrategy_GetPattern(string serializerName, EntityResolver resolver, EntityConverter converter)
        //{
        //    TestUtilities.RegisterEntityTypes(resolver, converter);

        //    var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

        //    Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
        //        .Extend(RelationshipDirection.Outgoing);

        //    query.WithPattern(out var actual, pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic));

        //    var expected = "(greysAnatomy:Series { Title: \"Grey's Anatomy\", Year: 2017 })" +
        //        "<-[:STARRED_IN|ACTED_IN*1]-" +
        //        "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })" +
        //        "-->(), (a)--(b)";

        //    Assert.Equal(expected, actual);
        //}

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void WithProperty(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            query = query.WithProperty((ActorNode actor) => actor.Address.City, out var actual);

            var expected = "NewAddressName_City";

            Assert.Equal(expected, actual[0]);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void WithProperty_MemberAccess(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            query = query.WithProperty((ActorNode actor) => actor.Address.City, out var actual, makeMemberAccess: true);

            var expected = "actor.NewAddressName_City";

            Assert.Equal(expected, actual[0]);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void WithProperty_Multiple(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            query = query.WithProperty((ActorNode actor) => new { actor.Name, actor.Born, actor.Address.City }, out var actual);

            var expected = new string[] { "Name", "Born", "NewAddressName_City" };

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void PatternNoParamsStrategy_WithPattern(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            query = query.WithPattern(out var actual, PropertiesBuildStrategy.NoParams, pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic));

            var expected = "(greysAnatomy:Series { Title: \"Grey's Anatomy\", Year: 2017 })" +
                "<-[:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: \"Ellen Pompeo\", Born: shondaRhimes.Born, Roles: [\r\n  \"Meredith Grey\"\r\n] })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void PatternWithParamsStrategy_WithPattern(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPathMixed(P)
                .Extend(RelationshipDirection.Outgoing);

            query = query.WithPattern(out var actual, PropertiesBuildStrategy.WithParams, pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic));

            var expected = "(greysAnatomy:Series $greysAnatomy)" +
                "<-[:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void PatternWithParamsForValuesStrategy_WithPattern(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPath(P)
                .Extend(RelationshipDirection.Outgoing);

            query = query.WithPattern(out var actual, pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic));

            var expected = "(greysAnatomy:Series { Title: $greysAnatomy.Title, Year: $greysAnatomy.Year })" +
                "<-[:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
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
                "<-[:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
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
                "<-[:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
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
                "<-[:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void Create(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPathMixed(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.Create(pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic)).Query.QueryText;

            //This scenario is probably unlikely in a real neo4j situation, but for tests sakes.
            var expected = "CREATE (greysAnatomy:Series $greysAnatomy)" +
                "<-[:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
                "-->(), (a)--(b)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void CreateUnique(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            Expression<Func<IPathBuilder, IPathExtent>> pathExpr = (P) => TestUtilities.BuildTestPathMixed(P)
                .Extend(RelationshipDirection.Outgoing);

            var actual = query.CreateUnique(pathExpr, (p) => p.Pattern("a", "b", RelationshipDirection.Automatic)).Query.QueryText;

            //This scenario is probably unlikely in a real neo4j situation, but for tests sakes.
            var expected = "CREATE UNIQUE (greysAnatomy:Series $greysAnatomy)" +
                "<-[:STARRED_IN|ACTED_IN*1]-" +
                "(ellenPompeo:Female:Actor { Name: $ellenPompeo.Name, Born: shondaRhimes.Born, Roles: $ellenPompeo.Roles })" +
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
                && actor.Roles == new string[] { "Meredith Grey" }, out var setParam).Query.QueryText;

            var expected = $"SET actor.Name = ${setParam}.Name, actor.Born = shondaRhimes.Born, actor.Roles = ${setParam}.Roles";

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

            //When using Set predicate, WithParams strategy is the same as WithParamsForValues
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
        public void SetAdd(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.SetAdd((MovieNode movie) => movie.Title == "Grey's Anatomy" 
            && movie.Year == 2017, setParameter: out var setParam).Query.QueryText;

            var expected = $"SET movie += ${setParam}";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void SetAddNoParams(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.SetAdd((MovieNode m) => m.Title == "Grey's Anatomy"
            && m.Year == 2017, PropertiesBuildStrategy.NoParams, "movie").Query.QueryText;

            var expected = $"SET movie += {{ Title: \"Grey's Anatomy\", Year: 2017 }}";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void Index(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.CreateIndex((ActorNode actor) => actor.Address.AddressLine).Query.QueryText;

            var expected = "CREATE INDEX ON :Actor(NewAddressName_AddressLine)";

            Assert.Equal(expected, actual);

            //test DROP too
            actual = query.DropIndex((ActorNode actor) => actor.Address.AddressLine).Query.QueryText;
            expected = "DROP INDEX ON :Actor(NewAddressName_AddressLine)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void CompositeIndex(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.CreateIndex((ActorNode actor) => new { (actor.Address as AddressWithComplexType).Location, actor.Name }).Query.QueryText;

            var expected = "CREATE INDEX ON :Actor(NewAddressName_Location_Latitude, NewAddressName_Location_Longitude, Name)";

            Assert.Equal(expected, actual);

            //test DROP too
            actual = query.DropIndex((ActorNode actor) => new { (actor.Address as AddressWithComplexType).Location, actor.Name }).Query.QueryText;
            expected = "DROP INDEX ON :Actor(NewAddressName_Location_Latitude, NewAddressName_Location_Longitude, Name)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void UniqueConstraint(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.CreateUniqueConstraint((ActorNode actor) => actor.Address.AddressLine).Query.QueryText;

            var expected = "CREATE CONSTRAINT ON (actor:Actor) ASSERT actor.NewAddressName_AddressLine IS UNIQUE";

            Assert.Equal(expected, actual);

            //test DROP too
            actual = query.DropUniqueConstraint((ActorNode actor) => actor.Address.AddressLine).Query.QueryText;
            expected = "DROP CONSTRAINT ON (actor:Actor) ASSERT actor.NewAddressName_AddressLine IS UNIQUE";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void ExistsConstraint(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.CreateExistsConstraint((ActorNode actor) => actor.Address.AddressLine).Query.QueryText;

            var expected = "CREATE CONSTRAINT ON (actor:Actor) ASSERT exists(actor.NewAddressName_AddressLine)";

            Assert.Equal(expected, actual);

            //test DROP too
            actual = query.DropExistsConstraint((ActorNode actor) => actor.Address.AddressLine).Query.QueryText;
            expected = "DROP CONSTRAINT ON (actor:Actor) ASSERT exists(actor.NewAddressName_AddressLine)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void RelationshipExistsConstraint(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.CreateExistsConstraint((MovieActorRelationship rel) => rel.Roles, isRelationship: true).Query.QueryText;

            var expected = "CREATE CONSTRAINT ON ()-[rel:ACTED_IN]-() ASSERT exists(rel.Roles)";

            Assert.Equal(expected, actual);

            //test DROP too
            actual = query.DropExistsConstraint((MovieActorRelationship rel) => rel.Roles, isRelationship: true).Query.QueryText;
            expected = "DROP CONSTRAINT ON ()-[rel:ACTED_IN]-() ASSERT exists(rel.Roles)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void KeyConstraint(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var actual = query.CreateKeyConstraint((ActorNode actor) => new { (actor.Address as AddressWithComplexType).Location, actor.Name }).Query.QueryText;

            var expected = "CREATE CONSTRAINT ON (actor:Actor) ASSERT (actor.NewAddressName_Location_Latitude, actor.NewAddressName_Location_Longitude, actor.Name) IS NODE KEY";

            Assert.Equal(expected, actual);

            //test DROP too
            actual = query.DropKeyConstraint((ActorNode actor) => new { (actor.Address as AddressWithComplexType).Location, actor.Name }).Query.QueryText;
            expected = "DROP CONSTRAINT ON (actor:Actor) ASSERT (actor.NewAddressName_Location_Latitude, actor.NewAddressName_Location_Longitude, actor.Name) IS NODE KEY";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void OrderBy_ThenByDesc(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var cypherQuery = query
                .OrderBy<MovieNode>(movie => movie.Year)
                .ThenByDescending<MovieNode>(movie => movie.Title)
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "ORDER BY movie.Year, movie.Title DESC";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void OrderByDesc_ThenBy(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

            var cypherQuery = query
                .OrderByDescending<MovieNode>(movie => movie.Year)
                .ThenBy<MovieNode>(movie => movie.Title)
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "ORDER BY movie.Year DESC, movie.Title";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void OrderBy_AnnotatedThenByDesc(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

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
        [MemberData("SerializerData", MemberType = typeof(TestUtilities))]
        public void OrderByDesc_AnnotatedThenBy(string serializerName, EntityResolver resolver, EntityConverter converter)
        {
            TestUtilities.RegisterEntityTypes(resolver, converter);

            var query = TestUtilities.GetCypherQuery(out var client, out var serializer);

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
    }
}
