using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClient.DataAnnotations.Extensions.Driver
{
    public class TransactionWrapper : BaseWrapper<ITransaction>, ITransaction
    {
        public TransactionWrapper(ITransaction transaction) : base(transaction) { }

        public Task CommitAsync()
        {
            return WrappedItem.CommitAsync();
        }

        public void Dispose()
        {
            WrappedItem.Dispose();
        }

        public void Failure()
        {
            WrappedItem.Failure();
        }

        public Task RollbackAsync()
        {
            return WrappedItem.RollbackAsync();
        }

        public IStatementResult Run(string statement)
        {
            return SessionWrapper.GetStatementResult(WrappedItem.Run(statement));
        }

        public IStatementResult Run(string statement, object parameters)
        {
            return SessionWrapper.GetStatementResult(WrappedItem.Run(statement, parameters));
        }

        public IStatementResult Run(string statement, IDictionary<string, object> parameters)
        {
            return SessionWrapper.GetStatementResult(WrappedItem.Run(statement, parameters));
        }

        public IStatementResult Run(Statement statement)
        {
            return SessionWrapper.GetStatementResult(WrappedItem.Run(statement));
        }

        public async Task<IStatementResultCursor> RunAsync(string statement)
        {
            return SessionWrapper.GetStatementResultCursor(await WrappedItem.RunAsync(statement));
        }

        public async Task<IStatementResultCursor> RunAsync(string statement, object parameters)
        {
            return SessionWrapper.GetStatementResultCursor(await WrappedItem.RunAsync(statement, parameters));
        }

        public async Task<IStatementResultCursor> RunAsync(string statement, IDictionary<string, object> parameters)
        {
            return SessionWrapper.GetStatementResultCursor(await WrappedItem.RunAsync(statement, parameters));
        }

        public async Task<IStatementResultCursor> RunAsync(Statement statement)
        {
            return SessionWrapper.GetStatementResultCursor(await WrappedItem.RunAsync(statement));
        }

        public void Success()
        {
            WrappedItem.Success();
        }
    }
}
