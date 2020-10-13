using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Utils;
using Neo4jClient.Serialization;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public class QueryContext : IHaveAnnotationsContext, IHaveEntityService
    {
        private AnnotationsContext annotationsContext;

        private ConcurrentDictionary<Expression, (Expression NewNode, string Build)> funcsCachedBuilds;

        private ConcurrentDictionary<Expression, JToken> funcsCachedJTokens;
        //public ICypherFluentQuery Query { get; set; }

        public IGraphClient Client { get; set; }
        public ISerializer ISerializer { get; set; }
        public EntityResolver Resolver => AnnotationsContext.EntityResolver; //{ get; set; }
        public EntityConverter Converter => AnnotationsContext.EntityConverter; //{ get; set; }
        public Func<object, string> SerializeCallback { get; set; }

        /// <summary>
        ///     Note that this query writer is useless against subsequent <see cref="ICypherFluentQuery" /> calls when the query
        ///     object changes
        /// </summary>
        public QueryWriterWrapper CurrentQueryWriter { get; set; }

        /// <summary>
        ///     Note that this build strategy might be is useless against subsequent <see cref="ICypherFluentQuery" /> calls when
        ///     the query object changes
        /// </summary>
        public PropertiesBuildStrategy? CurrentBuildStrategy { get; set; }

        public static Func<ICypherFluentQuery, QueryWriterWrapper> QueryWriterGetter { get; } = q =>
        {
            var qw = Defaults.QueryWriterInfo.GetValue(q) as QueryWriter;
            return qw != null ? new QueryWriterWrapper(qw, q.GetAnnotationsContext()) : null;
        };

        public static Func<ICypherFluentQuery, PropertiesBuildStrategy?> BuildStrategyGetter { get; } = q =>
        {
            var qw = Defaults.QueryWriterInfo.GetValue(q) as QueryWriter;
            var parameters = Defaults.QueryWriterParamsInfo.GetValue(qw) as IDictionary<string, object>;

            if (parameters != null && parameters.TryGetValue(Defaults.QueryBuildStrategyKey, out var buildStrategy))
                return buildStrategy as PropertiesBuildStrategy?;

            return null;
        };

        public ConcurrentDictionary<Expression, (Expression NewNode, string Build)> FuncsCachedBuilds =>
            funcsCachedBuilds ?? (funcsCachedBuilds =
                new ConcurrentDictionary<Expression, (Expression NewNode, string Build)>());

        public ConcurrentDictionary<Expression, JToken> FuncsCachedJTokens =>
            funcsCachedJTokens ?? (funcsCachedJTokens = new ConcurrentDictionary<Expression, JToken>());

        public AnnotationsContext AnnotationsContext
        {
            get
            {
                if (annotationsContext == null)
                    annotationsContext = Client?.GetAnnotationsContext()
                                         ?? Resolver?.AnnotationsContext
                                         ?? Converter?.AnnotationsContext;
                return annotationsContext;
            }
            set => annotationsContext = value;
        }

        public EntityService EntityService => AnnotationsContext?.EntityService;
    }
}