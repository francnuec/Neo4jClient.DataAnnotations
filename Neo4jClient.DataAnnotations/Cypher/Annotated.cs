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
        ///// <summary>
        ///// Gets the <see cref="CypherQuery"/> without a preceding call to <see cref="CommitToCypherQuery"/>.
        ///// Internally, in the <see cref="CommitToCypherQuery"/> method, use this property to get any methods from <see cref="ICypherFluentQuery"/>.
        ///// This is to avoid starting an infinite loop.
        ///// </summary>
        protected ICypherFluentQuery InternalCypherQuery { get; private set; }

        public Annotated(ICypherFluentQuery query)
        {
            this.InternalCypherQuery = query ?? throw new ArgumentNullException(nameof(query));
        }

        //public Annotated(Annotated existing)
        //    : this(existing?.InternalCypherQuery)
        //{
        //    if (existing == null)
        //        throw new ArgumentNullException(nameof(existing));

        //    //existing.CommitToCypherQuery();
        //}

        public virtual ICypherFluentQuery CypherQuery
        {
            get
            {
                //CommitToCypherQuery();
                return InternalCypherQuery;
            }
        }

        public abstract string Build();
    }
}
