//using Neo4jClient.DataAnnotations.Cypher;
//using Neo4jClient.DataAnnotations.Serialization;
//using System;
//using Neo4jClient.DataAnnotations.Utils;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Text;

//namespace Neo4jClient.DataAnnotations.Expressions
//{
//    public class SpecialNode
//    {
//        public SpecialNode(EntityExpressionVisitor visitor, Expression node,
//            List<Expression> filtered = null, Expression filteredVal = null)
//        {
//            Visitor = visitor;
//            Node = node;
//            this.filtered = filtered;
//            this.filteredVal = filteredVal;
//        }

//        public EntityExpressionVisitor Visitor { get; }

//        public Expression Node { get; internal set; }

//        private List<Expression> filtered;
//        public List<Expression> Filtered
//        {
//            get { return filtered ?? (Node != null ? (filtered = ExpressionUtilities.GetSimpleMemberAccessStretch
//                    (Visitor.EntityService, Node, out filteredVal)) : null); }
//            protected internal set { filtered = value; }
//        }

//        private Expression filteredVal;
//        public Expression FilteredValueGuess
//        {
//            get { return filteredVal; }
//            protected internal set { filteredVal = value; }
//        }

//        public int Index { get { return Visitor.SpecialNodes.IndexOf(this); } }

//        private SpecialNodeType? type;
//        public SpecialNodeType Type
//        {
//            get
//            {
//                //if (type == null && Node != null)
//                //{
//                //    type = Utilities.HasParams(Filtered) ? SpecialNodeType.CypherVariables : 
//                //        (Visitor != null && Visitor.IsPredicateAssignmentValue(Node) ? 
//                //        SpecialNodeType.PredicateAssignment : 
//                //        SpecialNodeType.Other
//                //        );
//                //}

//                return type ?? SpecialNodeType.Other;
//            }
//            protected internal set
//            {
//                type = value;
//            }
//        }

//        public Expression Placeholder { get; protected internal set; }

//        private object concreteValue;
//        public object ConcreteValue
//        {
//            get
//            {
//                if (concreteValue == null)
//                {
//                    concreteValue = ResolveConcreteValue(this, Visitor.QueryContext, useResolvedJsonName: null);
//                }

//                return concreteValue;
//            }
//            protected internal set
//            {
//                concreteValue = value;
//            }
//        }

//        public bool FoundWhileVisitingPredicate { get; protected internal set; }

//        public static object ResolveConcreteValue(SpecialNode specialNode, QueryContext queryContext,
//            bool? useResolvedJsonName = null)
//        {
//            object ret = null;

//            try
//            {
//                ret = specialNode.Node.ExecuteExpression<object>();
//            }
//            catch
//            {
//                //maybe has functions/variables that can be handled
//                var varsVisitor = new FunctionExpressionVisitor(queryContext, new FunctionVisitorContext()
//                {
//                    UseResolvedJsonName = useResolvedJsonName,
//                });
//                varsVisitor.Visit(specialNode.Node);

//                ret = varsVisitor.Builder.ToString();
//            }

//            //if (specialNode.Type == SpecialNodeType.Variable)
//            //{
//            //    var varsVisitor = new VarsExpressionVisitor(resolver, serializer);
//            //    varsVisitor.Visit(specialNode.Node);

//            //    ret = varsVisitor.Builder.ToString();
//            //}
//            //else
//            //{
//            //    ret = specialNode.Node.ExecuteExpression<object>();
//            //}

//            return ret;
//        }
//    }

//    public enum SpecialNodeType
//    {
//        Other = 0,
//        Function = 1,
//        //PredicateAssignment = 2,
//        MemberAccessExpression = 3
//    }
//}
