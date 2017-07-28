using Neo4jClient.DataAnnotations.Serialization;
using System;
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
            Assert.NotNull(Neo4jAnnotations.EntityTypes);
        }

        [Fact]
        public void RegisterWithResolverImplict()
        {
            Neo4jAnnotations.RegisterWithResolver(TestUtilities.EntityTypes);

            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(Neo4jAnnotations.EntityTypes.Contains(type)));

            Assert.Equal(typeof(EntityResolver), GraphClient.DefaultJsonContractResolver.GetType());

            Assert.Contains(GraphClient.DefaultJsonConverters, c => typeof(ResolverDummyConverter) == c.GetType());
        }

        [Fact]
        public void RegisterWithConverterImplicit()
        {
            Neo4jAnnotations.RegisterWithConverter(TestUtilities.EntityTypes);

            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(Neo4jAnnotations.EntityTypes.Contains(type)));

            Assert.Contains(GraphClient.DefaultJsonConverters, c => typeof(EntityConverter) == c.GetType());

            //Assert.Equal(typeof(EntityConverter), GraphClient.DefaultJsonConverters.Last().GetType());
        }

        [Fact]
        public void RegisterWithResolver()
        {
            Neo4jAnnotations.RegisterWithResolver(TestUtilities.EntityTypes, TestUtilities.Resolver);

            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(Neo4jAnnotations.EntityTypes.Contains(type)));

            Assert.True(GraphClient.DefaultJsonContractResolver == TestUtilities.Resolver);

            Assert.Contains(GraphClient.DefaultJsonConverters, c => typeof(ResolverDummyConverter) == c.GetType());
        }

        [Fact]
        public void RegisterWithConverter()
        {
            Neo4jAnnotations.RegisterWithConverter(TestUtilities.EntityTypes, TestUtilities.Converter);

            Assert.All(TestUtilities.EntityTypes, (type) => Assert.True(Neo4jAnnotations.EntityTypes.Contains(type)));

            Assert.True(GraphClient.DefaultJsonConverters.Last() == TestUtilities.Converter);
        }
    }
}
