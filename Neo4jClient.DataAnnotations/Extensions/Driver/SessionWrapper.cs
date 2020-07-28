﻿using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neo4jClient.Transactions;

namespace Neo4jClient.DataAnnotations.Extensions.Driver
{
    public class SessionWrapper : BaseWrapper<IAsyncSession>, IAsyncSession
    {
        public SessionWrapper(IAsyncSession session) : base(session) { }
        //TODO;REMOVE COMMENTED CODE
        //        public string LastBookmark => WrappedItem.LastBookmark;

        //        public ITransaction BeginTransaction()
        //        {
        //            return WrappedItem.BeginTransaction();
        //        }

        //        public ITransaction BeginTransaction(TransactionConfig txConfig)
        //        {
        //            return WrappedItem.BeginTransaction(txConfig);
        //        }

        //        public ITransaction BeginTransaction(string bookmark)
        //        {
        //            return WrappedItem.BeginTransaction(bookmark);
        //        }

        //        public Task<ITransaction> BeginTransactionAsync()
        //        {
        //            return WrappedItem.BeginTransactionAsync();
        //        }

        //        public Task<ITransaction> BeginTransactionAsync(TransactionConfig txConfig)
        //        {
        //            return WrappedItem.BeginTransactionAsync(txConfig);
        //        }

        //        public Task CloseAsync()
        //        {
        //            return WrappedItem.CloseAsync();
        //        }

        //        public void Dispose()
        //        {
        //            WrappedItem.Dispose();
        //        }

        //        public T ReadTransaction<T>(Func<ITransaction, T> work)
        //        {
        //            return WrappedItem.ReadTransaction<T>(tx => work(GetTransaction(tx)));
        //        }

        //        public void ReadTransaction(Action<ITransaction> work)
        //        {
        //            WrappedItem.ReadTransaction(tx => work(GetTransaction(tx)));
        //        }

        //        public T ReadTransaction<T>(Func<ITransaction, T> work, TransactionConfig txConfig)
        //        {
        //            return WrappedItem.ReadTransaction<T>(tx => work(GetTransaction(tx)), txConfig);
        //        }

        //        public void ReadTransaction(Action<ITransaction> work, TransactionConfig txConfig)
        //        {
        //            WrappedItem.ReadTransaction(tx => work(GetTransaction(tx)), txConfig);
        //        }

        //        public Task<T> ReadTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        //        {
        //            return WrappedItem.ReadTransactionAsync<T>(tx => work(GetTransaction(tx)));
        //        }

        //        public Task ReadTransactionAsync(Func<ITransaction, Task> work)
        //        {
        //            return WrappedItem.ReadTransactionAsync(tx => work(GetTransaction(tx)));
        //        }

        //        public Task<T> ReadTransactionAsync<T>(Func<ITransaction, Task<T>> work, TransactionConfig txConfig)
        //        {
        //            return WrappedItem.ReadTransactionAsync<T>(tx => work(GetTransaction(tx)), txConfig);
        //        }

        //        public Task ReadTransactionAsync(Func<ITransaction, Task> work, TransactionConfig txConfig)
        //        {
        //            return WrappedItem.ReadTransactionAsync(tx => work(GetTransaction(tx)), txConfig);
        //        }

        //        public IStatementResult Run(string statement, TransactionConfig txConfig)
        //        {
        //            return GetStatementResult(WrappedItem.Run(statement, txConfig));
        //        }

        //        public IStatementResult Run(string statement, IDictionary<string, object> parameters, TransactionConfig txConfig)
        //        {
        //            return GetStatementResult(WrappedItem.Run(statement, parameters, txConfig));
        //        }

        //        public IStatementResult Run(Statement statement, TransactionConfig txConfig)
        //        {
        //            return GetStatementResult(WrappedItem.Run(statement, txConfig));
        //        }

        //        public IStatementResult Run(string statement)
        //        {
        //            return GetStatementResult(WrappedItem.Run(statement));
        //        }

        //        public IStatementResult Run(string statement, object parameters)
        //        {
        //            return GetStatementResult(WrappedItem.Run(statement, parameters));
        //        }

