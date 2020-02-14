using Neo4jClient.DataAnnotations.Serialization;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class Neo4jAnnotationsTests
    {
        [Theory]
        [MemberData(nameof(TestUtilities.TestContextData), MemberType = typeof(TestUtilities))]
        public void EntityTypesAtStart_NotNull(string testContextName, TestContext testContext)
        {
            Assert.NotNull(testContext.AnnotationsContext.EntityService.EntityTypes);
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestResolverContextData), MemberType = typeof(TestUtilities))]
        public void RegisterWithResolverImplict(string testContextName, TestContext testContext)
        {
            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(testContext.AnnotationsContext.EntityService.EntityTypes.Contains(type)));

            Assert.Equal(typeof(EntityResolver), testContext.QueryContext.Client.JsonContractResolver.GetType());

            Assert.Contains(testContext.QueryContext.Client.JsonConverters, c => typeof(EntityResolverConverter) == c.GetType());
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestConverterContextData), MemberType = typeof(TestUtilities))]
        public void RegisterWithConverterImplicit(string testContextName, TestContext testContext)
        {
            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(testContext.AnnotationsContext.EntityService.EntityTypes.Contains(type)));

            Assert.Contains(testContext.QueryContext.Client.JsonConverters, c => typeof(EntityConverter) == c.GetType());

            //Assert.Equal(typeof(EntityConverter), client.JsonConverters.Last().GetType());
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestResolverContextData), MemberType = typeof(TestUtilities))]
        public void RegisterWithResolver(string testContextName, TestContext testContext)
        {
            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(testContext.AnnotationsContext.EntityService.EntityTypes.Contains(type)));

            Assert.True(testContext.QueryContext.Client.JsonContractResolver == testContext.AnnotationsContext.EntityResolver);

            Assert.Contains(testContext.QueryContext.Client.JsonConverters, c => typeof(EntityResolverConverter) == c.GetType());
        }

        [Theory]
        [MemberData(nameof(TestUtilities.TestConverterContextData), MemberType = typeof(TestUtilities))]
        public void RegisterWithConverter(string testContextName, TestContext testContext)
        {
            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(testContext.AnnotationsContext.EntityService.EntityTypes.Contains(type)));

            Assert.True(testContext.QueryContext.Client.JsonConverters.Last() == testContext.AnnotationsContext.EntityConverter);
        }
    }
}
