﻿using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using Neo4jClient.Serialization;
using Newtonsoft.Json.Linq;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Cypher;

namespace Neo4jClient.DataAnnotations.Expressions
{
    //
    public class NewEntityExpressionVisitor : ExpressionVisitor, IHaveAnnotationsContext
    {
        public static readonly Type DictType = typeof(Dictionary<string, object>);

        public static readonly MethodInfo DictAddMethod = Utils.Utilities.GetMethodInfo(() => new Dictionary<string, object>().Add(null, null));

        public Func<object, string> Serializer => QueryContext?.SerializeCallback;

        public EntityResolver Resolver => QueryContext?.Resolver;

        public QueryContext QueryContext { get; internal set; }


        public MethodCallExpression SetNode { get; private set; }

        public Expression SetInstanceNode { get; private set; }

        public Expression SetPredicateNode { get; private set; }

        public Dictionary<Expression, Expression> SetPredicateAssignments { get; private set; }

        public Dictionary<List<MemberInfo>, Expression> SetPredicateMemberAssignments { get; private set; }

        public bool SetUsePredicateOnly { get; private set; }

        private bool isVisitingPredicate = false;

        private FunctionExpressionVisitor _funcsVisitor = null;
        protected FunctionExpressionVisitor FuncsVisitor
        {
            get
            {
                if (_funcsVisitor == null)
                {
                    _funcsVisitor = new FunctionExpressionVisitor(QueryContext, new FunctionVisitorContext()
                    {
                        //we most likely aint needing these
                        WriteConstants = false,
                        RewriteNullEqualityComparisons = false,
                        WriteOperators = false,
                        WriteParameters = false
                    });
                }

                return _funcsVisitor;
            }
        }

        public IAnnotationsContext AnnotationsContext => QueryContext?.AnnotationsContext;

        public IEntityService EntityService => AnnotationsContext?.EntityService;

        public Dictionary<EntityMemberInfo, Expression> PendingAssignments { get; set; }
            = new Dictionary<EntityMemberInfo, Expression>();

        private EntityMemberInfo currentMember;
        private Type lastType;
        private Type currentType;

        public NewEntityExpressionVisitor(QueryContext queryContext)
        {
            QueryContext = queryContext ?? throw new ArgumentNullException(nameof(queryContext));
        }

        //private void SetCurrentType(Type newType)
        //{
        //    if (currentType == null || newType == null)
        //    {
        //        currentType = newType;
        //    }
        //    else if (!newType.IsAssignableFrom(currentType) && !newType.IsGenericAssignableFrom(currentType))
        //    {
        //        //we have a new type if the newType is not assignable from the currentType
        //        currentType = newType;
        //    }
        //}

        public override Expression Visit(Expression node)
        {
            lastType = currentType;
            currentType = node.Type;

            var newNode = base.Visit(node);
            return newNode;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (RootNode == node)
            {
                if (node.Type.IsAnonymousType())
                {
                    if (node.Members?.Count > 0)
                    {
                        var dictItems = GetNewExpressionItems(node);
                        //generate dictionary expression
                        var dictExpr = Expression.ListInit(Expression.New(DictType), dictItems);
                        var dictNode = VisitListInit(dictExpr);

                        //replace the empty members of dict member names generated
                        if (dictMemberNames != null && dictMemberNames.Count > 0)
                        {
                            foreach (var member in node.Members)
                            {
                                if (dictMemberNames.TryGetValue(member.Name, out var dictValue))
                                {
                                    if (dictValue.Count == 1 && dictValue[0].ComplexPath == null)
                                    {
                                        //not a complex type
                                        //so add this member
                                        //this way, all values here would have at least one member
                                        dictValue[0] = new DictMemberInfo(dictValue[0].JsonName, new List<MemberInfo>() { member });
                                    }
                                }
                            }
                        }

                        return dictNode;
                    }
                }
            }

            var index = SpecialNodePaths.Count; //do this to know which Paths to add to

            var newNode = base.VisitNew(node);

            if (newNode.Type.IsAnonymousType())
            {
                AddToPaths(newNode, index);
            }

            return newNode;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            if (node.NodeType == ExpressionType.NewArrayInit)
            {
                var index = SpecialNodePaths.Count; //do this to know which Paths to add to
                var newNode = base.VisitNewArray(node);
                AddToPaths(newNode, index);
                return newNode;
            }

            return node;
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            return node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var parentMember = currentMember;

            foreach (var binding in node.Bindings)
            {
                currentMember = new EntityMemberInfo(binding.Member, parentMember);

                if (currentMember.MemberFinalType != currentType
                    && (currentMember.MemberFinalType.IsAssignableFrom(lastType)
                    || currentMember.MemberFinalType.IsGenericAssignableFrom(lastType)))
                {
                    currentMember.MemberFinalType = lastType;
                }

                switch (binding)
                {
                    case MemberAssignment assignment:
                        {
                            if (IsSpecialNode(assignment.Expression))
                            {

                            }
                            break;
                        }
                    case MemberListBinding listBinding:
                        {
                            break;
                        }
                    case MemberMemberBinding memberMemberBinding:
                        {
                            break;
                        }
                }
            }

            return node;
        }

