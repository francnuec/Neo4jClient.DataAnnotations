using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using Neo4jClient.Serialization;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.DataAnnotations
{
    //
    public class EntityExpressionVisitor : ExpressionVisitor
    {
        private static Type dictType = typeof(Dictionary<string, object>);

        private static MethodInfo dictAddMethod = Utilities.GetMethodInfo(() => new Dictionary<string, object>().Add(null, null));

        private Dictionary<string, List<Tuple<string, List<MemberInfo>>>> dictMemberNames;


        private List<Expression> paramNodes = new List<Expression>();

        private List<MethodCallExpression> paramMarkers = new List<MethodCallExpression>();

        public List<List<Expression>> Params { get; } = new List<List<Expression>>();

        public List<List<object>> ParamsPaths { get; } = new List<List<object>>();

        public Func<object, string> Serializer { get; private set; }

        public Expression RootNode { get; private set; }


        public MethodCallExpression WithNode { get; private set; }

        public Expression WithInstanceNode { get; private set; }

        public Expression WithPredicateNode { get; private set; }

        public Dictionary<Expression, Expression> WithPredicateAssignments { get; private set; }

        public Dictionary<MemberInfo, Expression> WithPredicateMemberAssignments { get; private set; }

        public Dictionary<string, Expression> WithPredicateDictionaryItems { get; private set; }

        public List<object> WithPredicateBindings { get; private set; }

        public ParameterExpression WithPredicateParameter { get; private set; }

        public bool WithUsePredicateOnly { get; private set; }


        public EntityExpressionVisitor(Func<object, string> serializer)
        {
            this.Serializer = serializer;
        }


        public override Expression Visit(Expression node)
        {
            bool assignRootNode = false;

            if (RootNode == null)
            {
                RootNode = node;
                assignRootNode = true;
            }

            List<Expression> filtered = null;

            if ((filtered = Utilities.GetSimpleMemberAccessStretch(node, out var filteredVal)) != null
                && Utilities.HasParams(filtered))
            {
                //found our params call.
                //be careful to check first if we have encountered this params before
                int markerIndex = paramNodes.IndexOf(node);
                MethodCallExpression getParamsExpr = null;

                if (markerIndex < 0)
                {
                    //store
                    Params.Add(filtered);
                    paramNodes.Add(node);

                    markerIndex = Params.Count - 1;

                    //replace with marker
                    getParamsExpr = Expression.Call(typeof(Utilities), "GetParams", new[] { node.Type }, Expression.Constant(markerIndex));
                    paramMarkers.Add(getParamsExpr);
                }
                else
                {
                    getParamsExpr = paramMarkers[markerIndex];
                }

                ParamsPaths.Add(new List<object>() { getParamsExpr });

                return getParamsExpr;
            }

            node = base.Visit(node);

            if (assignRootNode)
                RootNode = node;

            return node;
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
                        var dictExpr = Expression.ListInit(Expression.New(dictType), dictItems);
                        return VisitListInit(dictExpr);
                    }
                }
            }

            var index = ParamsPaths.Count; //do this to know which Paths to add to

            var newNode = base.VisitNew(node);

            if (newNode.Type.IsAnonymousType())
            {
                AddToParamsPaths(newNode, index);
            }

            return newNode;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitNewArray(node);
            AddToParamsPaths(newNode, index);
            return newNode;
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            node = ResolveDictListInitExpression(node, storeMemberNames: dictMemberNames == null);

            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitListInit(node);
            AddToParamsPaths(newNode, index);

            return newNode;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitMemberInit(node);
            AddToParamsPaths(newNode, index);
            return newNode;
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitElementInit(node);
            AddToParamsPaths(newNode, index);
            return newNode;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitMemberAssignment(node);
            AddToParamsPaths(newNode, index);
            return newNode;
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitMemberListBinding(node);
            AddToParamsPaths(newNode, index);
            return newNode;
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitMemberMemberBinding(node);
            AddToParamsPaths(newNode, index);
            return newNode;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (WithNode == null && Utilities.HasWith(node))
            {
                WithNode = node;

                bool isAnonymousType = node.Type.IsAnonymousType();
                bool isDictionaryType = node.Type.IsDictionaryType();

                //Build the entity expression
                var instanceExpr = node.Arguments[0];

                if (node.Arguments.Count == 3)
                {
                    //variant method
                    WithUsePredicateOnly = node.Arguments[2].ExecuteExpression<bool>();
                }

                var predicate = node.Arguments[1].ExecuteExpression<LambdaExpression>();

                Expression predicateExpression = null;

                if (predicate != null && predicate.Body is BinaryExpression)
                {
                    //split the binary expressions
                    var predicateVisitor = new PredicateExpressionVisitor();
                    predicateVisitor.Visit(predicate.Body);

                    WithPredicateAssignments = predicateVisitor.Assignments;

                    if (WithPredicateAssignments.Count > 0)
                    {
                        WithPredicateParameter = predicate.Parameters[0];

                        if (!isDictionaryType)
                        {
                            WithPredicateMemberAssignments = GetPredicateMemberAssignments
                                (WithPredicateAssignments, out var rootMembers, out var memberChildren);

                            WithPredicateBindings = GetPathBindings(rootMembers.Where(m => m != null), memberChildren, WithPredicateMemberAssignments);
                        }
                        else
                        {
                            WithPredicateDictionaryItems = new Dictionary<string, Expression>();

                            foreach (var assignment in WithPredicateAssignments)
                            {
                                var retrieved = Utilities.GetSimpleMemberAccessStretch(assignment.Key, out var valExpr);
                                var dictKey = (retrieved[1] as MethodCallExpression).Arguments[0].ExecuteExpression<object>().ToString();
                                WithPredicateDictionaryItems[dictKey] = assignment.Value;
                            }
                        }

                        //all sorted. lets generate the expression
                        predicateExpression = GetPredicateExpression(WithPredicateParameter, WithPredicateBindings,
                            WithPredicateMemberAssignments, WithPredicateDictionaryItems);
                    }
                }

                //do visits
                if (RootNode == node)
                {
                    //try to exclude 'with' from the normal process.
                    //so that if the first argument is an anonymous type, it will continue accordingly
                    RootNode = instanceExpr;
                }

                //visit instance first
                WithInstanceNode = Visit(instanceExpr);

                if (WithUsePredicateOnly)
                {
                    //reassign or clear everything
                    //root node would change now
                    RootNode = predicateExpression;

                    //clear all params, we don't need them again.
                    Params.Clear();
                    ParamsPaths.Clear();
                    paramNodes.Clear();
                    paramMarkers.Clear();
                    dictMemberNames?.Clear();
                }
                else if(isAnonymousType || isDictionaryType)
                {
                    var dictItems = (predicateExpression as ListInitExpression)?.Initializers.AsEnumerable();

                    if (isAnonymousType)
                    {
                        //generate the dictionary items for the anonymous types
                        dictItems = GetNewExpressionItems(predicateExpression as NewExpression);
                    }

                    predicateExpression = ResolveDictListInitMemberNames(dictItems, dictMemberNames);
                }

                WithPredicateNode = Visit(predicateExpression);

                return WithUsePredicateOnly ? WithPredicateNode : WithInstanceNode;
            }

            return base.VisitMethodCall(node);
        }


        private void AddToParamsPaths(object node, int index)
        {
            for (int i = index, l = ParamsPaths.Count; i < l; i++)
            {
                ParamsPaths[i].Add(node);
            }
        }

        private List<ElementInit> GetNewExpressionItems(NewExpression node)
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
                        (dictAddMethod, Expression.Constant(member),
                        argument.Type != Defaults.ObjectType ?
                        Expression.Convert(argument, Defaults.ObjectType) :
                        argument
                        ));
                }
            }

            return dictItems;
        }

        private string GetMemberName(Expression argument)
        {
            string name = null;

            var retrievedExprs = Utilities.GetSimpleMemberAccessStretch(argument, out var argVal);

            //get the entity object
            var entityExpr = argVal;

            object entity = null;

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
                    entity = Activator.CreateInstance(entityExpr.Type);
                }
                catch
                {

                }
            }

            if (entity != null)
            {
                //get the names
                var currentIndex = retrievedExprs.IndexOf(argVal) + 1;
                var memberNames = Utilities.GetEntityPathNames
                    (entity, retrievedExprs, ref currentIndex, Serializer,
                    out var entityMembers, out var lastType, useResolvedJsonName: true);

                name = memberNames?.LastOrDefault(); //we are only interested in the last member name.
            }

            return name;
        }

        private List<object> GetPathBindings(IEnumerable<MemberInfo> members,
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

                bool isWritable = propInfo?.CanWrite == true || fieldInfo != null;
                bool hasChildren = children?.Count > 0;

                Type type = value.Item1 ?? propInfo?.PropertyType ?? fieldInfo?.FieldType;

                object binding = null;
                List<object> childBindings = null;
                IEnumerable<MemberBinding> filteredChildBindings = null;

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
                        var expr = !hasChildren ? memberAssignments[member] : Expression.MemberInit
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
                            hasChildren ? (object)filteredChildBindings.ToArray() : memberAssignments[member]));
                    }
                }

                bindings.Add(binding);
            }

            return bindings;
        }

        private Dictionary<MemberInfo, Expression> GetPredicateMemberAssignments(Dictionary<Expression, Expression> assignments,
            out List<MemberInfo> rootMembers,
            out Dictionary<MemberInfo, Tuple<Type, List<MemberInfo>>> memberChildren)
        {
            var memberAssignments = new Dictionary<MemberInfo, Expression>();

            memberChildren = new Dictionary<MemberInfo, Tuple<Type, List<MemberInfo>>>();

            rootMembers = new List<MemberInfo>();

            foreach (var assignment in assignments)
            {
                var assignmentExprs = Utilities.GetSimpleMemberAccessStretch(assignment.Key, out var entityVal);

                var index = assignmentExprs.IndexOf(entityVal) + 1;
                Utilities.TraverseEntityPath(null, assignmentExprs, ref index,
                    out var assignmentLastType, out var assignmentPaths, buildPath: false);

                memberAssignments[assignmentPaths.Last().Key] = assignment.Value;

                //sort members and children
                var members = assignmentPaths.OrderBy(p => p.Value.Item1).Select(p => p.Key).ToList();

                rootMembers.Add(members.FirstOrDefault());

                foreach (var member in members)
                {
                    if (!memberChildren.TryGetValue(member, out var memberVal))
                    {
                        memberVal = new Tuple<Type, List<MemberInfo>>(assignmentPaths[member].Item2, new List<MemberInfo>());
                        memberChildren[member] = memberVal;
                    }

                    var child = members.Skip(members.IndexOf(member) + 1).FirstOrDefault();

                    if (child != null)
                        memberVal.Item2.Add(child);
                }
            }

            return memberAssignments;
        }

        private Expression GetPredicateExpression(
            ParameterExpression parameter, List<object> bindings,
            Dictionary<MemberInfo, Expression> memberAssignments,
            Dictionary<string, Expression> dictionaryItems)
        {
            NewExpression newExpr = null;
            Expression predicateExpression = null;

            if (parameter.Type.IsAnonymousType())
            {
                //anonymous type
                var constructor = parameter.Type.GetConstructors()[0];

                //build the argument list from the anonymous type delcared properties.
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
                newExpr = Expression.New(dictType);
                predicateExpression = Expression.ListInit(newExpr,
                    dictionaryItems.Select(item =>
                    Expression.ElementInit(dictAddMethod, Expression.Constant(item.Key), 
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

        private ListInitExpression ResolveDictListInitExpression(ListInitExpression node, bool storeMemberNames)
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
                            dictMemberNames.Add(item.Key, item.Value);
                        }
                    }
                }
            }

            return node;
        }

        private ListInitExpression GetDictListInitExpression(IEnumerable<ElementInit> initializers, 
            out Dictionary<string, List<Tuple<string, List<MemberInfo>>>> dictMemberNames)
        {
            dictMemberNames = new Dictionary<string, List<Tuple<string, List<MemberInfo>>>>();

            //transform expression to a dictionary<string, object> expression
            List<ElementInit> dictItems = new List<ElementInit>();

            foreach(var initializer in initializers)
            {
                //expand members if complex type
                //if there's a cast, remove it.
                var key = initializer.Arguments[0].Uncast(out var keyCastType);
                var baseMember = key.ExecuteExpression<object>().ToString();
                var members = new List<string>() { baseMember };

                var value = initializer.Arguments[1].Uncast(out var argCastType);
                var arguments = new List<Expression>() { value };

                bool hasNfpEscape = Utilities.HasNfpEscape(value);

                if (!hasNfpEscape)
                {
                    bool originallyMemberAccess = value.NodeType == ExpressionType.MemberAccess
                        && (value as MemberExpression).Member.Name == baseMember; //test first level first

                    if (!originallyMemberAccess && value is UnaryExpression)
                    {
                        //try second level
                        var memberExpr = value.Uncast(out var valCast) as MemberExpression;

                        if(memberExpr != null)
                        {
                            //test and accept second level too
                            originallyMemberAccess = memberExpr.NodeType == ExpressionType.MemberAccess
                                && memberExpr.Member.Name == baseMember;
                        }
                    }

                    bool isComplexType = false;

                    //check if its complex and resolve in the possible best way.
                    var expandedArgs = Utilities.ExpandComplexTypeAccess(value, out var argInversePaths);

                    if (expandedArgs.Count > 0)
                    {
                        //has complex members
                        //use the scalars instead
                        arguments = expandedArgs;
                        isComplexType = true;
                    }

                    members.Clear();

                    for (int j = 0, jl = arguments.Count; j < jl; j++)
                    {
                        var argument = arguments[j];

                        string memberName = null;

                        if (originallyMemberAccess)
                        {
                            //try find the actual json name
                            var newMemberName = GetMemberName(argument);
                            memberName = !string.IsNullOrWhiteSpace(newMemberName) ? newMemberName : null;
                        }

                        if (memberName == null)
                        {
                            if (isComplexType)
                            {
                                //use appended name if complex type
                                memberName = $"{baseMember}_{argInversePaths[j].AsEnumerable().Reverse().Select(m => m.Name).Aggregate((first, second) => $"{first}_{second}")}";
                            }
                            else
                            {
                                //just use base name
                                memberName = baseMember;
                            }
                        }

                        members.Add(memberName);

                        if (!dictMemberNames.TryGetValue(baseMember, out var resolvedValues))
                        {
                            resolvedValues = new List<Tuple<string, List<MemberInfo>>>();
                            dictMemberNames[baseMember] = resolvedValues;
                        }

                        resolvedValues.Add(new Tuple<string, List<MemberInfo>>(memberName, isComplexType ? argInversePaths[j] : null));
                    }
                }

                //generate the expression entry
                for (int j = 0, jl = arguments.Count; j < jl; j++)
                {
                    var argument = arguments[j];
                    var member = members[j];

                    if (argument != null && member != null)
                    {
                        dictItems.Add(Expression.ElementInit
                            (dictAddMethod, Expression.Constant(member),
                            argument.Type != Defaults.ObjectType ?
                                Expression.Convert(argument, Defaults.ObjectType) :
                                argument
                            ));
                    }
                }
            }

            //generate dictionary expression
            return Expression.ListInit(Expression.New(dictType), dictItems);
        }

        private ListInitExpression ResolveDictListInitMemberNames(IEnumerable<ElementInit> initializers, 
            Dictionary<string, List<Tuple<string, List<MemberInfo>>>> dictMemberNames)
        {
            if (dictMemberNames?.Count > 0)
            {
                //manually adjust or expand the dictionary names
                var newDictItems = new List<ElementInit>();

                Action<string, Expression> addNewItem = (name, expr) =>
                    newDictItems.Add(Expression.ElementInit(dictAddMethod, Expression.Constant(name),
                    expr.Type != Defaults.ObjectType ? Expression.Convert(expr, Defaults.ObjectType) : expr));

                foreach (var item in initializers) // (int i = 0, l = dictItems.Count; i < l; i++)
                {
                    var memberName = item.Arguments[0].Uncast(out var memCast).ExecuteExpression<object>().ToString();
                    var dictValueExpr = item.Arguments[1].Uncast(out var valCast);

                    if (dictMemberNames.TryGetValue(memberName, out var baseMemberValues))
                    {
                        if (baseMemberValues.Count > 1)
                        {
                            //maybe a complex type
                            //replace and extend
                            foreach (var baseMemVal in baseMemberValues)
                            {
                                //generate new dictionary value expression
                                var baseMemValExpr = dictValueExpr;

                                if (baseMemVal.Item2?.Count > 0)
                                {
                                    //extend by members
                                    foreach (var baseMemValMemInfo in baseMemVal.Item2.AsEnumerable().Reverse()) //LIFO
                                    {
                                        baseMemValExpr = Expression.PropertyOrField(baseMemValExpr, baseMemValMemInfo.Name);
                                    }
                                }

                                //add the new item
                                //retrieved value wins key
                                addNewItem(baseMemVal.Item1, baseMemValExpr);
                            }
                        }
                        else if (memberName != baseMemberValues.FirstOrDefault()?.Item1)
                        {
                            //retrieved value wins key
                            addNewItem(baseMemberValues[0].Item1, dictValueExpr);
                        }
                        else
                        {
                            addNewItem(memberName, dictValueExpr);
                        }
                    }
                    else
                    {
                        addNewItem(memberName, dictValueExpr);
                    }
                }

                initializers = newDictItems;
            }

            return Expression.ListInit(Expression.New(dictType), initializers);
        }
    }
}
