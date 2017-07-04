using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public interface IAnnotated
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        ICypherFluentQuery CypherQuery { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        string Build();
    }
}