        //        public IStatementResult Run(string statement, IDictionary<string, object> parameters)
        //        {
        //            return GetStatementResult(WrappedItem.Run(statement, parameters));
        //        }

        //        public IStatementResult Run(Statement statement)
        //        {
        //            return GetStatementResult(WrappedItem.Run(statement));
        //        }

        //        public async Task<IStatementResultCursor> RunAsync(string statement, TransactionConfig txConfig)
        //        {
        //            return GetStatementResultCursor(await WrappedItem.RunAsync(statement, txConfig));
        //        }

        //        public async Task<IStatementResultCursor> RunAsync(string statement, IDictionary<string, object> parameters, TransactionConfig txConfig)
        //        {
        //            return GetStatementResultCursor(await WrappedItem.RunAsync(statement, parameters, txConfig));
        //        }

        //        public async Task<IStatementResultCursor> RunAsync(Statement statement, TransactionConfig txConfig)
        //        {
        //            return GetStatementResultCursor(await WrappedItem.RunAsync(statement, txConfig));
        //        }

        //        public async Task<IStatementResultCursor> RunAsync(string statement)
        //        {
        //            return GetStatementResultCursor(await WrappedItem.RunAsync(statement));
        //        }

        //        public async Task<IStatementResultCursor> RunAsync(string statement, object parameters)
        //        {
        //            return GetStatementResultCursor(await WrappedItem.RunAsync(statement, parameters));
        //        }

        //        public async Task<IStatementResultCursor> RunAsync(string statement, IDictionary<string, object> parameters)
        //        {
        //            return GetStatementResultCursor(await WrappedItem.RunAsync(statement, parameters));
        //        }

        //        public async Task<IStatementResultCursor> RunAsync(Statement statement)
        //        {
        //            return GetStatementResultCursor(await WrappedItem.RunAsync(statement));
        //        }

        //        public T WriteTransaction<T>(Func<ITransaction, T> work)
        //        {
        //            return WrappedItem.WriteTransaction<T>(tx => work(GetTransaction(tx)));
        //        }

        //        public void WriteTransaction(Action<ITransaction> work)
        //        {
        //            WrappedItem.WriteTransaction(tx => work(GetTransaction(tx)));
        //        }

        //        public T WriteTransaction<T>(Func<ITransaction, T> work, TransactionConfig txConfig)
        //        {
        //            return WrappedItem.WriteTransaction<T>(tx => work(GetTransaction(tx)), txConfig);
        //        }

        //        public void WriteTransaction(Action<ITransaction> work, TransactionConfig txConfig)
        //        {
        //            WrappedItem.WriteTransaction(tx => work(GetTransaction(tx)), txConfig);
        //        }

        //        public Task<T> WriteTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        //        {
        //            return WrappedItem.WriteTransactionAsync<T>(tx => work(GetTransaction(tx)));
        //        }

        //        public Task WriteTransactionAsync(Func<ITransaction, Task> work)
        //        {
        //            return WrappedItem.WriteTransactionAsync(tx => work(GetTransaction(tx)))
        //;
        //        }

        //        public Task<T> WriteTransactionAsync<T>(Func<ITransaction, Task<T>> work, TransactionConfig txConfig)
        //        {
        //            return WrappedItem.WriteTransactionAsync<T>(tx => work(GetTransaction(tx)), txConfig);
        //        }

        //        public Task WriteTransactionAsync(Func<ITransaction, Task> work, TransactionConfig txConfig)
        //        {
        //            return WrappedItem.WriteTransactionAsync(tx => work(GetTransaction(tx)), txConfig);
        //        }

        //protected internal static IStatementResult GetStatementResult(IStatementResult statementResult)
        //{
        //    if (statementResult != null && !(statementResult is StatementResultWrapper))
        //    {
        //        return new StatementResultWrapper(statementResult);
        //    }

        //    return statementResult;
        //}

        protected internal static IResultCursor GetResultCursor(IResultCursor resultCursor)
        {
            if (resultCursor != null && !(resultCursor is ResultCursorWrapper))
            {
                return new ResultCursorWrapper(resultCursor);
            }

            return resultCursor;
        }

