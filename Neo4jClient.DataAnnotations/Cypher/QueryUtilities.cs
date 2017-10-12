using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public class QueryUtilities
    {
        //public ICypherFluentQuery Query { get; set; }

        public IGraphClient Client { get; set; }
        public ISerializer ISerializer { get; set; }
        public EntityResolver Resolver { get; set; }
        public EntityConverter Converter { get; set; }
        public Func<object, string> SerializeCallback { get; set; }

        /// <summary>
        /// Note that this query writer is useless against subsequent <see cref="ICypherFluentQuery"/> calls when the query object changes
        /// </summary>
        public QueryWriter CurrentQueryWriter { get; set; }
        /// <summary>
        /// Note that this build strategy might be is useless against subsequent <see cref="ICypherFluentQuery"/> calls when the query object changes
        /// </summary>
        public PropertiesBuildStrategy? CurrentBuildStrategy { get; set; }

        public static Func<ICypherFluentQuery, QueryWriter> QueryWriterGetter { get; } = (q) =>
        {
            QueryWriter qw = Defaults.QueryWriterInfo.GetValue(q) as QueryWriter;
            return qw;
        };

        public static Func<ICypherFluentQuery, PropertiesBuildStrategy?> BuildStrategyGetter { get; } = (q) =>
        {
            QueryWriter qw = Defaults.QueryWriterInfo.GetValue(q) as QueryWriter;
            var parameters = Defaults.QueryWriterParamsInfo.GetValue(qw) as IDictionary<string, object>;

            if (parameters != null && parameters.TryGetValue(Defaults.QueryBuildStrategyKey, out var buildStrategy))
                return buildStrategy as PropertiesBuildStrategy?;

            return null;
        };
    }
}
