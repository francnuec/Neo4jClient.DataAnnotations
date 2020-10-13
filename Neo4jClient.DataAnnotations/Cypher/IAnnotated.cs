using System.ComponentModel;
using Neo4jClient.Cypher;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public interface IAnnotated : IHaveAnnotationsContext
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        ref ICypherFluentQuery CypherQuery { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        string Build(ref ICypherFluentQuery currentQuery);
    }
}