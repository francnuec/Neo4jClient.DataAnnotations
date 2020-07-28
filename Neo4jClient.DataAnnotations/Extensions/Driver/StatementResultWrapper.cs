using Neo4j.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations.Extensions.Driver
{
    //public class StatementResultWrapper : BaseWrapper<Result>, IStatementResult
    //{
    //    public StatementResultWrapper(IStatementResult statementResult) : base(statementResult) { }

    //    public IReadOnlyList<string> Keys => WrappedItem.Keys;

    //    public IResultSummary Summary => WrappedItem.Summary;

    //    public IResultSummary Consume()
    //    {
    //        return WrappedItem.Consume();
    //    }

    //    public IEnumerator<IRecord> GetEnumerator()
    //    {
    //        return new EnumeratorWrapper(WrappedItem.GetEnumerator());
    //    }

    //    public IRecord Peek()
    //    {
    //        return GetRecord(WrappedItem.Peek());
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return GetEnumerator();
    //    }


    //    public class EnumeratorWrapper : IEnumerator<IRecord>
    //    {
    //        public EnumeratorWrapper(IEnumerator<IRecord> enumerator)
    //        {
    //            Enumerator = enumerator;
    //        }

    //        public IEnumerator<IRecord> Enumerator { get; }

    //        public IRecord Current => GetRecord(Enumerator.Current);

    //        object IEnumerator.Current => Current;

    //        public void Dispose()
    //        {
    //            Enumerator.Dispose();
    //        }

    //        public bool MoveNext()
    //        {
    //            return Enumerator.MoveNext();
    //        }

    //        public void Reset()
    //        {
    //            Enumerator.Reset();
    //        }
    //    }
    //}
}