        private IEnumerable<MemberBinding> InternalVisitBindings(List<MemberBinding> bindings, EntityMemberInfo parent)
        {
            var newBindings = bindings ?? new List<MemberBinding>();

            foreach (var binding in bindings)
            {
                var currentMember = new EntityMemberInfo(binding.Member, parent);

                switch (binding)
                {
                    case MemberAssignment assignment:
                        {
                            if (IsSpecialNode(assignment.Expression, out var hasVars, out var hasFunctions, out var hasDummyMethod))
                            {
                                if (currentMember.MemberFinalType != currentType
                                    && (currentMember.MemberFinalType.IsAssignableFrom(lastType)
                                    || currentMember.MemberFinalType.IsGenericAssignableFrom(lastType)))
                                {
                                    currentMember.MemberFinalType = lastType;
                                }
                                assignment = Expression.Bind(assignment.Member, Expression.Constant());
                            }

                            break;
                        }
                    case MemberListBinding listBinding:
                        {
                            
                            break;
                        }
                    case MemberMemberBinding memberMemberBinding:
                        {
                            break;
                        }
                }
            }
        }

        private bool IsSpecialNode(Expression node, out bool hasVars, out bool hasFunctions, out bool hasDummyMethod)
        {
            hasVars = false; hasFunctions = false; hasDummyMethod = false;

            if (node == null)
                return false;

            bool isSpecialNode = false;

            //first see if we can successfully execute the expression
            object value = null;
            try
            {
                value = node.ExecuteExpression<object>();
            }
            catch(Exception e)
            {
                while (e != null)
                {
                    if (e is NotImplementedException ne)
                    {
                        switch (e.Message)
                        {
                            case Messages.VarsGetError:
                                {
                                    hasVars = true;
                                    break;
                                }
                            case Messages.FunctionsInvokeError:
                                {
                                    hasFunctions = true;
                                    break;
                                }
                            case Messages.DummyMethodInvokeError:
                                {
                                    hasDummyMethod = true;
                                    break;
                                }
                        }

                        isSpecialNode = hasVars || hasFunctions || hasDummyMethod;

                        if (isSpecialNode)
                            break;
                    }

                    e = e.InnerException;
                }
            }

            if (!isSpecialNode)
            {
                //try a full search then
                List<Expression> filtered = ExpressionUtilities.GetSimpleMemberAccessStretch(EntityService, node, out var filteredVal, out var isContinuous);
                var referenceNode = filtered.LastOrDefault();

                //if (Utils.Utilities.HasVars(filtered))
                //{
                //    //found a special node.
                //    isSpecialNode = true;
                //}
                //else 
                if (referenceNode?.Type == Defaults.JRawType) //never directly delegate jraw to the serializer. use the functions visitor for it instead
                {
                    isSpecialNode = true;
                }
                else if (isContinuous && filtered.Count > 0 && referenceNode?.NodeType == ExpressionType.Call)
                {
                    //maybe one of our functions
                    //check if it can be handled
                    if (FuncsVisitor.CanHandle(referenceNode, out var handler))
                    {
                        //found a special node
                        isSpecialNode = true;
                    }
                }
            }

            return isSpecialNode;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.NodeType == ExpressionType.MemberAccess)
            {
                var exprType = node.Expression?.NodeType;
                switch (exprType)
                {
                    case ExpressionType.MemberInit:
                    case ExpressionType.ListInit:
                    case ExpressionType.New:
                        {
                            //cache this expression because of the expansions we make
                            var specialNode =
                                AddSpecialNode(node.Expression, SpecialNodeType.MemberAccessExpression,
                                out var isNewlyAddedSpecialNode, generatePlaceholder: true);

                            if (isNewlyAddedSpecialNode)
                            {
                                specialNode.Node = Visit(node.Expression);
                            }

                            node = Expression.MakeMemberAccess(specialNode.Placeholder, node.Member);
                            break;
                        }
                }
            }

            return base.VisitMember(node);
        }
    }
}
