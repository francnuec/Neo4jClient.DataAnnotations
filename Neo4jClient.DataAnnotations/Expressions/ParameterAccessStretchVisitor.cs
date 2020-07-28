using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class ParameterAccessStretchVisitor : ExpressionVisitor, IHaveEntityService
    {
        private readonly List<Expression> seenExpressions = new List<Expression>();

        public ParameterAccessStretchVisitor(EntityService entityService)
        {
            EntityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        }

        public Dictionary<Expression, ParameterExpression> ParameterAccesses { get; }
            = new Dictionary<Expression, ParameterExpression>();

        public EntityService EntityService { get; }

        public override Expression Visit(Expression node)
        {
            if (!seenExpressions.Contains(node))
            {
                //we need to check if there's a parameter at the root of this and store the expression
                var exprs = ExpressionUtilities.GetSimpleMemberAccessStretch(EntityService, node, out var entityExpr);
                if (exprs?.Count > 0 && (entityExpr = exprs[0]) != null)
                {
                    seenExpressions.AddRange(exprs);

                    var parameterExpr = entityExpr as ParameterExpression;

                    if (parameterExpr != null)
                        ParameterAccesses.Add(node, parameterExpr);
                }
            }

            return base.Visit(node);
        }
    }
}