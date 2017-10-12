using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

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
    }
}
