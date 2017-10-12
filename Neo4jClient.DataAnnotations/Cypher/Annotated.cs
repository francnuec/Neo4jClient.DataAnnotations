using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public abstract class Annotated : IAnnotated //, ICypherFluentQuery
    {
        private ICypherFluentQuery internalCypherQuery;

        public Annotated(ICypherFluentQuery query)
        {
            internalCypherQuery = query ?? throw new ArgumentNullException(nameof(query));
        }

        public virtual ref ICypherFluentQuery CypherQuery
        {
            get
            {
                return ref internalCypherQuery;
            }
        }

        public string Build(ref ICypherFluentQuery query)
        {
            internalCypherQuery = query;

            var build = InternalBuild();

            query = CypherQuery;

            return build;
        }

        protected abstract string InternalBuild();
    }
}
