using Neo4jClient.DataAnnotations.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class Neo4jAnnotationsTests
    {
        [Fact]
        public void EntityTypesAtStart_NotNull()
        {
            Assert.NotNull(Neo4jAnnotations.EntityTypes);
        }

        [Fact]
        public void RegisterWithResolverImplict()
        {
            Neo4jAnnotations.RegisterWithResolver(TestUtilities.EntityTypes);

            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(Neo4jAnnotations.EntityTypes.Contains(type)));

            Assert.Equal(GraphClient.DefaultJsonContractResolver.GetType(), typeof(EntityResolver));
        }

        [Fact]
        public void RegisterWithConverterImplicit()
        {
            Neo4jAnnotations.RegisterWithConverter(TestUtilities.EntityTypes);

            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(Neo4jAnnotations.EntityTypes.Contains(type)));

            Assert.Equal(GraphClient.DefaultJsonConverters[0].GetType(), typeof(EntityConverter));
        }

        [Fact]
        public void RegisterWithResolver()
        {
            Neo4jAnnotations.RegisterWithResolver(TestUtilities.EntityTypes, TestUtilities.Resolver);

            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(Neo4jAnnotations.EntityTypes.Contains(type)));

            Assert.Same(GraphClient.DefaultJsonContractResolver, TestUtilities.Resolver);
        }

        [Fact]
        public void RegisterWithConverter()
        {
            Neo4jAnnotations.RegisterWithConverter(TestUtilities.EntityTypes, TestUtilities.Converter);

            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(Neo4jAnnotations.EntityTypes.Contains(type)));

            Assert.Same(GraphClient.DefaultJsonConverters[0], TestUtilities.Converter);
        }
    }
}
