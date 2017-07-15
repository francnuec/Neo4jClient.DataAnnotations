﻿using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Tests.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
    public class EntitySerializationTests
    {
        public static List<object[]> SerializerData { get; } = new List<object[]>()
        {
            new object[] { TestUtilities.SerializeWithConverter, "ConverterSerializer" },
            new object[] { TestUtilities.SerializeWithResolver, "ResolverSerializer" },
        };

        public static List<object[]> DeserializerData { get; } = new List<object[]>()
        {
            new object[] { TestUtilities.SerializerSettingsWithConverter, "ConverterSerializerSettings" },
            new object[] { TestUtilities.SerializerSettingsWithResolver, "ResolverSerializerSettings" },
        };

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(EntitySerializationTests))]
        public void NullComplexTypePropertyWrite_InvalidOperationException(Func<object, string> serializer, string name = null)
        {
            TestUtilities.AddEntityTypes();

            var actor = new ActorNode();

            var ex = Assert.Throws<InvalidOperationException>(() => serializer(actor));

            Assert.Equal(string.Format(Messages.NullComplexTypePropertyError, "Address", "PersonNode"), ex.Message);
        }

        [Theory]
        [MemberData("SerializerData", MemberType = typeof(EntitySerializationTests))]
        public void EntityWrite(Func<object, string> serializer, string name = null)
        {
            TestUtilities.AddEntityTypes();

            var actor = new ActorNode<int>()
            {
                Name = "Ellen Pompeo",
                Born = 1969,
                Address = new AddressWithComplexType()
                {
                    //While crude functionality to handle polymorphic instance of complex types is in place, it is advised to not subclass a complex type.
                    //this is because there may be an issue at deserialization.
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

            var serialized = serializer(actor);

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

            foreach (var jChild in jToken.Children())
            {
                Assert.Equal(JTokenType.Property, jChild.Type);

                var property = jChild as JProperty;

                Assert.Contains(property.Name, tokensExpected.Keys);

                var tokenExpected = tokensExpected[property.Name];

                Assert.Equal(tokenExpected.Item1, property.Value.Type);
                Assert.Equal(tokenExpected.Item2, property.Value.ToObject<dynamic>());
            }
        }

        [Theory]
        [MemberData("DeserializerData", MemberType = typeof(EntitySerializationTests))]
        public void EntityRead(JsonSerializerSettings deserializerSettings, string name = null)
        {
            Dictionary<string, dynamic> actorTokens = new Dictionary<string, dynamic>()
            {
                { "Name", "Ellen Pompeo" },
                { "Born", 1969 },
                { "Roles", null},
                { "NewAddressName_AddressLine", null },
                { "NewAddressName_City", "Los Angeles" },
                { "NewAddressName_State", "California" },
                { "NewAddressName_Country", "US" },
                { "NewAddressName_Location_Latitude", 34.0522 },
                { "NewAddressName_Location_Longitude", -118.2437 },
                { "NewAddressName_ComplexProperty_Property", 14859 },
                { "NewAddressName_SomeOtherProperty", "something" },
                { "TestForeignKeyId", 0 },
                { "TestMarkedFK", 0 },
                { "TestGenericForeignKeyId", null },
            };

            TestUtilities.AddEntityTypes();

            var actorJObject = JObject.FromObject(actorTokens);

            var actor = actorJObject.ToObject<ActorNode<int>>(JsonSerializer.CreateDefault(deserializerSettings));

            Assert.NotNull(actor);

            Assert.Equal(typeof(ActorNode<int>), actor.GetType());

            var actorContract = TestUtilities.Resolver.ResolveContract(actor.GetType());

            var jsonProperties = (actorContract as JsonObjectContract)?.Properties;

            Assert.NotNull(jsonProperties);
            Assert.InRange(jsonProperties.Count, actorTokens.Count, int.MaxValue); //the amount of properties returned can't be less than the token sent in

            foreach (var token in actorTokens)
            {
                var jsonProp = jsonProperties.Where(jp => jp.PropertyName == token.Key).SingleOrDefault(); //has to be just one

                Assert.NotNull(jsonProp);

                var jsonPropValue = jsonProp.ValueProvider.GetValue(actor);

                Assert.Equal(token.Value, jsonPropValue);
            }
        }
    }
}
