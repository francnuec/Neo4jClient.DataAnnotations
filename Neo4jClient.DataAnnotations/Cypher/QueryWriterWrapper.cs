using System;
using System.Collections.Generic;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public class QueryWriterWrapper : IHaveAnnotationsContext
    {
        public QueryWriterWrapper(QueryWriter queryWriter, AnnotationsContext annotationsContext)
        {
            QueryWriter = queryWriter ?? throw new ArgumentNullException(nameof(queryWriter));
            AnnotationsContext = annotationsContext ?? throw new ArgumentNullException(nameof(annotationsContext));
            IsBoltClient = annotationsContext.IsBoltClient;
        }

        public QueryWriter QueryWriter { get; protected set; }

        public bool IsBoltClient { get; }

        public AnnotationsContext AnnotationsContext { get; }

        public EntityService EntityService => AnnotationsContext?.EntityService;

        public virtual object GetTransformedParameterValue(object value)
        {
            if (IsBoltClient && value is JObject valueJObject)
                //because bolt client uses the Neo4j driver directly, 
                //we need to convert this to dictionary so it can be understood by the driver.
                value = valueJObject.ToObject<Dictionary<string, object>>();

            return value;
        }

        public string CreateParameter(object paramValue)
        {
            return QueryWriter.CreateParameter(GetTransformedParameterValue(paramValue));
        }

        public void CreateParameter(string key, object value)
        {
            QueryWriter.CreateParameter(key, GetTransformedParameterValue(value));
        }

        public void CreateParameters(IDictionary<string, object> parameters)
        {
            foreach (var param in parameters) QueryWriter.CreateParameter(param.Key, param.Value);
        }

        public bool ContainsParameterWithKey(string key)
        {
            return QueryWriter.ContainsParameterWithKey(key);
        }

        public CypherQuery ToCypherQuery(IContractResolver contractResolver = null, bool isWrite = true)
        {
            return QueryWriter.ToCypherQuery(contractResolver, isWrite);
        }
    }
}