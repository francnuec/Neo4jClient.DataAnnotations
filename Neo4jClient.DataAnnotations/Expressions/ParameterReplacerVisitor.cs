using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class ParameterReplacerVisitor : ExpressionVisitor
    {
        Dictionary<string, Expression> parameterReplacements;

        public ParameterReplacerVisitor(Dictionary<string, Expression> parameterReplacements)
        {
            this.parameterReplacements = parameterReplacements;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (parameterReplacements != null 
                && parameterReplacements.TryGetValue(node.Name, out var replacement))
            {
                return replacement;
            }

            return base.VisitParameter(node);
        }

        //protected override Expression VisitMember(MemberExpression node)
        //{
        //    var newNode = base.VisitMember(node);

        //    if (newNode != node
        //        && newNode is MemberExpression newMemberExpr
        //        && newMemberExpr.Expression != node.Expression
        //        && parameterReplacements.FirstOrDefault(p => p.Value == newMemberExpr.Expression 
        //        && p.Value.Type != newMemberExpr.Type) is var replacementPair
        //        && replacementPair.Value != null)
        //    {
        //        //replace the type on the new member expr
        //    }

        //    return newNode;
        //}

    }
}
