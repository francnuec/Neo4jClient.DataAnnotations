using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.DataAnnotations.Extensions.Driver
{
    public class AsyncTransactionWrapper : BaseWrapper<IAsyncTransaction>, IAsyncTransaction
    {
        public TransactionConfig TransactionConfig => WrappedItem.TransactionConfig;

        public AsyncTransactionWrapper(IAsyncTransaction item) : base(item)
        {
        }

        public async Task<IResultCursor> RunAsync(string query)
        {
            return SessionWrapper.GetResultCursor(await WrappedItem.RunAsync(query));
        }

        public async Task<IResultCursor> RunAsync(string query, object parameters)
        {
            return await RunAsync(new Query(query, parameters));
        }

        public async Task<IResultCursor> RunAsync(string query, IDictionary<string, object> parameters)
        {
            return await RunAsync(new Query(query, parameters));
        }

        public async Task<IResultCursor> RunAsync(Query query)
        {
            if (query?.Parameters?.Count > 0)
            {
                var parameters = query.Parameters;
                var modifications = new Dictionary<string, object>();

                foreach (var entry in parameters.Keys)
                {
                    if (parameters[entry] is JObject valueJObject)
                    {
                        //we need to convert this to dictionary so it can be understood by the driver.
                        //otherwise, the driver would assume the JObject is a List,
                        //because JObject inherits from JContainer which in turn inherits from IList<JToken>.
                        modifications[entry] = valueJObject.ToObject<Dictionary<string, object>>();
                    }
                }

                foreach (var entry in modifications)
                {
                    parameters[entry.Key] = entry.Value;
                }
            }

            return SessionWrapper.GetResultCursor(await WrappedItem.RunAsync(query));
        }

        public Task CommitAsync()
        {
            return WrappedItem.CommitAsync();
        }

        public Task RollbackAsync()
        {
            return WrappedItem.RollbackAsync();
        }
    }
}