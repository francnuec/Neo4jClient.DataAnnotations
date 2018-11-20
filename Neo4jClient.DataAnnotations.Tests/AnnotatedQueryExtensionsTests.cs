using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Tests.Models;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class AnnotatedQueryTests
    {
        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void Where(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .Where((MovieNode movie) => movie.Title == "Grey's Anatomy" && movie.Year == 2017)
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = $"WHERE ((movie.Title = ${cypherQuery.QueryParameters.First().Key}) AND (movie.Year = ${cypherQuery.QueryParameters.Last().Key}))";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void WhereMultipleParams(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .Where((MovieNode movie, ActorNode actor) => (movie.Title == "Grey's Anatomy" && movie.Year == 2017)
                || ((actor.Address as AddressThirdLevel).Location.Longitude == 0.1 
                && (actor.Address as AddressThirdLevel).ComplexProperty.Property == 5))
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = $"WHERE (((movie.Title = ${cypherQuery.QueryParameters.ElementAt(0).Key})" 
                + $" AND (movie.Year = ${cypherQuery.QueryParameters.ElementAt(1).Key}))" 
                + $" OR ((actor.NewAddressName_Location_Longitude = ${cypherQuery.QueryParameters.ElementAt(2).Key})" 
                + $" AND (actor.NewAddressName_ComplexProperty_Property = ${cypherQuery.QueryParameters.ElementAt(3).Key})))";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void AndWhere(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .Where((MovieNode movie) => movie.Title == "Grey's Anatomy")
                .AndWhere((MovieNode movie) => movie.Year == 2017)
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = $"WHERE (movie.Title = ${cypherQuery.QueryParameters.First().Key})" + 
                $"\r\nAND (movie.Year = ${cypherQuery.QueryParameters.Last().Key})";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void OrWhere(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .Where((MovieNode movie) => movie.Title == "Grey's Anatomy")
                .OrWhere((MovieNode movie) => movie.Year == 2017)
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = $"WHERE (movie.Title = ${cypherQuery.QueryParameters.First().Key})" +
                $"\r\nOR (movie.Year = ${cypherQuery.QueryParameters.Last().Key})";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void With(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .With(movie => new
                {
                    Funcs.Star,
                    title = movie.As<MovieNode>().Title,
                    movie.As<MovieNode>().Year,
                    mCollected = movie.CollectAsDistinct<MovieNode>()
                })
                .AsCypherQuery()
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

            var cypherQuery = query.AsAnnotatedQuery()
                .With(movie => movie.As<MovieNode>().Title)
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "WITH movie.Title";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void WithComplexMemberAccess(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .With(actor => actor.As<ActorNode>().Address as AddressThirdLevel)
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "WITH actor.NewAddressName_ComplexProperty_Property" 
                + ", actor.NewAddressName_SomeOtherProperty"
                + ", actor.NewAddressName_Location_Latitude"
                + ", actor.NewAddressName_Location_Longitude"
                + ", actor.NewAddressName_AddressLine, actor.NewAddressName_City"
                + ", actor.NewAddressName_State, actor.NewAddressName_Country";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void WithAddedText(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .With(movie => new
                {
                    title = movie.As<MovieNode>().Title,
                    movie.As<MovieNode>().Year
                }, addedWithText: "*")
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "WITH *, movie.Title AS title, movie.Year AS Year";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void WithMultipleParams(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .With((movie, actor, something) => new
                {
                    title = movie.As<MovieNode>().Title,
                    movie.As<MovieNode>().Year,
                    lg = (actor.As<ActorNode>().Address as AddressThirdLevel).Location.Longitude,
                    (actor.As<ActorNode>().Address as AddressThirdLevel).ComplexProperty.Property,
                    something_count = something.Count()
                })
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "WITH movie.Title AS title, movie.Year AS Year, actor.NewAddressName_Location_Longitude AS lg"
                + ", actor.NewAddressName_ComplexProperty_Property AS Property, count(something) AS something_count";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void WithComplexMultipleParams(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .With((movie, actor, something) => new
                {
                    title = movie.As<MovieNode>().Title,
                    movie.As<MovieNode>().Year,
                    lg = actor.As<ActorNode>().Address as AddressThirdLevel,
                    (actor.As<ActorNode>().Address as AddressThirdLevel).ComplexProperty.Property,
                    something_count = something.Count()
                })
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "WITH movie.Title AS title, movie.Year AS Year" 
                + ", { ComplexProperty_Property: actor.NewAddressName_ComplexProperty_Property" 
                + ", SomeOtherProperty: actor.NewAddressName_SomeOtherProperty" 
                + ", Location_Latitude: actor.NewAddressName_Location_Latitude" 
                + ", Location_Longitude: actor.NewAddressName_Location_Longitude" 
                + ", AddressLine: actor.NewAddressName_AddressLine, City: actor.NewAddressName_City" 
                + ", State: actor.NewAddressName_State, Country: actor.NewAddressName_Country } AS lg"
                + ", actor.NewAddressName_ComplexProperty_Property AS Property, count(something) AS something_count";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void Return(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .Return(movie => new
                {
                    title = movie.As<MovieNode>().Title,
                    movie.As<MovieNode>().Year
                })
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "RETURN movie.Title AS title, movie.Year AS Year";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void ReturnComplexMemberAccess(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .Return(actor => actor.As<ActorNode>().Address as AddressThirdLevel)
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            //doesn't make cypher sense but for tests
            var expected = "RETURN { ComplexProperty_Property: actor.NewAddressName_ComplexProperty_Property"
                + ", SomeOtherProperty: actor.NewAddressName_SomeOtherProperty"
                + ", Location_Latitude: actor.NewAddressName_Location_Latitude"
                + ", Location_Longitude: actor.NewAddressName_Location_Longitude"
                + ", AddressLine: actor.NewAddressName_AddressLine, City: actor.NewAddressName_City"
                + ", State: actor.NewAddressName_State, Country: actor.NewAddressName_Country }";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void ReturnMultipleParams(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .Return((movie, actor, something) => new
                {
                    title = movie.As<MovieNode>().Title,
                    movie.As<MovieNode>().Year,
                    lg = (actor.As<ActorNode>().Address as AddressThirdLevel).Location.Longitude,
                    (actor.As<ActorNode>().Address as AddressThirdLevel).ComplexProperty.Property,
                    something_count = something.Count()
                })
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "RETURN movie.Title AS title, movie.Year AS Year, actor.NewAddressName_Location_Longitude AS lg"
                + ", actor.NewAddressName_ComplexProperty_Property AS Property, count(something) AS something_count";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void ReturnDistinct(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .ReturnDistinct(movie => new
                {
                    title = movie.As<MovieNode>().Title,
                    movie.As<MovieNode>().Year
                })
                .AsCypherQuery()
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

            var cypherQuery = query.AsAnnotatedQuery()
                .ReturnDistinct((movie, actor, something) => new
                {
                    title = movie.As<MovieNode>().Title,
                    movie.As<MovieNode>().Year,
                    lg = (actor.As<ActorNode>().Address as AddressThirdLevel).Location.Longitude,
                    (actor.As<ActorNode>().Address as AddressThirdLevel).ComplexProperty.Property,
                    something_count = something.Count()
                })
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "RETURN DISTINCT movie.Title AS title, movie.Year AS Year, actor.NewAddressName_Location_Longitude AS lg"
                + ", actor.NewAddressName_ComplexProperty_Property AS Property, count(something) AS something_count";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void OrderBy_ThenByDesc(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .OrderBy(movie => movie.As<MovieNode>().Year)
                .ThenByDescending(movie => movie.Id())
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "ORDER BY movie.Year, id(movie) DESC";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void OrderByDesc_ThenBy(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .OrderByDescending(movie => movie.As<MovieNode>().Year)
                .ThenBy(movie => movie.Id())
                .AsCypherQuery()
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "ORDER BY movie.Year DESC, id(movie)";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void OrderBy_CypherThenByDesc(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .OrderBy(movie => movie.Id())
                .AsCypherQuery()
                .ThenByDescending((MovieNode movie) => movie.Year)
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "ORDER BY id(movie), movie.Year DESC";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void OrderByDesc_CypherThenBy(string testContextName, TestContext testContext)
        {
            var query = testContext.Query;

            var cypherQuery = query.AsAnnotatedQuery()
                .OrderByDescending(movie => movie.Id())
                .AsCypherQuery()
                .ThenBy((MovieNode movie) => movie.Year)
                .Query;

            var actual = cypherQuery.QueryText;

            var expected = "ORDER BY id(movie) DESC, movie.Year";

            Assert.Equal(expected, actual);
        }
    }
}
