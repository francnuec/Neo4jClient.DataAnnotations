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
        [Fact]
        public void EntityTypesAtStart_NotNull()
        {
            Assert.NotNull(new ResolverTestContext().AnnotationsContext.EntityService.EntityTypes);
            Assert.NotNull(new ConverterTestContext().AnnotationsContext.EntityService.EntityTypes);
        }

        [Fact]
        public void RegisterWithResolverImplict()
        {
            var testContext = new ResolverTestContext();

            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(testContext.AnnotationsContext.EntityService.EntityTypes.Contains(type)));

            Assert.Equal(typeof(EntityResolver), testContext.QueryContext.Client.JsonContractResolver.GetType());

            Assert.Contains(testContext.QueryContext.Client.JsonConverters, c => typeof(EntityResolverConverter) == c.GetType());
        }

        [Fact]
        public void RegisterWithConverterImplicit()
        {
            var testContext = new ConverterTestContext();

            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(testContext.AnnotationsContext.EntityService.EntityTypes.Contains(type)));

            Assert.Contains(testContext.QueryContext.Client.JsonConverters, c => typeof(EntityConverter) == c.GetType());

            //Assert.Equal(typeof(EntityConverter), client.JsonConverters.Last().GetType());
        }

        [Fact]
        public void RegisterWithResolver()
        {
            var testContext = new ResolverTestContext();

            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(testContext.AnnotationsContext.EntityService.EntityTypes.Contains(type)));

            Assert.True(testContext.QueryContext.Client.JsonContractResolver == testContext.AnnotationsContext.EntityResolver);

            Assert.Contains(testContext.QueryContext.Client.JsonConverters, c => typeof(EntityResolverConverter) == c.GetType());
        }

        [Fact]
        public void RegisterWithConverter()
        {
            var testContext = new ConverterTestContext();

            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(testContext.AnnotationsContext.EntityService.EntityTypes.Contains(type)));

            Assert.True(testContext.QueryContext.Client.JsonConverters.Last() == testContext.AnnotationsContext.EntityConverter);
        }
    }
}
