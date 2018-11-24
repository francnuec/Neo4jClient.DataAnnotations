using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Serialization;

namespace Neo4jClient.DataAnnotations
{
    public interface IAnnotationsContext
    {
        IGraphClient GraphClient { get; }
        IEntityService EntityService { get; }
        EntityResolver EntityResolver { get; }
        EntityResolverConverter EntityResolverConverter { get; }
        EntityConverter EntityConverter { get; }
        ICypherFluentQuery Cypher { get; }
    }
}