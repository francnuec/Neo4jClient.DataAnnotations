using Neo4jClient.DataAnnotations.Serialization;

namespace Neo4jClient.DataAnnotations
{
    public interface IAnnotationsContext
    {
        EntityConverter EntityConverter { get; }
        EntityResolver EntityResolver { get; }
        IEntityService EntityService { get; }
        IGraphClient GraphClient { get; }
        EntityResolverConverter EntityResolverConverter { get; }
    }
}