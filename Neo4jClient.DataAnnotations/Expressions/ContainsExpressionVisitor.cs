using System.Linq.Expressions;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class ContainsExpressionVisitor : ExpressionVisitor
    {
        public Expression Item { get; set; }

        public Expression Container { get; private set; }

        public bool IsContained { get; private set; }

        public void Reset()
        {
            Item = null;
            Container = null;
            IsContained = false;
        }

        public override Expression Visit(Expression node)
        {
            if (node != null && node == Item)
            {
                IsContained = true;
                return node;
            }

            node = base.Visit(node);

            if (IsContained && Container == null)
                Container = node;

            return node;
        }
    }
}