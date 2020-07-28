using System.Collections.Generic;
using System.Linq.Expressions;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class ReplacerExpressionVisitor : ExpressionVisitor
    {
        public ReplacerExpressionVisitor()
        {
            ExpressionReplacements = new Dictionary<Expression, Expression>();
        }

        public ReplacerExpressionVisitor(Dictionary<Expression, Expression> exprToReplacement)
        {
            ExpressionReplacements = exprToReplacement;
        }

        public Dictionary<Expression, Expression> ExpressionReplacements { get; }

        public override Expression Visit(Expression node)
        {
            if (node != null && ExpressionReplacements.TryGetValue(node, out var newNode)) return newNode;

            return base.Visit(node);
        }
    }
}