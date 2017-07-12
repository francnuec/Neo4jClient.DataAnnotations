using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Tests.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Xunit;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class EntityConverterTests
    {
        [Fact]
        public void NullComplexTypePropertyWrite_InvalidOperationException()
        {
            Neo4jAnnotations.AddEntityType(typeof(ActorNode));

            var actor = new ActorNode();

            var ex = Assert.Throws<InvalidOperationException>(() => JsonConvert.SerializeObject(actor, new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new EntityConverter() },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            }));

            Assert.Equal(string.Format(Messages.NullComplexTypePropertyError, "Address", "PersonNode"), ex.Message);
        }

        [Fact]
        public void EntityWrite()
        {
            Neo4jAnnotations.AddEntityType(typeof(ActorNode<>));

            var actor = new ActorNode<int>()
            {
                Name = "Ellen Pompeo",
                Born = 1969,
                Address = new AddressWithComplexType()
                {
                    City = "Los Angeles",
                    State = "California",
                    Country = "US",
                    Location = new Location()
                    {
                        Latitude = 34.0522,
                        Longitude = -118.2437
                    }
                }
            };

            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new EntityConverter() },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            Expression<Func<object>> _f = () => new { ar = new double[] { (double)Params.Get("wh")[""] } };
            Expression<Func<object>> _f1 = () => new { Location = (TestUtilities.Actor.Address as AddressWithComplexType).Location._() };
            Expression<Func<object>> _f2 = () => new { Location = new AddressWithComplexType() { Location = new Location() { Latitude = (double)Params.Get("wh")[""] } } };
            Expression<Func<object>> _f3 = () => new { new AddressWithComplexType() { AddressLine = Params.Get("al")["yes"] as string, Location = new Location() { Latitude = (double)Params.Get("wh")[""], Longitude = (double)Params.Get("lg")[""] } }.Location };

            Expression<Func<object>> f = () => new { ar = new double[] { (double)Params.Get("wh")[""] }, Location = (TestUtilities.Actor.Address as AddressWithComplexType).Location._() };
            Expression<Func<object>> f2 = () => new { L = 123.ToString() }; //((AddressWithComplexType)actor.Address).Location };
            Expression<Func<object>> f3 = () => new { _ = ((AddressWithComplexType)actor.Address).Location };
            Expression<Func<ActorNode, bool>> f4 =
                (a) => a == Params.Get<ActorNode>("actor")
                && (a.Address as AddressWithComplexType).Location.Latitude == (double)Params.Get("actor")["address_location_latitude"]
                && a.Roles[0] == ((string[])Params.Get("actor")["roles"])[0]
                && a.Roles[0] == (Params.Get("actor")["roles"] as string[])[0]
                && a.Roles.ElementAt(0) == Params.Get<ActorNode>("actor").Roles.ElementAt(2)
                && (a.Address as AddressWithComplexType).Location == (Params.Get<ActorNode>("actor").Address as AddressWithComplexType).Location
                && ((AddressWithComplexType)a.Address).Location.Latitude == (Params.Get<ActorNode>("actor").Address as AddressWithComplexType).Location.Latitude;
            Expression<Func<object>> f5 = () => new ActorNode()
            {
                Address = new AddressWithComplexType()
                {
                    Location = new Location()
                    {
                        Latitude = 0.0
                    }
                }
            };

            Expression<Func<object>> f9 = () => new Dictionary<string, object>() { { "new", "yes" }, { "miss", "gone" } }.With(dict => dict["miss"] as string == "fresh" && dict["new"] == (object)34);

            Expression<Func<object>> f6 = () => new { actor.Name, actor.Born, Address = actor.Address as AddressWithComplexType }
            .With(t => t.Address == new AddressWithComplexType() { AddressLine = Params.Get<ActorNode>("shonda").Address.AddressLine, Location = new Location() { Longitude = (double)Params.Get("f")["yes"] } } &&  t.Name == "New Guy"); //actor.With(a => a.Born == Params.Get<ActorNode>("shondaRhimes").Born && a.Name == "New Guy");

            Expression<Func<object>> f7 = () => new { actor.Name, actor.Born, actor.Address }
            .With(t => t.Address.AddressLine == Params.Get<ActorNode>("shonda").Address.AddressLine && t.Name == "New Guy");

            //var set = Expression.Assign(Expression.Property(null, typeof(TestObjects).GetTypeInfo().GetProperty("Email")), Expression.Constant("Whatever"));
            //var set =  Expression.MemberInit(Expression.New(typeof(TestObjects)), Expression.Bind(typeof(TestObjects).GetTypeInfo().GetProperty("Email"), Expression.Constant("Whatever")));

            //try
            //{
            //    var t = set.ExecuteExpression<TestObjects>();
            //    var em = t.Email;
            //}
            //catch (Exception e)
            //{

            //}

            //var te = new TestUtilities();
            //te.checkvariable();


            var ex = new EntityExpressionVisitor((entity) => JsonConvert.SerializeObject(entity, serializerSettings));
            var v = ex.Visit(f6.Body);

            //var exprs = ex.Params; //.FilteredExpressions.Where((e, i) => i >= 16 && i <= 18).ToList();

            //var paramText = Utilities.BuildParams(exprs, (entity) => JsonConvert.SerializeObject(entity, serializerSettings),
            //    out var typeRet);

            var serialized = JsonConvert.SerializeObject(actor, serializerSettings);

            Dictionary<string, Tuple<JTokenType, dynamic>> tokensExpected = new Dictionary<string, Tuple<JTokenType, dynamic>>()
            {
                { "Name", new Tuple<JTokenType, dynamic>(JTokenType.String, "Ellen Pompeo") },
                { "Born", new Tuple<JTokenType, dynamic>(JTokenType.Integer, 1969) },
                { "Roles", new Tuple<JTokenType, dynamic>(JTokenType.Null, null)},
                { "NewAddressName_AddressLine", new Tuple<JTokenType, dynamic>(JTokenType.Null, null) },
                { "NewAddressName_City", new Tuple<JTokenType, dynamic>(JTokenType.String, "Los Angeles") },
                { "NewAddressName_State", new Tuple<JTokenType, dynamic>(JTokenType.String, "California") },
                { "NewAddressName_Country", new Tuple<JTokenType, dynamic>(JTokenType.String, "US") },
                { "NewAddressName_Location_Latitude", new Tuple<JTokenType, dynamic>(JTokenType.Float, 34.0522) },
                { "NewAddressName_Location_Longitude", new Tuple<JTokenType, dynamic>(JTokenType.Float, -118.2437) },
                { "TestForeignKeyId", new Tuple<JTokenType, dynamic>(JTokenType.Integer, 0) },
                { "TestMarkedFK", new Tuple<JTokenType, dynamic>(JTokenType.Integer, 0) },
                { "TestGenericForeignKeyId", new Tuple<JTokenType, dynamic>(JTokenType.Null, null) },
            };

            var jToken = JToken.Parse(serialized) as JObject;

            Assert.Equal(JTokenType.Object, jToken.Type);

            Assert.Equal(tokensExpected.Count, jToken.Count);

            foreach(var jChild in jToken.Children())
            {
                Assert.Equal(JTokenType.Property, jChild.Type);

                var property = jChild as JProperty;

                Assert.Contains(property.Name, tokensExpected.Keys);

                var tokenExpected = tokensExpected[property.Name];

                Assert.Equal(tokenExpected.Item1, property.Value.Type);
                Assert.Equal(tokenExpected.Item2, property.Value.ToObject<dynamic>());
            }
        }
    }
}
