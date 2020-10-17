using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Xunit;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Tests.Models;
using System.Linq;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class LiveTests
    {
        IGraphClient graphClient;

        IGraphClient GraphClient
        {
            get
            {
                if (graphClient == null)
                {
                    graphClient = new BoltGraphClient("bolt://localhost:11006", "neo4j", "testdb");
                    graphClient.ConnectAsync().Wait();
                    graphClient = graphClient.WithAnnotations<TestAnnotationsContext>();
                }

                return graphClient;
            }
        }

        [Fact]
        public void Create()
        {
            var actor = TestUtilities.Actor;
            var query = GraphClient.Cypher.Create(p => p.Pattern<ActorNode>("a").Prop(() => actor));
            query.ExecuteWithoutResultsAsync().Wait();
        }
    }
}
