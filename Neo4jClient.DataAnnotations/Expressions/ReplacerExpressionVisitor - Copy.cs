//using System;
//using Neo4jClient.DataAnnotations.Utils;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Text;

//namespace Neo4jClient.DataAnnotations.Expressions
//{
//    public class ReducerExpressionVisitor : ExpressionVisitor
//    {
//        public override Expression Visit(Expression node)
//        {
//            Expression reduced = node;

//            do
//            {
//                reduced = reduced?.Reduce();
//                if (reduced != node)
//                {

//                }
//            } while (reduced?.CanReduce == true);

//            return base.Visit(reduced);
//        }
//    }
//}
