using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class ReplacerExpressionVisitor : ExpressionVisitor
    {
        public Dictionary<Expression, Expression> ExpressionReplacements { get; }

        public ReplacerExpressionVisitor()
        {
            ExpressionReplacements = new Dictionary<Expression, Expression>();
        }

        public ReplacerExpressionVisitor(Dictionary<Expression, Expression> exprToReplacement)
        {
            ExpressionReplacements = exprToReplacement;
        }

        public override Expression Visit(Expression node)
        {
            if (node != null && ExpressionReplacements.TryGetValue(node, out var newNode))
            {
                return newNode;
            }

            return base.Visit(node);
        }
    }
}
