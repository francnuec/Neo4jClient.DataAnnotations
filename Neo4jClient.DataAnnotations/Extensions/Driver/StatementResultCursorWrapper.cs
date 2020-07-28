using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClient.DataAnnotations.Extensions.Driver
{
    public class StatementResultCursorWrapper : BaseWrapper<IStatementResultCursor>, IStatementResultCursor
    {
        public StatementResultCursorWrapper(IStatementResultCursor statementResultCursor) : base(statementResultCursor) { }

        public IReadOnlyList<string> Keys => WrappedItem.Keys;

        public IRecord Current => StatementResultWrapper.GetRecord(WrappedItem.Current);

        public Task<IResultSummary> ConsumeAsync()
        {
            return WrappedItem.ConsumeAsync();
        }

        public Task<bool> FetchAsync()
        {
            return WrappedItem.FetchAsync();
        }

        public async Task<IRecord> PeekAsync()
        {
            return StatementResultWrapper.GetRecord(await WrappedItem.PeekAsync());
        }

        public Task<IResultSummary> SummaryAsync()
        {
            return WrappedItem.SummaryAsync();
        }
    }
}
