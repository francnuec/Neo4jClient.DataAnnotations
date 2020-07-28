using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class FunctionsVisitorBuilder
    {
        public FunctionsVisitorBuilder(FunctionExpressionVisitor visitor)
        {
            Visitor = visitor ?? throw new ArgumentNullException(nameof(visitor));
        }

        protected StringBuilder ActualBuilder { get; } = new StringBuilder();

        public List<Monitor> Monitors { get; } = new List<Monitor>();

        public FunctionExpressionVisitor Visitor { get; }

        private IEnumerable<Monitor> GetMonitorsOfInterest(Expression node)
        {
            //bool foundTargetNode = false;
            //return Monitors.TakeWhile(m =>
            //{
            //    if (foundTargetNode)
            //    {
            //        return false;
            //    }

            //    return m.Node != node || (foundTargetNode = true);
            //});

            foreach (var m in Monitors)
                if (m.Node != node)
                {
                    yield return m;
                }
                else
                {
                    yield return m;
                    yield break;
                }
        }

        public void Append(dynamic value, Expression callerNode)
        {
            var targetMonitors = GetMonitorsOfInterest(callerNode);

            foreach (var m in targetMonitors) m.WillAppend?.Invoke(this, callerNode, value);

            if (value is null)
                ActualBuilder.Append((object)null);
            else
                ActualBuilder.Append(value);


            foreach (var m in targetMonitors) m.DidAppend?.Invoke(this, callerNode, value);
        }

        public void Clear()
        {
            Monitors.ForEach(m => m.WillClearAll?.Invoke(this));
            ActualBuilder.Clear();
            Monitors.ForEach(m => m.DidClearAll?.Invoke(this));
        }

        public override string ToString()
        {
            return ActualBuilder.ToString();
        }

        public class Monitor
        {
            public Expression Node { get; set; }

            public Action<FunctionsVisitorBuilder, Expression, dynamic> WillAppend { get; set; }
            public Action<FunctionsVisitorBuilder, Expression, dynamic> DidAppend { get; set; }

            public Action<FunctionsVisitorBuilder> WillClearAll { get; set; }
            public Action<FunctionsVisitorBuilder> DidClearAll { get; set; }
        }
    }
}