        protected internal static IAsyncTransaction GetAsyncTransaction(IAsyncTransaction transaction)
        {
            if (transaction != null && !(transaction is AsyncTransactionWrapper))
            {
                return new AsyncTransactionWrapper(transaction);
            }

            return transaction;
        }

        public async Task<IResultCursor> RunAsync(string query)
        {
            return GetResultCursor(await WrappedItem.RunAsync(query));
        }

        public async Task<IResultCursor> RunAsync(string query, object parameters)
        {
            return GetResultCursor(await WrappedItem.RunAsync(query, parameters));
        }

        public async Task<IResultCursor> RunAsync(string query, IDictionary<string, object> parameters)
        {
            return GetResultCursor(await WrappedItem.RunAsync(query, parameters));
        }

        public async Task<IResultCursor> RunAsync(Query query)
        {
            return GetResultCursor(await WrappedItem.RunAsync(query));
        }

        public Task<IAsyncTransaction> BeginTransactionAsync()
        {
            return WrappedItem.BeginTransactionAsync();
        }

        public Task<IAsyncTransaction> BeginTransactionAsync(Action<TransactionConfigBuilder> action)
        {
            return WrappedItem.BeginTransactionAsync(action);
        }

        public Task<T> ReadTransactionAsync<T>(Func<IAsyncTransaction, Task<T>> work)
        {
            return WrappedItem.ReadTransactionAsync(tx => work(GetAsyncTransaction(tx)));
        }

        public Task ReadTransactionAsync(Func<IAsyncTransaction, Task> work)
        {
            return WrappedItem.ReadTransactionAsync(tx => work(GetAsyncTransaction(tx)));
        }

        public Task<T> ReadTransactionAsync<T>(Func<IAsyncTransaction, Task<T>> work, Action<TransactionConfigBuilder> action)
        {
            return WrappedItem.ReadTransactionAsync(tx => work(GetAsyncTransaction(tx)), action);
        }

        public Task ReadTransactionAsync(Func<IAsyncTransaction, Task> work, Action<TransactionConfigBuilder> action)
        {
            return WrappedItem.ReadTransactionAsync(tx => work(GetAsyncTransaction(tx)), action);
        }

        public Task<T> WriteTransactionAsync<T>(Func<IAsyncTransaction, Task<T>> work)
        {
            return WrappedItem.WriteTransactionAsync(tx => work(GetAsyncTransaction(tx)));
        }

        public Task WriteTransactionAsync(Func<IAsyncTransaction, Task> work)
        {
            return WrappedItem.WriteTransactionAsync(tx => work(GetAsyncTransaction(tx)));
        }

        public Task<T> WriteTransactionAsync<T>(Func<IAsyncTransaction, Task<T>> work, Action<TransactionConfigBuilder> action)
        {
            return WrappedItem.WriteTransactionAsync(tx => work(GetAsyncTransaction(tx)), action);
        }

        public Task WriteTransactionAsync(Func<IAsyncTransaction, Task> work, Action<TransactionConfigBuilder> action)
        {
            return WrappedItem.WriteTransactionAsync(tx => work(GetAsyncTransaction(tx)), action);
        }

        public Task CloseAsync()
        {
            return WrappedItem.CloseAsync();
        }

        public async Task<IResultCursor> RunAsync(string query, Action<TransactionConfigBuilder> action)
        {
            return GetResultCursor(await WrappedItem.RunAsync(query, action));
        }

        public async Task<IResultCursor> RunAsync(string query, IDictionary<string, object> parameters, Action<TransactionConfigBuilder> action)
        {
            return GetResultCursor(await WrappedItem.RunAsync(query, parameters, action));
        }

        public async Task<IResultCursor> RunAsync(Query query, Action<TransactionConfigBuilder> action)
        {
            return GetResultCursor(await WrappedItem.RunAsync(query, action));
        }

        public Bookmark LastBookmark => WrappedItem.LastBookmark;
        public SessionConfig SessionConfig => WrappedItem.SessionConfig;
    }
}
