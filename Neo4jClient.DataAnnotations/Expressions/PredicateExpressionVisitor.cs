using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class PredicateExpressionVisitor : ExpressionVisitor
    {
        private ReplacerExpressionVisitor replacerVisitor;

        public Dictionary<Expression, Expression> Assignments { get; }
            = new Dictionary<Expression, Expression>();

        public Dictionary<Expression, Expression> OriginalAssignments { get; }
            = new Dictionary<Expression, Expression>();

        public Dictionary<Expression, Expression> RightNodeReplacements { get; set; } 
            = new Dictionary<Expression, Expression>();

        public bool MakeRightNodeReplacements { get; set; } = true;

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.Equal)
            {
                var rightNode = node.Right;

                if (MakeRightNodeReplacements && RightNodeReplacements?.Count > 0)
                {
                    //replace all the parameter calls on the right with a vars get call instead
                    if (replacerVisitor == null)
                    {
                        replacerVisitor = new ReplacerExpressionVisitor(RightNodeReplacements);
                    }

                    rightNode = replacerVisitor.Visit(rightNode);
                }
                
                Assignments[node.Left] = rightNode;
                OriginalAssignments[node.Left] = node.Right;

                return Expression.Constant(true);
            }

            return base.VisitBinary(node);
        }

        public void Clear()
        {
            Assignments.Clear();
            OriginalAssignments.Clear();
            replacerVisitor = null;
            RightNodeReplacements?.Clear();
        }
    }
}
