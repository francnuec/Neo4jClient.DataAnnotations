//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Linq;

//namespace Neo4jClient.DataAnnotations
//{
//    public class MemberAccessVisitor : ExpressionVisitor
//    {
//        Stack<Expression> filtered = new Stack<Expression>();

//        protected override Expression VisitMethodCall(MethodCallExpression node)
//        {
//            filtered.Push(node);
//            return base.VisitMethodCall(node);
//        }

//        protected override Expression VisitMember(MemberExpression node)
//        {
//            if (node.NodeType == ExpressionType.MemberAccess)
//                filtered.Push(node);

//            return base.VisitMember(node);
//        }

//        protected override Expression VisitUnary(UnaryExpression node)
//        {
//            switch (node.NodeType)
//            {
//                case ExpressionType.TypeAs:
//                case ExpressionType.Convert:
//                case ExpressionType.ConvertChecked:
//                case ExpressionType.Unbox:
//                    {
//                        filtered.Push(node);
//                        break;
//                    }
//            }

//            return base.VisitUnary(node);
//        }

//        protected override Expression VisitBinary(BinaryExpression node)
//        {
//            if (node.NodeType == ExpressionType.ArrayIndex)
//            {
//                filtered.Push(node);
//            }

//            return base.VisitBinary(node);
//        }

//        public override Expression Visit(Expression node)
//        {
//            return base.Visit(node);
//        }

//        public List<Expression> FilteredExpressions
//        {
//            get
//            {
//                return filtered.ToList();
//            }
//        }

//        public void Clear()
//        {
//            filtered.Clear();
//        }
//    }
//}
