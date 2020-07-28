using Neo4jClient.DataAnnotations.Utils;

namespace Neo4jClient.DataAnnotations
{
    public interface IHaveEntityInfo
    {
        EntityTypeInfo EntityInfo { get; }
    }
}