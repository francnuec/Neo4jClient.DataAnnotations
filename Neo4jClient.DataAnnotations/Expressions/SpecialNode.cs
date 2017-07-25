using Neo4jClient.DataAnnotations.Serialization;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class SpecialNode
    {
        public SpecialNode(EntityExpressionVisitor visitor, Expression node,
            List<Expression> filtered = null, Expression filteredVal = null)
        {
            Visitor = visitor;
            Node = node;
            this.filtered = filtered;
            this.filteredVal = filteredVal;
        }

        public EntityExpressionVisitor Visitor { get; }

        public Expression Node { get; internal set; }

        private List<Expression> filtered;
        public List<Expression> Filtered
        {
            get { return filtered ?? (Node != null ? (filtered = Utilities.GetSimpleMemberAccessStretch(Node, out filteredVal)) : null); }
            protected internal set { filtered = value; }
        }

        private Expression filteredVal;
        public Expression FilteredValueGuess
        {
            get { return filteredVal; }
            protected internal set { filteredVal = value; }
        }

        public int Index { get { return Visitor.SpecialNodes.IndexOf(this); } }

        private SpecialNodeType? type;
        public SpecialNodeType Type
        {
            get
            {
                //if (type == null && Node != null)
                //{
                //    type = Utilities.HasParams(Filtered) ? SpecialNodeType.Vars : 
                //        (Visitor != null && Visitor.IsPredicateAssignmentValue(Node) ? 
                //        SpecialNodeType.PredicateAssignment : 
                //        SpecialNodeType.Other
                //        );
                //}

                return type ?? SpecialNodeType.Other;
            }
            protected internal set
            {
                type = value;
            }
        }

        public Expression Placeholder { get; protected internal set; }

        private object concreteValue;
        public object ConcreteValue
        {
            get
            {
                if (concreteValue == null)
                {
                    concreteValue = ResolveConcreteValue(this, Visitor.Resolver, Visitor.Serializer, useResolvedJsonName: null);
                }

                return concreteValue;
            }
            protected internal set
            {
                concreteValue = value;
            }
        }

        public bool FoundWhileVisitingPredicate { get; protected internal set; }

        public static object ResolveConcreteValue(SpecialNode specialNode, EntityResolver resolver = null, Func<object, string> serializer = null, bool? useResolvedJsonName = null)
        {
            object ret = null;

            if (specialNode.Type == SpecialNodeType.Variable)
            {
                ret = Utilities.BuildVars(specialNode.Filtered, resolver, serializer, out var typeReturned, useResolvedJsonName: useResolvedJsonName);
            }
            else
            {
                ret = specialNode.Node.ExecuteExpression<object>();
            }

            return ret;
        }
    }

    public enum SpecialNodeType
    {
        Other = 0,
        Variable = 1,
        //PredicateAssignment = 2,
        MemberAccessExpression = 3
    }
}
