using System;
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
    public class EntityExpressionVisitor : ExpressionVisitor, IHaveAnnotationsContext
    {
        public static readonly Type DictType = typeof(Dictionary<string, object>);

        public static readonly MethodInfo DictAddMethod = Utils.Utilities.GetMethodInfo(() => new Dictionary<string, object>().Add(null, null));

        private Dictionary<string, List<DictMemberInfo>> dictMemberNames;
        public Dictionary<string, List<DictMemberInfo>> DictMemberNames { get { return dictMemberNames; } }


        protected internal List<Expression> SpecialNodeExpressions { get; } = new List<Expression>();

        public List<SpecialNodePath> SpecialNodePaths { get; } = new List<SpecialNodePath>();

        public List<SpecialNode> SpecialNodes { get; } = new List<SpecialNode>();

        public Func<object, string> Serializer => QueryContext?.SerializeCallback;

        public EntityResolver Resolver => QueryContext?.Resolver;

        public Expression RootNode { get; private set; }

        public Expression Source { get; private set; }

        public QueryContext QueryContext { get; internal set; }


        public MethodCallExpression SetNode { get; private set; }

        public Expression SetInstanceNode { get; private set; }

        public Expression SetPredicateNode { get; private set; }

        public Dictionary<Expression, Expression> SetPredicateAssignments { get; private set; }

        public Dictionary<MemberInfo, Expression> SetPredicateMemberAssignments { get; private set; }

        public Dictionary<string, Expression> SetPredicateDictionaryAssignments { get; private set; }

        public List<object> SetPredicateBindings { get; private set; }

        public ParameterExpression SetPredicateParameter { get; private set; }

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

        public EntityExpressionVisitor(QueryContext queryContext)
        {
            QueryContext = queryContext ?? throw new ArgumentNullException(nameof(queryContext));
        }


        public override Expression Visit(Expression node)
        {
            if (Source == null)
                Source = node; //this never changes

            bool assignRootNode = false;

            if (RootNode == null)
            {
                RootNode = node;
                assignRootNode = true;
            }

            bool isSpecialFunctionsNode = false;

            List<Expression> filtered = ExpressionUtilities.GetSimpleMemberAccessStretch(EntityService, node, out var filteredVal, out var isContinuous);
            bool isVars = Utils.Utilities.HasVars(filtered);

            if (isVars)
            {
                //found a special node.
                isSpecialFunctionsNode = true;
            }
            else if (node?.Type == Defaults.JRawType) //never directly delegate jraw to the serializer. use the functions visitor for it instead
            {
                isSpecialFunctionsNode = true;
            }
            else if (isContinuous && filtered.Count > 0 && node?.NodeType == ExpressionType.Call)
            {
                //first see if we can successfully execute the expression
                object value = null;
                try
                {
                    value = node.ExecuteExpression<object>();
                }
                catch
                {

                }

                if (value == null)
                {
                    //maybe one of our functions
                    //check if it can be handled
                    if (FuncsVisitor.CanHandle(node, out var handler))
                    {
                        //found a special node
                        isSpecialFunctionsNode = true;
                    }
                }
            }

            if (isSpecialFunctionsNode)
            {
                SpecialNode specialNode = AddSpecialNode(node, SpecialNodeType.Function, out var isNew,
                    filtered, filteredVal, generatePlaceholder: true, nodePlaceholder: node);

                node = specialNode.Placeholder;
            }

            var newNode = base.Visit(node);

            if (assignRootNode)
                RootNode = newNode;

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
            node = ResolveDictListInitExpression(node, storeMemberNames: dictMemberNames == null);

            var index = SpecialNodePaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitListInit(node);
            AddToPaths(newNode, index);

            return newNode;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var index = SpecialNodePaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitMemberInit(node);
            AddToPaths(newNode, index);
            return newNode;
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            var index = SpecialNodePaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitElementInit(node);
            AddToPaths(newNode, index);
            return newNode;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            //if (ExpressionUtilities.HasNfpEscape(node.Expression) && !NfpEscapedMembers.Any(m => (m as MemberInfo)?.IsEquivalentTo(node.Member) == true))
            //{
            //    NfpEscapedMembers.Add(node.Member);
            //}

            var index = SpecialNodePaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitMemberAssignment(node);
            AddToPaths(newNode, index);
            return newNode;
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            var index = SpecialNodePaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitMemberListBinding(node);
            AddToPaths(newNode, index);
            return newNode;
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            var index = SpecialNodePaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitMemberMemberBinding(node);
            AddToPaths(newNode, index);
            return newNode;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (SetNode == null && Utils.Utilities.HasSet(node))
            {
                SetNode = node;

                bool isAnonymousType = node.Type.IsAnonymousType();
                bool isDictionaryType = node.Type.IsDictionaryType();

                //Build the entity expression
                var instanceExpr = node.Arguments[0];

                if (node.Arguments.Count == 3)
                {
                    //variant method
                    SetUsePredicateOnly = node.Arguments[2].ExecuteExpression<bool>();
                }

                var predicate = node.Arguments[1].ExecuteExpression<LambdaExpression>();


                //do instance visit early on
                if (RootNode == node)
                {
                    //try to exclude 'set' from the normal process.
                    //so that if the first argument is an anonymous type, it will continue accordingly
                    RootNode = instanceExpr;
                }

                SetInstanceNode = Visit(instanceExpr);


                //sort out predicate
                Expression predicateExpression = null;

                if (predicate != null && predicate.Body is BinaryExpression)
                {
                    //split the binary expressions
                    var predicateVisitor = new PredicateExpressionVisitor()
                    {
                        MakeRightNodeReplacements = true,
                        RightNodeReplacements = predicate.Parameters.Select(p => new
                        {
                            Parameter = p,
                            VarsGetCall = ExpressionUtilities.GetVarsGetExpressionFor(p.Name, p.Type)
                        }).ToDictionary(np => np.Parameter as Expression, np => np.VarsGetCall as Expression)
                    };
                    predicateVisitor.Visit(predicate.Body);

                    SetPredicateAssignments = predicateVisitor.Assignments;

                    if (SetPredicateAssignments.Count > 0)
                    {
                        SetPredicateParameter = predicate.Parameters[0];

                        if (!isDictionaryType)
                        {
                            SetPredicateMemberAssignments = GetPredicateMemberAssignments
                                (SetPredicateAssignments, out var rootMembers, out var memberChildren);

                            SetPredicateBindings = GetPathBindings(rootMembers.Where(m => m != null), memberChildren, SetPredicateMemberAssignments);
                        }
                        else
                        {
                            SetPredicateDictionaryAssignments = new Dictionary<string, Expression>();

                            foreach (var assignment in SetPredicateAssignments)
                            {
                                var retrieved = ExpressionUtilities.GetSimpleMemberAccessStretch
                                    (EntityService, assignment.Key, out var valExpr);
                                var dictKey = (retrieved[1] as MethodCallExpression).Arguments[0].ExecuteExpression<object>().ToString();
                                SetPredicateDictionaryAssignments[dictKey] = assignment.Value;
                            }
                        }

                        //all sorted. lets generate the expression
                        predicateExpression = GetPredicateExpression(SetPredicateParameter, SetPredicateBindings,
                            SetPredicateMemberAssignments, SetPredicateDictionaryAssignments);
                    }
                }
                
                //perform predicate visit
                if (SetUsePredicateOnly)
                {
                    //reassign or clear everything
                    //root node would change now
                    RootNode = predicateExpression;

                    //clear all vars, we don't need them again.
                    SpecialNodes.Clear();
                    SpecialNodePaths.Clear();
                    SpecialNodeExpressions.Clear();
                    dictMemberNames?.Clear();
                }
                else if (isAnonymousType || isDictionaryType)
                {
                    var dictItems = (predicateExpression as ListInitExpression)?.Initializers.AsEnumerable();

                    if (isAnonymousType)
                    {
                        //generate the dictionary items for the anonymous types
                        dictItems = GetNewExpressionItems(predicateExpression as NewExpression);
                    }

                    //use the instance dictionary names to set this predicate member names
                    predicateExpression = ResolveDictListInitMemberNames(dictItems, DictMemberNames);
                }

                isVisitingPredicate = true;
                SetPredicateNode = Visit(predicateExpression);
                isVisitingPredicate = false;

                return SetUsePredicateOnly ? SetPredicateNode : SetInstanceNode;
            }

            return base.VisitMethodCall(node);
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


        protected virtual SpecialNode AddSpecialNode
            (Expression node, SpecialNodeType type, out bool isNew, 
            List<Expression> filtered = null, Expression filteredVal = null, 
            bool generatePlaceholder = false, Expression nodePlaceholder = null)
        {
            //found a special node.
            //be careful to check first if we have encountered this node before
            int index = SpecialNodeExpressions.IndexOf(node);
            nodePlaceholder = node;
            SpecialNode specialNode = null;
            isNew = false;

            if (index < 0)
            {
                //store
                specialNode = new SpecialNode(this, node, filtered, filteredVal)
                {
                    Type = type,
                    FoundWhileVisitingPredicate = isVisitingPredicate
                };

                SpecialNodes.Add(specialNode);
                SpecialNodeExpressions.Add(node);

                index = SpecialNodeExpressions.Count - 1;

                if (generatePlaceholder)
                {
                    if (type == SpecialNodeType.Function)
                    {
                        nodePlaceholder = Expression.Call(Defaults.UtilitiesType, "GetValue", new[] { node.Type }, Expression.Constant(index));
                    }
                    else
                    {
                        nodePlaceholder = Expression.Call(Defaults.ExtensionsType, "GetValue", new[] { node.Type },
                            Expression.Constant(this), Expression.Constant(index));
                    }
                }

                specialNode.Placeholder = nodePlaceholder;
                isNew = true;
            }
            else
            {
                specialNode = SpecialNodes[index];
                nodePlaceholder = specialNode.Placeholder;
            }

            SpecialNodePaths.Add(new SpecialNodePath(new List<object>() { nodePlaceholder }, specialNode));

            return specialNode;
        }

        protected virtual void AddToPaths(object node, int index)
        {
            for (int i = index, l = SpecialNodePaths.Count; i < l; i++)
            {
                SpecialNodePaths[i].Path.Add(node);
            }
        }

        protected virtual List<ElementInit> GetNewExpressionItems(NewExpression node)
        {
            //transform the anonymous type expression to a dictionary expression
            List<ElementInit> dictItems = new List<ElementInit>();

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                //generate the expression entry
                var argument = node.Arguments[i];
                var member = node.Members[i].Name;

                if (argument != null && member != null)
                {
                    dictItems.Add(Expression.ElementInit
                        (DictAddMethod, Expression.Constant(member),
                        argument.Type != Defaults.ObjectType ?
                        Expression.Convert(argument, Defaults.ObjectType) :
                        argument
                        ));
                }
            }

            return dictItems;
        }

        protected virtual string GetMemberName(Expression argument)
        {
            string name = null;

            var retrievedExprs = ExpressionUtilities.GetSimpleMemberAccessStretch(EntityService, argument, out var entityExpr);

            if (retrievedExprs?.Count > 1)
            {
                object entity = null;

                if (Resolver == null && Serializer != null)
                {
                    //get the entity object
                    try
                    {
                        entity = entityExpr.ExecuteExpression<object>();
                    }
                    catch
                    {
                        //something went wrong.
                        //that shouldn't deter us now to get memberName
                        //try activating manually

                        try
                        {
                            entity = Utils.Utilities.CreateInstance(entityExpr.Type);
                        }
                        catch
                        {

                        }
                    }
                }

                //get the names
                var currentIndex = ExpressionUtilities.GetFirstMemberAccessIndex(retrievedExprs, entityExpr, out var firstAccess); //retrievedExprs.IndexOf(entityExpr) + 1;
                Type cast = null;
                var entityType = entity?.GetType() ??
                    (currentIndex < retrievedExprs.Count //check if the next expression to entity expression was just a cast, and instead use the cast type as entity type
                    && retrievedExprs[currentIndex]?.Uncast(out cast) == entityExpr
                    && cast != null ? cast : entityExpr.Type);

                var memberNames = ExpressionUtilities.GetEntityPathNames
                    (EntityService, ref entity, ref entityType, retrievedExprs, 
                    ref currentIndex, Resolver, Serializer,
                    out var entityMembers, out var lastType, useResolvedJsonName: true);

                name = memberNames?.LastOrDefault(); //we are only interested in the last member name.
            }

            return name;
        }

        protected virtual List<object> GetPathBindings(IEnumerable<MemberInfo> members,
            Dictionary<MemberInfo, Tuple<Type, List<MemberInfo>>> memberChildren,
            Dictionary<MemberInfo, Expression> memberAssignments)
        {
            List<object> bindings = new List<object>();

            foreach (var member in members)
            {
                var propInfo = member as PropertyInfo;
                var fieldInfo = member as FieldInfo;
                var value = memberChildren[member];
                var children = value.Item2;

                Expression assignment = null;
                memberAssignments.TryGetValue(member, out assignment);

                bool isWritable = propInfo?.CanWrite == true || fieldInfo != null;
                bool hasChildren = children?.Count > 0 && assignment == null; //skip the children if it was assigned to directly

                Type type = value.Item1 ?? propInfo?.PropertyType ?? fieldInfo?.FieldType;

                object binding = null;
                List<object> childBindings = null;
                IEnumerable<MemberBinding> filteredChildBindings = null;

                assignment = assignment ?? (!hasChildren ? Expression.Constant(type.GetDefaultValue()) : null);

                if (hasChildren)
                {
                    childBindings = GetPathBindings(children, memberChildren, memberAssignments);
                    filteredChildBindings = childBindings?.Select(cb => cb as MemberBinding).Where(cb => cb != null);
                }

                if (isWritable)
                {
                    //assign a new instance
                    try
                    {
                        var expr = !hasChildren ? assignment : Expression.MemberInit
                            (Expression.New(type), filteredChildBindings);

                        binding = Expression.Bind(member, expr);
                    }
                    catch (Exception e)
                    {
                    }
                }

                if (binding == null)
                {
                    //maybe readonly property.

                    if (hasChildren && isWritable)
                    {
                        //take a wild shot at membermemberbinding
                        //if it then fails, user must change his code.
                        binding = Expression.MemberBind(member, filteredChildBindings);
                    }
                    else
                    {
                        //use tuple
                        binding = new Tuple<MemberInfo, Tuple<Type, object>>(member,
                            new Tuple<Type, object>(type,
                            hasChildren ? (object)filteredChildBindings.ToArray() : assignment));
                    }
                }

                bindings.Add(binding);
            }

            return bindings;
        }

        protected virtual Dictionary<MemberInfo, Expression> GetPredicateMemberAssignments(
            Dictionary<Expression, Expression> assignments,
            out List<MemberInfo> rootMembers,
            out Dictionary<MemberInfo, Tuple<Type, List<MemberInfo>>> memberChildren)
        {
            var memberAssignments = new Dictionary<MemberInfo, Expression>();

            memberChildren = new Dictionary<MemberInfo, Tuple<Type, List<MemberInfo>>>();

            rootMembers = new List<MemberInfo>();

            foreach (var assignment in assignments)
            {
                var assignmentExprs = ExpressionUtilities.GetSimpleMemberAccessStretch
                    (EntityService, assignment.Key, out var entityVal);
                var startIndex = ExpressionUtilities.GetFirstMemberAccessIndex(assignmentExprs, entityVal, out var firstAccess);

                ExpressionUtilities.TraverseEntityPath(EntityService, null, assignmentExprs, ref startIndex,
                    out var assignmentLastType, out var assignmentPath, buildPath: false);

                memberAssignments[assignmentPath.Last().Key] = assignment.Value;

                //sort members and children
                var members = assignmentPath.OrderBy(p => p.Value.Item1).Select(p => p.Key).ToList();

                rootMembers.Add(members.FirstOrDefault());

                SortMemberAccess(memberChildren, members, (member) => assignmentPath
                .TryGetValue(member, out var val) ? val.Item2 : member.GetMemberType());

                //foreach (var member in members)
                //{
                //    var child = members.Skip(members.IndexOf(member) + 1).FirstOrDefault();

                //    if (!memberChildren.TryGetValue(member, out var memberVal))
                //    {
                //        memberVal = new Tuple<Type, List<MemberInfo>>(assignmentPath[member].Item2, new List<MemberInfo>());
                //        memberChildren[member] = memberVal;
                //    }
                //    else if (child?.DeclaringType != memberVal.Item1
                //      && memberVal.Item1.IsGenericAssignableFrom(child.DeclaringType))
                //    {
                //        //pick the derived type
                //        memberVal = new Tuple<Type, List<MemberInfo>>(child.DeclaringType, memberVal.Item2);
                //        memberChildren[member] = memberVal;
                //    }

                //    if (child != null)
                //        memberVal.Item2.Add(child);
                //}
            }

            return memberAssignments;
        }

        private void SortMemberAccess(Dictionary<MemberInfo, Tuple<Type, List<MemberInfo>>> memberChildren,
            List<MemberInfo> members, Func<MemberInfo, Type> getMemberType, bool handleComplexTypes = true)
        {
            foreach (var member in members)
            {
                var child = members.Skip(members.IndexOf(member) + 1).FirstOrDefault();

                if (!memberChildren.TryGetValue(member, out var memberVal))
                {
                    memberVal = new Tuple<Type, List<MemberInfo>>(getMemberType(member), new List<MemberInfo>());
                    memberChildren[member] = memberVal;
                }

                if (child != null)
                {
                    if (child.DeclaringType != memberVal.Item1
                        && memberVal.Item1.IsGenericAssignableFrom(child.DeclaringType))
                    {
                        //pick the derived type
                        memberVal = new Tuple<Type, List<MemberInfo>>(child.DeclaringType, memberVal.Item2);
                        memberChildren[member] = memberVal;
                    }

                    if (!memberVal.Item2.Contains(child))
                        memberVal.Item2.Add(child);
                }
            }

            //handle complex types
            foreach (var member in members)
            {
                var memberVal = memberChildren[member];

                if (memberVal.Item1.IsComplex())
                {
                    //get all complex paths
                    ExpressionUtilities.ExplodeComplexType
                        (EntityService, memberVal.Item1, out var inversePaths);

                    if (inversePaths.Count > 0)
                    {
                        foreach (var inversePath in inversePaths)
                        {
                            inversePath.Reverse();

                            var child = inversePath[0];

                            if (!memberVal.Item2.Contains(child))
                            {
                                memberVal.Item2.Add(child);
                            }

                            SortMemberAccess(memberChildren, inversePath, getMemberType, handleComplexTypes: handleComplexTypes);
                        }
                    }
                }
            }
        }

        protected virtual Expression GetPredicateExpression(
            ParameterExpression parameter, List<object> bindings,
            Dictionary<MemberInfo, Expression> memberAssignments,
            Dictionary<string, Expression> dictionaryAssignments)
        {
            NewExpression newExpr = null;
            Expression predicateExpression = null;

            if (parameter.Type.IsAnonymousType())
            {
                //anonymous type
                var constructor = parameter.Type.GetConstructors()[0];

                //build the argument list from the anonymous type declared properties.
                //by default, the order at which they were declared on the type is the order for which they would appear in the constructor
                var props = parameter.Type.GetProperties();
                var arguments = new List<Expression>();

                var tuples = bindings
                        .Select(b => b as Tuple<MemberInfo, Tuple<Type, object>>) //we expect tuples here as anonymous types would naturally have readonly properties
                        .Where(b => b != null)
                        .ToArray();

                foreach (var prop in props)
                {
                    Expression arg = null;

                    arg = tuples
                        .Where(b => b.Item1.IsEquivalentTo(prop))
                        .Select(b =>
                        {
                            var value = b.Item2.Item2 as Expression;

                            if (value == null)
                            {
                                //chould have children then
                                //construct a new expression
                                value = Expression.MemberInit(
                                    Expression.New(b.Item2.Item1 ?? prop.PropertyType),
                                    b.Item2.Item2 as IEnumerable<MemberBinding>);
                            }

                            return value;
                        }).FirstOrDefault();


                    if (arg == null)
                    {
                        //try member assignments
                        arg = memberAssignments.FirstOrDefault(mp => mp.Key.IsEquivalentTo(prop)).Value;
                    }

                    if (arg == null)
                    {
                        //finally just assign a default value.
                        //the belief is that by now if there was an error, an exception should have been thrown somewhere already.
                        arg = Expression.Constant(prop.PropertyType.GetDefaultValue());
                    }

                    arguments.Add(arg);
                }

                newExpr = Expression.New(constructor, arguments, props);
                predicateExpression = newExpr; //one and the same
            }
            else if (parameter.Type.IsDictionaryType())
            {
                //build list init
                newExpr = Expression.New(DictType);
                predicateExpression = Expression.ListInit(newExpr,
                    dictionaryAssignments.Select(item =>
                    Expression.ElementInit(DictAddMethod, Expression.Constant(item.Key), 
                    item.Value.Type != Defaults.ObjectType ?
                    Expression.Convert(item.Value, Defaults.ObjectType) :
                    item.Value)));
            }
            else
            {
                newExpr = Expression.New(parameter.Type);
                predicateExpression = Expression.MemberInit(newExpr, bindings.Select(b => b as MemberBinding).Where(b => b != null));
            }

            return predicateExpression;
        }

        protected virtual ListInitExpression ResolveDictListInitExpression(ListInitExpression node, bool storeMemberNames)
        {
            if (node.NewExpression.Type.IsDictionaryType())
            {
                //generate dictionary expression
                node = GetDictListInitExpression(node.Initializers, out var _dictMemberNames);

                if (storeMemberNames)
                {
                    if (dictMemberNames == null)
                    {
                        dictMemberNames = _dictMemberNames;
                    }
                    else
                    {
                        foreach (var item in _dictMemberNames)
                        {
                            dictMemberNames[item.Key] = item.Value;
                        }
                    }
                }
            }

            return node;
        }

        protected virtual ListInitExpression GetDictListInitExpression(IEnumerable<ElementInit> initializers, 
            out Dictionary<string, List<DictMemberInfo>> dictMemberNames)
        {
            dictMemberNames = new Dictionary<string, List<DictMemberInfo>>();

            //transform expression to a dictionary<string, object> expression
            List<ElementInit> dictItems = new List<ElementInit>();

            foreach(var initializer in initializers)
            {
                //expand members if complex type
                //if there's a cast, remove it.
                var key = initializer.Arguments[0].Uncast(out var keyCastType);
                var baseMemberName = key.ExecuteExpression<object>().ToString();

                var value = initializer.Arguments[1].Uncast(out var argCastType);

                var baseMemberJsonName = new string(baseMemberName.ToCharArray());

                ResolveDictionaryMemberAssignment(baseMemberName, ref baseMemberJsonName, ref value, dictMemberNames,
                    out var members, out var arguments, out var isMemberAccess,
                    out var isComplexType, out var hasNfpEscape);

                //generate the expression entry
                for (int j = 0, jl = arguments.Count; j < jl; j++)
                {
                    var argument = arguments[j];
                    var member = members[j];

                    if (argument != null && member != null)
                    {
                        dictItems.Add(Expression.ElementInit
                            (DictAddMethod, Expression.Constant(member),
                            argument.Type != Defaults.ObjectType ?
                                Expression.Convert(argument, Defaults.ObjectType) :
                                argument
                            ));
                    }
                }
            }

            //generate dictionary expression
            return Expression.ListInit(Expression.New(DictType), dictItems);
        }

        protected virtual void ResolveDictionaryMemberAssignment
            (string baseMemberName, ref string baseMemberJsonName, ref Expression value,
            Dictionary<string, List<DictMemberInfo>> dictMemberNames,
            out List<string> members, out List<Expression> arguments,
            out bool isMemberAccess, out bool isComplexType, out bool hasNfpEscape)
        {
            baseMemberJsonName = baseMemberJsonName ?? baseMemberName;
            members = new List<string>() { baseMemberJsonName };
            arguments = new List<Expression>() { value };
            isMemberAccess = false;

            hasNfpEscape = Utils.Utilities.HasNfpEscape(value);

            var valueType = value.Type;
            isComplexType = valueType.IsComplex();

            if (!hasNfpEscape)
            {
                isMemberAccess = value.NodeType == ExpressionType.MemberAccess
                    && (value as MemberExpression).Member.Name == baseMemberName; //test first level first

                if (!isMemberAccess && value is UnaryExpression)
                {
                    //try second level
                    var memberExpr = value.Uncast(out var valCast) as MemberExpression;

                    if (memberExpr != null)
                    {
                        //test and accept second level too
                        isMemberAccess = memberExpr.NodeType == ExpressionType.MemberAccess
                            && memberExpr.Member.Name == baseMemberName;
                    }
                }

                if (isMemberAccess)
                {
                    value = value.Cast(out var valueCast); //take advantage of the object passed where available.
                    valueType = value.Type;
                }

                List<List<MemberInfo>> argInversePaths = null;

                if (isComplexType)
                {
                    //it's complex so resolve in the possible best way.
                    var expandedArgs = ExpressionUtilities.ExplodeComplexTypeMemberAccess(EntityService, value, out argInversePaths);

                    if (expandedArgs.Count > 0)
                    {
                        //has complex members
                        //use the scalars instead
                        arguments = expandedArgs;
                        isComplexType = true;
                    }
                }

                members.Clear();

                for (int j = 0, jl = arguments.Count; j < jl; j++)
                {
                    var argument = arguments[j];

                    string memberName = null;

                    if (isMemberAccess || isComplexType)
                    {
                        argument = argument.Cast(out var argCast);

                        //try find the actual json name
                        var newMemberName = GetMemberName(argument);
                        newMemberName = !string.IsNullOrWhiteSpace(newMemberName) ? newMemberName : null;

                        if (newMemberName != null)
                        {
                            if (isMemberAccess)
                            {
                                memberName = newMemberName;
                            }
                            else if (isComplexType)
                            {
                                memberName = $"{baseMemberJsonName}{Defaults.ComplexTypeNameSeparator}{newMemberName}";
                            }
                        }
                    }

                    if (memberName == null)
                    {
                        if (isComplexType)
                        {
                            //use appended name if complex type
                            memberName = argInversePaths[j].AsEnumerable().Reverse().Select(m => m.Name)
                                .Aggregate((first, second) => $"{first}{Defaults.ComplexTypeNameSeparator}{second}");

                            memberName = $"{baseMemberName}{Defaults.ComplexTypeNameSeparator}{memberName}"; //use its base member name here, not json's
                        }
                        else
                        {
                            //just use base name
                            memberName = baseMemberName;
                        }
                    }

                    members.Add(memberName);

                    AddToDictMemberName(dictMemberNames, baseMemberName, memberName, isComplexType ? argInversePaths[j] : null);
                }
            }
            else
            {
                AddToDictMemberName(dictMemberNames, baseMemberName, baseMemberJsonName, null);
            }
        }

        private void AddToDictMemberName(Dictionary<string, List<DictMemberInfo>> /*Dictionary<string, List<DictMemberInfo>>*/ dictMemberNames,
            string baseMemberName, string memberName, List<MemberInfo> complexPath)
        {
            if (!dictMemberNames.TryGetValue(baseMemberName, out var resolvedValues))
            {
                resolvedValues = new List<DictMemberInfo>();
                dictMemberNames[baseMemberName] = resolvedValues;
            }

            if (!resolvedValues.Any(r => r.JsonName == memberName))
            {
                resolvedValues.Add(new DictMemberInfo(memberName, complexPath));
            }
        }

        protected virtual ListInitExpression ResolveDictListInitMemberNames(IEnumerable<ElementInit> initializers, 
            Dictionary<string, List<DictMemberInfo>> dictMemberNames)
        {
            if (dictMemberNames?.Count > 0)
            {
                //manually adjust or expand the dictionary names
                var newDictItems = new List<ElementInit>();

                Action<string, Expression> addNewItem = (name, expr) =>
                    newDictItems.Add(Expression.ElementInit(DictAddMethod, Expression.Constant(name),
                    expr.Type != Defaults.ObjectType ? Expression.Convert(expr, Defaults.ObjectType) : expr));

                foreach (var item in initializers)
                {
                    var baseMemberName = item.Arguments[0].ExecuteExpression<object>().ToString();
                    var dictValueExpr = item.Arguments[1].Uncast(out var valCast, Defaults.ObjectType);

                    if (dictMemberNames.TryGetValue(baseMemberName, out var baseMemberValues))
                    {
                        if (baseMemberValues.Count > 1)
                        {
                            //complex type
                            //replace and extend
                            //but let dictValueExpr take precedence

                            //pick the first name before the first 'underscore'
                            var itemName = baseMemberValues.First().JsonName;
                            var sepIdx = itemName.IndexOf(Defaults.ComplexTypeNameSeparator);
                            var baseMemberJsonName = sepIdx > 0 ? itemName.Substring(0, sepIdx) : new string(itemName.ToCharArray());

                            ResolveDictionaryMemberAssignment(baseMemberName, ref baseMemberJsonName, ref dictValueExpr, dictMemberNames, out var newMembers,
                                out var newArgs, out var isMemberAccess, out var isComplexType, out var hasNfpEscape);

                            //add the new members
                            for (int i = 0, l = newMembers.Count; i < l; i++)
                            {
                                addNewItem(newMembers[i], newArgs[i]);
                            }

                            //foreach (var baseMemVal in baseMemberValues)
                            //{
                            //    //generate new dictionary value expression
                            //    var baseMemValExpr = dictValueExpr;

                            //    if (baseMemVal.Item2?.Count > 0)
                            //    {
                            //        //extend by members
                            //        foreach (var baseMemValMemInfo in baseMemVal.Item2.AsEnumerable().Reverse()) //LIFO
                            //        {
                            //            if (!baseMemValMemInfo.DeclaringType.IsGenericAssignableFrom(baseMemValExpr.Type))
                            //            {
                            //                //unexpected member. it belongs to a derived type not exposed to this expression
                            //                //break this loop
                            //                //don't add
                            //                baseMemValExpr = dictValueExpr;
                            //                break;
                            //            }

                            //            baseMemValExpr = Expression.MakeMemberAccess(baseMemValExpr, baseMemValMemInfo);
                            //        }

                            //        if (baseMemValExpr == dictValueExpr)
                            //            continue; //nothing happened, don't add
                            //    }

                            //    //add the new item
                            //    //retrieved value wins key
                            //    addNewItem(baseMemVal.Item1, baseMemValExpr);
                            //}
                            continue;
                        }
                        else if (baseMemberName != baseMemberValues.FirstOrDefault()?.JsonName)
                        {
                            //retrieved value wins key
                            addNewItem(baseMemberValues[0].JsonName, dictValueExpr);
                            continue;
                        }
                    }
                    else
                    {
                        AddToDictMemberName(dictMemberNames, baseMemberName, baseMemberName, null);
                    }

                    addNewItem(baseMemberName, dictValueExpr);
                }

                initializers = newDictItems;
            }

            return Expression.ListInit(Expression.New(DictType), initializers);
        }

        protected internal bool IsPredicateAssignmentValue(Expression expression)
        {
            return SetPredicateAssignments != null && SetPredicateAssignments.Values.Contains(expression);
        }
    }
}
