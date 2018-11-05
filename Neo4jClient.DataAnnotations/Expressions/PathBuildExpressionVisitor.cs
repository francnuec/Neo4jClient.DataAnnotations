//using System;
using Neo4jClient.DataAnnotations.Utils;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Text;
//using System.Linq;

//namespace Neo4jClient.DataAnnotations.Expressions
//{
//    public class PathBuildExpressionVisitor : ExpressionVisitor
//    {
//        List<Expression> seenExpressions = new List<Expression>();
//        List<object> objectsToConsider = null;

//        public PathBuildExpressionVisitor(List<object> objectsToConsider)
//        {
//            this.objectsToConsider = objectsToConsider;
//        }

//        protected override Expression VisitMember(MemberExpression node)
//        {
//            if (node.NodeType == ExpressionType.MemberAccess
//                && !seenExpressions.Contains(node))
//            {
//                //we need to check if there's a constant at the root of this and build the entire path on that constant
//                var exprs = Utilities.GetSimpleMemberAccessStretch(node, out var entityExpr);
//                if (entityExpr != null || (exprs?.Count > 0 && (entityExpr = exprs[0]) != null))
//                {
//                    seenExpressions.AddRange(exprs);

//                    var constantExpr = entityExpr as ConstantExpression ?? ((entityExpr as MemberExpression)?.Expression as ConstantExpression);
//                    var obj = constantExpr?.Value;

//                    if (obj != null && (objectsToConsider == null || objectsToConsider.Contains(obj)))
//                    {
//                        //build this object path
//                        var index = exprs.IndexOf(entityExpr) + 1;
//                        Utilities.TraverseEntityPath(obj, exprs, ref index,
//                            out var lastType, out var pathTraversed, buildPath: true);
//                    }
//                }
//            }

//            return base.VisitMember(node);
//        }
//    }
//}
