using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public interface ICypherClause : IAnnotated
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        string Clause { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        void CommitToQuery();
    }
}
