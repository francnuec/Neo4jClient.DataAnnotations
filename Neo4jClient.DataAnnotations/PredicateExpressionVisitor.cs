using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.DataAnnotations
{
    public class PredicateExpressionVisitor : ExpressionVisitor
    {
        public Dictionary<Expression, Expression> Assignments 
            = new Dictionary<Expression, Expression>();

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.Equal)
            {
                Assignments[node.Left] = node.Right;

                return Expression.Constant(true);
            }

            return base.VisitBinary(node);
        }
    }
}
