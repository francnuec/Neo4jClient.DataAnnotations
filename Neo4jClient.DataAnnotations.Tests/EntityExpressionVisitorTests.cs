using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Neo4jClient.DataAnnotations.Expressions;
using Neo4jClient.DataAnnotations.Tests.Models;
using Neo4jClient.DataAnnotations.Utils;
using Xunit;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class EntityExpressionVisitorTests
    {
        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void Constant(string testContextName, TestContext testContext)
        {
            Expression<Func<object>> expression = () => TestUtilities.Actor;

            var entityVisitor = new EntityExpressionVisitor(testContext.QueryContext);
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.Equal(newExpression, expression.Body);

            Assert.Empty(entityVisitor.PendingAssignments);

            dynamic result = newExpression.ExecuteExpression<ActorNode>();
            Assert.NotNull(result);

            Assert.Equal(result, TestUtilities.Actor);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void Dictionary(string testContextName, TestContext testContext)
        {
            Expression<Func<object>> expression = () => new Dictionary<string, object>
            {
                {"Name", TestUtilities.Actor.Name},
                {"Born", TestUtilities.Actor.Born},
                {"Address", TestUtilities.Actor.Address}
            };

            var entityVisitor = new EntityExpressionVisitor(testContext.QueryContext);
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Empty(entityVisitor.PendingAssignments);

            dynamic result = newExpression.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            var tokensExpected = new Dictionary<string, object>
            {
                {"Name", "Ellen Pompeo"},
                {"Born", 1969},
                {"NewAddressName_AddressLine", null},
                {"NewAddressName_City", "Los Angeles"},
                {"NewAddressName_State", "California"},
                {"NewAddressName_Country", "US"},
                {"NewAddressName_Location_Latitude", 34.0522},
                {"NewAddressName_Location_Longitude", -118.2437}
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result) Assert.Equal(tokensExpected[pair.Key], pair.Value);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void AnonymousType(string testContextName, TestContext testContext)
        {
            Expression<Func<object>> expression = () =>
                new
                {
                    Name = "Ellen Pompeo",
                    CypherVariables.Get<ActorNode>("shondaRhimes").Born,
                    Roles = new[] { "Meredith Grey" },
                    Age = 47.ToString()
                };

            var entityVisitor = new EntityExpressionVisitor(testContext.QueryContext);
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Single(entityVisitor
                .PendingAssignments); //because we are accounting for an implicit convert to object type.

            Assert.Equal("Born", entityVisitor.PendingAssignments.First().Key.ComplexJsonName);
            Assert.Equal("Get(\"shondaRhimes\").Born", entityVisitor.PendingAssignments.First().Value.ToString());

            var result = newExpression.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            var tokensExpected = new Dictionary<string, object>
            {
                {"Name", "Ellen Pompeo"},
                {"Born", 0}, //to be later replaced at serialization with params
                {"Roles", new[] {"Meredith Grey"}},
                {"Age", "47"}
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result) Assert.Equal(tokensExpected[pair.Key], pair.Value);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void ComplexAnonymousType(string testContextName, TestContext testContext)
        {
            //the following is purely a test, and not necessarily a good example for neo4j cypher.
            Expression<Func<object>> expression = () => new
            {
                new AddressWithComplexType
                {
                    AddressLine = CypherVariables.Get("A")["AddressLine"] as string,
                    Location = new Location
                    {
                        Latitude = (double)CypherVariables.Get("A")["Location_Latitude"],
                        Longitude = 56.90
                    }
                }.Location
            };

            var entityVisitor = new EntityExpressionVisitor(testContext.QueryContext);
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(2, entityVisitor.PendingAssignments.Count);

            Assert.Equal("Location_Latitude", entityVisitor.PendingAssignments.First().Key.ComplexJsonName);
            Assert.Contains("Get(\"A\").get_Item(\"Location_Latitude\")",
                entityVisitor.PendingAssignments.First().Value.ToString());

            Assert.Equal("Location_Longitude", entityVisitor.PendingAssignments.Last().Key.ComplexJsonName);
            Assert.Contains("Get(\"A\").get_Item(\"AddressLine\")",
                entityVisitor.PendingAssignments.Last().Value.ToString());

            var result = newExpression.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            var tokensExpected = new Dictionary<string, object>
            {
                {"Location_Latitude", 0.0}, //because the assignment is pending due to the CypherVariables Get method.
                {"Location_Longitude", 0.0} //because the assignment is pending due to the CypherVariables Get method.
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result) Assert.Equal(tokensExpected[pair.Key], pair.Value);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void EscapedComplexAnonymousType(string testContextName, TestContext testContext)
        {
            //the following is purely a test, and not necessarily a good example for neo4j cypher.
            Expression<Func<object>> expression = () => new
            {
                Address = CypherFunctions._(TestUtilities.Actor.Address as AddressWithComplexType),
                Coordinates = new[]
                {
                    (TestUtilities.Actor.Address as AddressWithComplexType).Location.Latitude,
                    (double) CypherVariables.Get("shondaRhimes")["NewAddressName_Location_Longitude"]
                }
            };

            var entityVisitor = new EntityExpressionVisitor(testContext.QueryContext);
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Single(entityVisitor.PendingAssignments);

            Assert.Equal("Coordinates", entityVisitor.PendingAssignments.First().Key.ComplexJsonName);
            Assert.Contains("Get(\"shondaRhimes\").get_Item(\"NewAddressName_Location_Longitude\")",
                entityVisitor.PendingAssignments.First().Value.ToString());

            var result = newExpression.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            var tokensExpected = new Dictionary<string, object>
            {
                //same name (Address) returned because it was escaped (instead of: NewAddressName)
                {"Address_AddressLine", null},
                {"Address_City", "Los Angeles"},
                {"Address_State", "California"},
                {"Address_Country", "US"},
                {"Address_Location_Latitude", 34.0522},
                {"Address_Location_Longitude", -118.2437},
                {"Coordinates", null} //assignment pending due to CypherVariables Get call
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result) Assert.Equal(tokensExpected[pair.Key], pair.Value);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void Predicate(string testContextName, TestContext testContext)
        {
            Expression<Func<ActorNode, bool>> expression = a =>
                a.Born == CypherVariables.Get<ActorNode>("ellenPompeo").Born
                && a.Name == "Shonda Rhimes"
                && a.Address.AddressLine == CypherVariables.Get<ActorNode>("shondaRhimes").Address.AddressLine;

            var entityVisitor =
                new EntityExpressionVisitor(testContext.QueryContext, expression.Parameters.FirstOrDefault());
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(3, entityVisitor.PredicateAssignments.Count);

            Assert.Equal(2, entityVisitor.PendingAssignments.Count);

            Assert.Equal("Born", entityVisitor.PendingAssignments.First().Key.ComplexJsonName);
            Assert.Equal("Get(\"ellenPompeo\").Born._()",
                entityVisitor.PendingAssignments.First().Value
                    .ToString()); //the npf escape will be added for concrete classes

            var result = newExpression.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            var tokensExpected = new Dictionary<string, object>
            {
                {"Name", "Shonda Rhimes"},
                {"Born", 0}, //assignment pending due to CypherVariables Get call
                {"NewAddressName_AddressLine", null}
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result) Assert.Equal(tokensExpected[pair.Key], pair.Value);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void PredicateComplexTypeSet(string testContextName, TestContext testContext)
        {
            Expression<Func<ActorNode, bool>> expression = a => a.Address == new AddressWithComplexType
            {
                //Use this style only if you're sure all the properties here are assigned, 
                //because this address object would replace the instance address property entirely.
                //Also note that there's a good chance the parameters set inline here wouldn't make it to the generated pattern.
                //This was done mainly for testing. 
                //Use a => a.Address.Location.Longitude == (double)CypherVariables.Get("ellenPompeo")["NewAddressName_Location_Longitude"] instead.

                AddressLine = CypherVariables.Get<ActorNode>("shondaRhimes").Address.AddressLine,
                Location = new Location
                {
                    Latitude = 4.0,
                    Longitude = (double)CypherVariables.Get("ellenPompeo")["NewAddressName_Location_Longitude"]
                }
            } && a.Name == "Shonda Rhimes";

            var entityVisitor =
                new EntityExpressionVisitor(testContext.QueryContext, expression.Parameters.FirstOrDefault());
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(2, entityVisitor.PredicateAssignments.Count);
            Assert.Equal(6, entityVisitor.PendingAssignments.Count);

            dynamic result = newExpression.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            var tokensExpected = new Dictionary<string, object>
            {
                {"Name", "Shonda Rhimes"},
                {"NewAddressName_AddressLine", null},
                {"NewAddressName_City", null},
                {"NewAddressName_State", null},
                {"NewAddressName_Country", null},
                {"NewAddressName_Location_Latitude", 0.0},
                {"NewAddressName_Location_Longitude", 0.0}
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result) Assert.Equal(tokensExpected[pair.Key], pair.Value);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void PredicateComplexTypeMemberAccessSet(string testContextName, TestContext testContext)
        {
            Expression<Func<ActorNode, bool>> expression = a =>
                (a.Address as AddressWithComplexType).Location.Longitude == new AddressWithComplexType
                {
                    //Using this style, parameters set inline of a member access may or may not make it to the generated pattern, or even throw an exception.
                    //This is because this MemberInit may be taken as an object value, since it was accessed, and then used directly.
                    //This was done mainly for testing. 
                    //Use a => a.Address.Location.Longitude == (double)CypherVariables.Get("ellenPompeo")["NewAddressName_Location_Longitude"] instead.

                    AddressLine = CypherVariables.Get<ActorNode>("shondaRhimes").Address.AddressLine,
                    Location = new Location
                    {
                        Latitude = 4.0,
                        Longitude = (double)CypherVariables.Get("ellenPompeo")["NewAddressName_Location_Longitude"]
                    }
                }.Location.Longitude && a.Name == "Shonda Rhimes";

            var entityVisitor =
                new EntityExpressionVisitor(testContext.QueryContext, expression.Parameters.FirstOrDefault());
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(2, entityVisitor.PredicateAssignments.Count);
            Assert.Single(entityVisitor.PendingAssignments);

            Assert.Equal("NewAddressName_Location_Longitude",
                entityVisitor.PendingAssignments.First().Key.ComplexJsonName);

            dynamic result = newExpression.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            var tokensExpected = new Dictionary<string, object>
            {
                {"Name", "Shonda Rhimes"},
                {"NewAddressName_Location_Longitude", 0.0}
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result) Assert.Equal(tokensExpected[pair.Key], pair.Value);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void PredicateDictionarySet(string testContextName, TestContext testContext)
        {
            Expression<Func<Dictionary<string, object>, bool>> expression = a =>
                a["Address"] == CypherVariables.Get<ActorNode>("ellenPompeo").Address
                && (int)a["Born"] == 1671
                && a["Name"] == "Shonda Rhimes";

            var entityVisitor =
                new EntityExpressionVisitor(testContext.QueryContext, expression.Parameters.FirstOrDefault());
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(3, entityVisitor.PredicateAssignments.Count);
            Assert.Equal(4,
                entityVisitor.PendingAssignments.Count); //the address complex type should have exploded to make 4

            Assert.Equal("NewAddressName_AddressLine", entityVisitor.PendingAssignments.First().Key.ComplexJsonName);
            Assert.Contains("Get(\"ellenPompeo\").Address.AddressLine",
                entityVisitor.PendingAssignments.First().Value.ToString());

            dynamic result = newExpression.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            var tokensExpected = new Dictionary<string, object>
            {
                {"Name", "Shonda Rhimes"},
                {"Born", 1671},
                {"NewAddressName_AddressLine", null},
                {"NewAddressName_City", null},
                {"NewAddressName_State", null},
                {"NewAddressName_Country", null}
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result) Assert.Equal(tokensExpected[pair.Key], pair.Value);
        }
    }
}