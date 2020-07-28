using System;
using Neo4jClient.Cypher;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public abstract class Annotated : IAnnotated, IHaveAnnotationsContext, IHaveEntityService //, ICypherFluentQuery
    {
        private AnnotationsContext context;
        private ICypherFluentQuery internalCypherQuery;

        public Annotated(ICypherFluentQuery query)
        {
            internalCypherQuery = query ?? throw new ArgumentNullException(nameof(query));
        }

        public Annotated(ICypherFluentQuery query, AnnotationsContext context = null) : this(query)
        {
            AnnotationsContext = context;
        }

        public virtual ref ICypherFluentQuery CypherQuery => ref internalCypherQuery;

        public AnnotationsContext AnnotationsContext
        {
            get
            {
                if (context == null) context = CypherQuery.GetAnnotationsContext();

                return context;
            }
            protected internal set => context = value;
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