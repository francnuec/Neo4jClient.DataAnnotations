using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class ReplacerExpressionVisitor : ExpressionVisitor
    {
        Dictionary<Expression, Expression> exprToReplacement;

        public ReplacerExpressionVisitor(Dictionary<Expression, Expression> exprToReplacement)
        {
            this.exprToReplacement = exprToReplacement;
        }

        public override Expression Visit(Expression node)
        {
            if (exprToReplacement.TryGetValue(node, out var newNode))
            {
                return newNode;
            }

            return base.Visit(node);
        }
    }
}
