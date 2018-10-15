using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Cypher.Extensions;
using Neo4jClient.DataAnnotations.Expressions;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Tests.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class EntityExpressionVisitorTests
    {
        public static List<object[]> SerializerData { get; } = new List<object[]>()
        {
            new object[] { null, TestUtilities.SerializeWithConverter },
            new object[] { TestUtilities.Resolver, null },
        };

        [Theory]
        [MemberData(nameof(SerializerData), MemberType = typeof(EntityExpressionVisitorTests))]
        public void AnonymousType(EntityResolver resolver, Func<object, string> serializer)
        {
            TestUtilities.RegisterEntityTypes(resolver, resolver == null ? TestUtilities.Converter : null);

            Expression<Func<object>> expression = () =>
                new
                {
                    Name = "Ellen Pompeo",
                    Born = Vars.Get<ActorNode>("shondaRhimes").Born,
                    Roles = new string[] { "Meredith Grey" },
                    Age = 47.ToString()
                };

            var entityVisitor = new EntityExpressionVisitor(new QueryUtilities()
            {
                Resolver = resolver,
                SerializeCallback = serializer
            });
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(3, entityVisitor.SpecialNodes[0].Filtered.Count); //because we are accounting for an implicit convert to object type.
            Assert.Equal("Get(\"shondaRhimes\")", entityVisitor.SpecialNodes[0].Filtered[0].ToString());
            Assert.Equal(3, entityVisitor.SpecialNodePaths[0].Item1.Count);

            var result = newExpression.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            Dictionary<string, object> tokensExpected = new Dictionary<string, object>()
            {
                { "Name", "Ellen Pompeo" },
                { "Born", null }, //to be later replaced at serialization with params
                { "Roles", new string[] { "Meredith Grey" } },
                { "Age", "47" }
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result)
            {
                Assert.Equal(tokensExpected[pair.Key], pair.Value);
            }
        }

        [Theory]
        [MemberData(nameof(SerializerData), MemberType = typeof(EntityExpressionVisitorTests))]
        public void ComplexAnonymousType_ComplexName(EntityResolver resolver, Func<object, string> serializer)
        {
            TestUtilities.RegisterEntityTypes(resolver, resolver == null ? TestUtilities.Converter : null);

            //the following is purely a test, and not necessarily a good example for neo4j cypher.
            Expression<Func<object>> expression = () => new
            {
                new AddressWithComplexType()
                {
                    AddressLine = Vars.Get("A")["AddressLine"] as string,
                    Location = new Location()
                    {
                        Latitude = (double)Vars.Get("A")["Location_Latitude"],
                        Longitude = 56.90
                    }
                }.Location
            };

            var entityVisitor = new EntityExpressionVisitor(new QueryUtilities()
            {
                Resolver = resolver,
                SerializeCallback = serializer
            });
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(3, entityVisitor.SpecialNodes.Count);

            Assert.Equal(3, entityVisitor.SpecialNodes[1].Filtered.Count);
            Assert.Equal("Get(\"A\").get_Item(\"AddressLine\")", entityVisitor.SpecialNodes[1].Filtered[1].ToString());
            Assert.Equal(5, entityVisitor.SpecialNodePaths[1].Item1.Count);

            var result = newExpression.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            Dictionary<string, object> tokensExpected = new Dictionary<string, object>()
            {
                { "Location_Latitude", 0.0 }, //because the inner property was assigned 0 by the GetVars method.
                { "Location_Longitude", 56.90 }
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result)
            {
                Assert.Equal(tokensExpected[pair.Key], pair.Value);
            }
        }

        [Theory]
        [MemberData(nameof(SerializerData), MemberType = typeof(EntityExpressionVisitorTests))]
        public void EscapedComplexAnonymousType_SimpleName(EntityResolver resolver, Func<object, string> serializer)
        {
            TestUtilities.RegisterEntityTypes(resolver, resolver == null ? TestUtilities.Converter : null);

            //the following is purely a test, and not necessarily a good example for neo4j cypher.
            Expression<Func<object>> expression = () => new
            {
                Address = (TestUtilities.Actor.Address as AddressWithComplexType)._(),
                Coordinates = new double[] { (TestUtilities.Actor.Address as AddressWithComplexType).Location.Latitude,
                    (double)Vars.Get("shondaRhimes")["NewAddressName_Location_Longitude"] }
            };

            var entityVisitor = new EntityExpressionVisitor(new QueryUtilities()
            {
                Resolver = resolver,
                SerializeCallback = serializer
            });
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(1, entityVisitor.SpecialNodes.Count);

            Assert.Equal(3, entityVisitor.SpecialNodes[0].Filtered.Count);
            Assert.Equal("Get(\"shondaRhimes\")", entityVisitor.SpecialNodes[0].Filtered[0].ToString());
            Assert.Equal(4, entityVisitor.SpecialNodePaths[0].Item1.Count);

            var result = newExpression.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            Dictionary<string, object> tokensExpected = new Dictionary<string, object>()
            {
                { "Address", TestUtilities.Actor.Address as AddressWithComplexType }, //same object returned because it was escaped
                { "Coordinates", new double[] { (TestUtilities.Actor.Address as AddressWithComplexType).Location.Latitude, 0.0 } }
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result)
            {
                Assert.Equal(tokensExpected[pair.Key], pair.Value);
            }
        }

        [Theory]
        [MemberData(nameof(SerializerData), MemberType = typeof(EntityExpressionVisitorTests))]
        public void Set(EntityResolver resolver, Func<object, string> serializer)
        {
            TestUtilities.RegisterEntityTypes(resolver, resolver == null ? TestUtilities.Converter : null);

            Expression<Func<object>> expression = () => TestUtilities.Actor._Set(a => a.Born == Vars.Get<ActorNode>("ellenPompeo").Born && a.Name == "Shonda Rhimes");

            var entityVisitor = new EntityExpressionVisitor(new QueryUtilities()
            {
                Resolver = resolver,
                SerializeCallback = serializer
            });
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(1, entityVisitor.SpecialNodes.Count);

            Assert.Equal(2, entityVisitor.SpecialNodes[0].Filtered.Count);
            Assert.Equal("Get(\"ellenPompeo\").Born", entityVisitor.SpecialNodes[0].Filtered[1].ToString());
            Assert.Equal(3, entityVisitor.SpecialNodePaths[0].Item1.Count);

            var result = entityVisitor.SetPredicateNode.ExecuteExpression<ActorNode>();
            Assert.NotNull(result);

            Assert.Equal("Shonda Rhimes", result.Name);
            Assert.Equal(0, result.Born);
        }

        [Theory]
        [MemberData(nameof(SerializerData), MemberType = typeof(EntityExpressionVisitorTests))]
        public void AnonymousTypeMemberAccessSet(EntityResolver resolver, Func<object, string> serializer)
        {
            TestUtilities.RegisterEntityTypes(resolver, resolver == null ? TestUtilities.Converter : null);

            Expression<Func<object>> expression = () => new { TestUtilities.Actor.Name, TestUtilities.Actor.Born, TestUtilities.Actor.Address }
                ._Set(a => a.Address.AddressLine == Vars.Get<ActorNode>("shondaRhimes").Address.AddressLine && a.Name == "Shonda Rhimes");

            var entityVisitor = new EntityExpressionVisitor(new QueryUtilities()
            {
                Resolver = resolver,
                SerializeCallback = serializer
            });
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(2, entityVisitor.SpecialNodes.Count);

            Assert.Equal(3, entityVisitor.SpecialNodes[1].Filtered.Count);
            Assert.Equal("Get(\"shondaRhimes\").Address", entityVisitor.SpecialNodes[1].Filtered[1].ToString());
            Assert.Equal(3, entityVisitor.SpecialNodePaths[0].Item1.Count);

            dynamic result = entityVisitor.SetPredicateNode.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            Dictionary<string, object> tokensExpected = new Dictionary<string, object>()
            {
                { "Name", "Shonda Rhimes"},
                { "Born", 0 },
                { "NewAddressName_AddressLine", null },
                { "NewAddressName_City", null },
                { "NewAddressName_State", null },
                { "NewAddressName_Country", null }
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result)
            {
                Assert.Equal(tokensExpected[pair.Key], pair.Value);
            }
        }

        [Theory]
        [MemberData(nameof(SerializerData), MemberType = typeof(EntityExpressionVisitorTests))]
        public void ComplexAnonymousTypeSet(EntityResolver resolver, Func<object, string> serializer)
        {
            TestUtilities.RegisterEntityTypes(resolver, resolver == null ? TestUtilities.Converter : null);

            Expression<Func<object>> expression = () => new { TestUtilities.Actor.Name, TestUtilities.Actor.Born, Address = TestUtilities.Actor.Address as AddressWithComplexType }
            ._Set(a => a.Address == new AddressWithComplexType()
            {   //Use this style only if you're sure all the properties here are assigned, 
                //because this address object would replace the instance address property entirely.
                //Also note that there's a good chance the parameters set inline here wouldn't make it to the generated pattern.
                //This was done mainly for testing. 
                //Use a => a.Address.Location.Longitude == (double)Vars.Get("ellenPompeo")["NewAddressName_Location_Longitude"] instead.

                AddressLine = Vars.Get<ActorNode>("shondaRhimes").Address.AddressLine,
                Location = new Location()
                {
                    Latitude = 4.0,
                    Longitude = (double)Vars.Get("ellenPompeo")["NewAddressName_Location_Longitude"]
                }
            } && a.Name == "Shonda Rhimes");

            var entityVisitor = new EntityExpressionVisitor(new QueryUtilities()
            {
                Resolver = resolver,
                SerializeCallback = serializer
            });
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(3, entityVisitor.SpecialNodes.Count);

            dynamic result = entityVisitor.SetPredicateNode.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            Dictionary<string, object> tokensExpected = new Dictionary<string, object>()
            {
                { "Name", "Shonda Rhimes"},
                { "Born", 0 },
                { "NewAddressName_AddressLine", null },
                { "NewAddressName_City", null },
                { "NewAddressName_State", null },
                { "NewAddressName_Country", null },
                { "NewAddressName_Location_Latitude", 4.0 },
                { "NewAddressName_Location_Longitude", 0.0 }
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result)
            {
                Assert.Equal(tokensExpected[pair.Key], pair.Value);
            }
        }

        [Theory]
        [MemberData(nameof(SerializerData), MemberType = typeof(EntityExpressionVisitorTests))]
        public void ComplexAnonymousTypeMemberAccessSet(EntityResolver resolver, Func<object, string> serializer)
        {
            TestUtilities.RegisterEntityTypes(resolver, resolver == null ? TestUtilities.Converter : null);

            Expression<Func<object>> expression = () => new { TestUtilities.Actor.Name, TestUtilities.Actor.Born, Address = TestUtilities.Actor.Address as AddressWithComplexType }
            ._Set(a => a.Address.Location.Longitude == new AddressWithComplexType()
            {   //Using this style, parameters set inline of a member access may or may not make it to the generated pattern, or even throw an exception.
                //This is because this MemberInit may be taken as an object value, since it was accessed, and then used directly.
                //This was done mainly for testing. 
                //Use a => a.Address.Location.Longitude == (double)Vars.Get("ellenPompeo")["NewAddressName_Location_Longitude"] instead.

                AddressLine = Vars.Get<ActorNode>("shondaRhimes").Address.AddressLine,
                Location = new Location()
                {
                    Latitude = 4.0,
                    Longitude = (double)Vars.Get("ellenPompeo")["NewAddressName_Location_Longitude"]
                }
            }.Location.Longitude && a.Name == "Shonda Rhimes");

            var entityVisitor = new EntityExpressionVisitor(new QueryUtilities()
            {
                Resolver = resolver,
                SerializeCallback = serializer
            });
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(4, entityVisitor.SpecialNodes.Count);

            dynamic result = entityVisitor.SetPredicateNode.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            Dictionary<string, object> tokensExpected = new Dictionary<string, object>()
            {
                { "Name", "Shonda Rhimes"},
                { "Born", 0 },
                { "NewAddressName_AddressLine", null },
                { "NewAddressName_City", null },
                { "NewAddressName_State", null },
                { "NewAddressName_Country", null },
                { "NewAddressName_Location_Latitude", 0.0 },
                { "NewAddressName_Location_Longitude", 0.0 }
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result)
            {
                Assert.Equal(tokensExpected[pair.Key], pair.Value);
            }
        }

        [Theory]
        [MemberData(nameof(SerializerData), MemberType = typeof(EntityExpressionVisitorTests))]
        public void AnonymousTypeComplexMemberSet(EntityResolver resolver, Func<object, string> serializer)
        {
            TestUtilities.RegisterEntityTypes(resolver, resolver == null ? TestUtilities.Converter : null);

            Expression<Func<object>> expression = () => new { TestUtilities.Actor.Name, TestUtilities.Actor.Born, TestUtilities.Actor.Address }
                ._Set(a => (a.Address as AddressWithComplexType).AddressLine == Vars.Get<ActorNode>("shondaRhimes").Address.AddressLine && a.Name == "Shonda Rhimes");

            var entityVisitor = new EntityExpressionVisitor(new QueryUtilities()
            {
                Resolver = resolver,
                SerializeCallback = serializer
            });
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(2, entityVisitor.SpecialNodes.Count);

            Assert.Equal(3, entityVisitor.SpecialNodes[1].Filtered.Count);
            Assert.Equal("Get(\"shondaRhimes\").Address", entityVisitor.SpecialNodes[1].Filtered[1].ToString());
            Assert.Equal(3, entityVisitor.SpecialNodePaths[0].Item1.Count);

            dynamic result = entityVisitor.SetPredicateNode.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            Dictionary<string, object> tokensExpected = new Dictionary<string, object>()
            {
                { "Name", "Shonda Rhimes"},
                { "Born", 0 },
                { "NewAddressName_AddressLine", null },
                { "NewAddressName_City", null },
                { "NewAddressName_State", null },
                { "NewAddressName_Country", null },
                { "NewAddressName_Location_Latitude", 0.0 }, //
                { "NewAddressName_Location_Longitude", 0.0 } //
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result)
            {
                Assert.Equal(tokensExpected[pair.Key], pair.Value);
            }
        }

        [Theory]
        [MemberData(nameof(SerializerData), MemberType = typeof(EntityExpressionVisitorTests))]
        public void AnonymousTypeComplexMemberSet2(EntityResolver resolver, Func<object, string> serializer)
        {
            TestUtilities.RegisterEntityTypes(resolver, resolver == null ? TestUtilities.Converter : null);

            Expression<Func<object>> expression = () => new { TestUtilities.Actor.Name, TestUtilities.Actor.Born, TestUtilities.Actor.Address }
            ._Set(a => a.Address == new AddressWithComplexType()
            {   //Use this style only if you're sure all the properties here are assigned, 
                //because this address object would replace the instance address property entirely.
                //Also note that there's a good chance the parameters set inline here wouldn't make it to the generated pattern.
                //This was done mainly for testing. 
                //Use a => a.Address.Location.Longitude == (double)Vars.Get("ellenPompeo")["NewAddressName_Location_Longitude"] instead.

                AddressLine = Vars.Get<ActorNode>("shondaRhimes").Address.AddressLine,
                Location = new Location()
                {
                    Latitude = 4.0,
                    Longitude = (double)Vars.Get("ellenPompeo")["NewAddressName_Location_Longitude"]
                }
            } && a.Name == "Shonda Rhimes");

            var entityVisitor = new EntityExpressionVisitor(new QueryUtilities()
            {
                Resolver = resolver,
                SerializeCallback = serializer
            });
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(3, entityVisitor.SpecialNodes.Count);

            dynamic result = entityVisitor.SetPredicateNode.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            Dictionary<string, object> tokensExpected = new Dictionary<string, object>()
            {
                { "Name", "Shonda Rhimes"},
                { "Born", 0 },
                { "NewAddressName_AddressLine", null },
                { "NewAddressName_City", null },
                { "NewAddressName_State", null },
                { "NewAddressName_Country", null },
                { "NewAddressName_Location_Latitude", 4.0 }, //
                { "NewAddressName_Location_Longitude", 0.0 } //
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result)
            {
                Assert.Equal(tokensExpected[pair.Key], pair.Value);
            }
        }

        [Theory]
        [MemberData(nameof(SerializerData), MemberType = typeof(EntityExpressionVisitorTests))]
        public void DictionarySet(EntityResolver resolver, Func<object, string> serializer)
        {
            TestUtilities.RegisterEntityTypes(resolver, resolver == null ? TestUtilities.Converter : null);

            Expression<Func<object>> expression = () => new Dictionary<string, object>()
            {
                { "Name", TestUtilities.Actor.Name },
                { "Born", TestUtilities.Actor.Born },
                { "Address", TestUtilities.Actor.Address }
            }._Set(a => a["Address"] == Vars.Get<ActorNode>("ellenPompeo").Address && (int)a["Born"] == 1671 && a["Name"] == "Shonda Rhimes");

            var entityVisitor = new EntityExpressionVisitor(new QueryUtilities()
            {
                Resolver = resolver,
                SerializeCallback = serializer
            });
            var newExpression = entityVisitor.Visit(expression.Body);

            Assert.NotNull(newExpression);
            Assert.NotEqual(newExpression, expression.Body);

            Assert.Equal(4, entityVisitor.SpecialNodes.Count);

            Assert.Equal(4, entityVisitor.SpecialNodes[0].Filtered.Count);
            Assert.Equal("Get(\"ellenPompeo\").Address", entityVisitor.SpecialNodes[0].Filtered[1].ToString());
            Assert.Equal(3, entityVisitor.SpecialNodePaths[0].Item1.Count);

            dynamic result = entityVisitor.SetPredicateNode.ExecuteExpression<Dictionary<string, object>>();
            Assert.NotNull(result);

            Dictionary<string, object> tokensExpected = new Dictionary<string, object>()
            {
                { "Name", "Shonda Rhimes"},
                { "Born", 1671 },
                { "NewAddressName_AddressLine", null },
                { "NewAddressName_City", null },
                { "NewAddressName_State", null },
                { "NewAddressName_Country", null }
            };

            Assert.Equal(tokensExpected.Count, result.Count);

            foreach (var pair in result)
            {
                Assert.Equal(tokensExpected[pair.Key], pair.Value);
            }
        }
    }
}
