using Neo4jClient.Cypher;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

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
