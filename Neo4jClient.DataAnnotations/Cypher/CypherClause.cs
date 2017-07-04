using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public class CypherClause : Annotated, ICypherClause
    {
        public CypherClause(ICypherFluentQuery query, string cypherClause)
            :base(query)
        {
            this.Clause = cypherClause;
        }

        public string Clause { get; }

        public override string Build()
        {
            throw new NotImplementedException();
        }

        public void CommitToQuery()
        {

        }
    }
}
