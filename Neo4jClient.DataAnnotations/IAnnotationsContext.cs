using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Serialization;

namespace Neo4jClient.DataAnnotations
{
    internal interface IAnnotationsContext
    {
        IGraphClient GraphClient { get; }
        EntityService EntityService { get; }
        EntityResolver EntityResolver { get; }
        EntityResolverConverter EntityResolverConverter { get; }
        EntityConverter EntityConverter { get; }
        ICypherFluentQuery Cypher { get; }
        bool IsBoltClient { get; }
    }
}