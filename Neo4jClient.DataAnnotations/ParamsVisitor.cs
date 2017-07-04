using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Neo4jClient.DataAnnotations
{
    public class ParamsVisitor : ExpressionVisitor
    {
        public List<List<Expression>> Params { get; } = new List<List<Expression>>();

        List<object> currentPath = new List<object>();

        public List<List<object>> ParamsPaths { get; } = new List<List<object>>();

        public override Expression Visit(Expression node)
        {
            List<Expression> filtered = null;

            if ((filtered = Utilities.GetValidSimpleAccessStretch(node)) != null
                && Utilities.HasParams(filtered))
            {
                //found our params call.
                //store and replace with marker
                Params.Add(filtered);

                var expr = Expression.Call(typeof(Utilities), "GetParams", new[] { node.Type }, Expression.Constant(Params.Count - 1));

                ParamsPaths.Add(new List<object>() { expr });

                return expr;
            }

            return base.Visit(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to

            var expr = base.VisitNew(node);

            if (expr.Type.IsAnonymousType())
            {
                for (int i = index, l = ParamsPaths.Count; i < l; i++)
                {
                    ParamsPaths[i].Add(expr);
                }
            }

            return expr;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var expr = base.VisitNewArray(node);
            for (int i = index, l = ParamsPaths.Count; i < l; i++)
            {
                ParamsPaths[i].Add(expr);
            }
            return expr;
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var expr = base.VisitListInit(node);
            for (int i = index, l = ParamsPaths.Count; i < l; i++)
            {
                ParamsPaths[i].Add(expr);
            }
            return expr;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var expr = base.VisitMemberInit(node);
            for (int i = index, l = ParamsPaths.Count; i < l; i++)
            {
                ParamsPaths[i].Add(expr);
            }
            return expr;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var expr = base.VisitMemberAssignment(node);
            for (int i = index, l = ParamsPaths.Count; i < l; i++)
            {
                ParamsPaths[i].Add(expr);
            }
            return expr;
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var expr = base.VisitMemberListBinding(node);
            for (int i = index, l = ParamsPaths.Count; i < l; i++)
            {
                ParamsPaths[i].Add(expr);
            }
            return expr;
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var expr = base.VisitMemberMemberBinding(node);
            for (int i = index, l = ParamsPaths.Count; i < l; i++)
            {
                ParamsPaths[i].Add(expr);
            }
            return expr;
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var expr = base.VisitElementInit(node);
            for (int i = index, l = ParamsPaths.Count; i < l; i++)
            {
                ParamsPaths[i].Add(expr);
            }
            return expr;
        }
    }
}
