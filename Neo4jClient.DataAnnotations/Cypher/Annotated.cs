using Neo4jClient.Cypher;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public abstract class Annotated : IAnnotated, IHaveAnnotationsContext, IHaveEntityService //, ICypherFluentQuery
    {
        private ICypherFluentQuery internalCypherQuery;

        public Annotated(ICypherFluentQuery query)
        {
            internalCypherQuery = query ?? throw new ArgumentNullException(nameof(query));
        }

        public Annotated(ICypherFluentQuery query, AnnotationsContext context = null) : this(query)
        {
            AnnotationsContext = context;
        }

        public virtual ref ICypherFluentQuery CypherQuery
        {
            get
            {
                return ref internalCypherQuery;
            }
        }

        private AnnotationsContext context;

        public AnnotationsContext AnnotationsContext
        {
            get
            {
                if (context == null)
                {
                    context = CypherQuery.GetAnnotationsContext();
                }

                return context;
            }
            protected internal set
            {
                context = value;
            }
        }

        public EntityService EntityService => AnnotationsContext?.EntityService;

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
