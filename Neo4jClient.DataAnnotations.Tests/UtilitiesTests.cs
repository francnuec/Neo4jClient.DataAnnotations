using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class UtilitiesTests
    {
        public static List<object[]> ParamsData = new List<object[]>()
        {
            new object[] { (Expression<Func<ActorNode>>)(() => Params.Get<ActorNode>("actor")), "actor" },

            new object[] { (Expression<Func<int>>)(() => (int)Params.Get("actor")["address_location_latitude"]),
                "actor.address_location_latitude" },

            new object[] { (Expression<Func<string>>)(() => ((string[])Params.Get("actor")["roles"])[0]),
                "actor.roles[0]" },

            new object[] { (Expression<Func<string>>)(() => (Params.Get("actor")["roles"] as string[])[0]),
                "actor.roles[0]" },

            new object[] { (Expression<Func<string>>)(() => Params.Get<ActorNode>("actor").Roles.ElementAt(2)),
                "actor.Roles[2]" },

            new object[] { (Expression<Func<Location>>)(() => (Params.Get<ActorNode>("actor").Address as AddressWithComplexType).Location),
                "actor.NewAddressName.Location" },

            new object[] { (Expression<Func<double>>)(() => (Params.Get<ActorNode>("actor").Address as AddressWithComplexType).Location.Latitude),
                "actor.NewAddressName_Location_Latitude" },
        };

        [Theory]
        [MemberData("ParamsData", MemberType = typeof(UtilitiesTests))]
        public void ParamsSerialization<T>(Expression<Func<T>> expression,
            string expectedText, bool useResolvedJsonName = true)
        {
            TestUtilities.AddEntityTypes();

            var retrievedMembers = Utilities.GetSimpleMemberAccessStretch(expression.Body, out var val);

            Assert.Equal(true, Utilities.HasParams(retrievedMembers));

            var paramText = Utilities
                .BuildParams(retrievedMembers, TestUtilities.Resolver, TestUtilities.SerializeWithResolver,
                out var typeReturned, useResolvedJsonName: useResolvedJsonName);

            Assert.Equal(expectedText, paramText);
            Assert.Equal(typeof(T), typeReturned);
        }
    }
}
