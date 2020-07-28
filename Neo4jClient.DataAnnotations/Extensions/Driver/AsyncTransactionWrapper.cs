using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neo4jClient.Transactions;

namespace Neo4jClient.DataAnnotations.Extensions.Driver
{
    public class AsyncTransactionWrapper : BaseWrapper<IAsyncTransaction>, IAsyncTransaction
    {




        //public Task CommitAsync()
        //{
        //    return WrappedItem.CommitAsync();
        //}

        //public void Dispose()
        //{
        //    WrappedItem.Dispose();
        //}

        //public void Failure()
        //{
        //    WrappedItem.Failure();
        //}

        //public Task RollbackAsync()
        //{
        //    return WrappedItem.RollbackAsync();
        //}

        //public IStatementResult Run(string statement)
        //{
        //    return SessionWrapper.GetStatementResult(WrappedItem.Run(statement));
        //}

        //public IStatementResult Run(string statement, object parameters)
        //{
        //    return SessionWrapper.GetStatementResult(WrappedItem.Run(statement, parameters));
        //}

        //public IStatementResult Run(string statement, IDictionary<string, object> parameters)
        //{
        //    return SessionWrapper.GetStatementResult(WrappedItem.Run(statement, parameters));
        //}

        //public IStatementResult Run(Statement statement)
        //{
        //    return SessionWrapper.GetStatementResult(WrappedItem.Run(statement));
        //}

        //public async Task<IStatementResultCursor> RunAsync(string statement)
        //{
        //    return SessionWrapper.GetStatementResultCursor(await WrappedItem.RunAsync(statement));
        //}

        //public async Task<IStatementResultCursor> RunAsync(string statement, object parameters)
        //{
        //    return SessionWrapper.GetStatementResultCursor(await WrappedItem.RunAsync(statement, parameters));
        //}

        //public async Task<IStatementResultCursor> RunAsync(string statement, IDictionary<string, object> parameters)
        //{
        //    return SessionWrapper.GetStatementResultCursor(await WrappedItem.RunAsync(statement, parameters));
        //}

        //public async Task<IStatementResultCursor> RunAsync(Statement statement)
        //{
        //    return SessionWrapper.GetStatementResultCursor(await WrappedItem.RunAsync(statement));
        //}

        //public void Success()
        //{
        //    WrappedItem.Success();
        //}
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

        public TransactionConfig TransactionConfig => WrappedItem.TransactionConfig;
    }
}
