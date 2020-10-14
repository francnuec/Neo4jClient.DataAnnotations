using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

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
            return SessionWrapper.GetResultCursor(await WrappedItem.RunAsync(query, parameters));
        }

        public async Task<IResultCursor> RunAsync(string query, IDictionary<string, object> parameters)
        {
            return SessionWrapper.GetResultCursor(await WrappedItem.RunAsync(query, parameters));
        }

        public async Task<IResultCursor> RunAsync(Query query)
        {
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