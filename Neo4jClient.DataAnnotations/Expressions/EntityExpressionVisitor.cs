using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Utils;

namespace Neo4jClient.DataAnnotations.Expressions
{
    //
    public class EntityExpressionVisitor : ExpressionVisitor, IHaveAnnotationsContext
    {
        private FunctionExpressionVisitor _funcsVisitor;


        public EntityExpressionVisitor(QueryContext queryContext, ParameterExpression sourceParameter = null)
        {
            QueryContext = queryContext ?? throw new ArgumentNullException(nameof(queryContext));
            SourceParameter = sourceParameter;
        }

        public Func<object, string> Serializer => QueryContext?.SerializeCallback;

        public EntityResolver Resolver => QueryContext?.Resolver;

        public QueryContext QueryContext { get; internal set; }

        public Dictionary<Expression, Expression> PredicateAssignments { get; private set; }

        public ParameterExpression SourceParameter { get; set; }

        protected FunctionExpressionVisitor FuncsVisitor
        {
            get
            {
                if (_funcsVisitor == null)
                    _funcsVisitor = new FunctionExpressionVisitor(QueryContext, new FunctionVisitorContext
                    {
                        //we most likely aint needing these
                        WriteConstants = false
                        //RewriteNullEqualityComparisons = false,
                        //WriteOperators = false,
                        //WriteParameters = false
                    });

                return _funcsVisitor;
            }
        }

        public Dictionary<EntityMemberInfo, object> PendingAssignments { get; set; }
            = new Dictionary<EntityMemberInfo, object>();

        public Expression Source { get; private set; }

        public Expression RootNode { get; private set; }

        public AnnotationsContext AnnotationsContext => QueryContext?.AnnotationsContext;

        public EntityService EntityService => AnnotationsContext?.EntityService;


        public override Expression Visit(Expression node)
        {
            if (Source == null && node != null)
                Source = node;

            var newNode = base.Visit(node);
            return newNode;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (RootNode == null)
                if (node.Type.IsAnonymousType())
                {
                    //convert anonymous object to dictionary items
                    var dictItems = GetNewExpressionItems(node);
                    //generate dictionary expression
                    var dictExpr = Expression.ListInit(Expression.New(Defaults.DictStringObjectType), dictItems);
                    var dictNode = VisitListInit(dictExpr);
                    return dictNode;
                }

            return base.VisitNew(node);
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            if (RootNode == null)
            {
                RootNode = node;
                node = ResolveDictListInitExpression(node);
                return node;
            }

            return base.VisitListInit(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            if (RootNode == null)
            {
                RootNode = node;
                var newBindings = InternalVisitBindings(node.Bindings, null, out var isNewResult);

                return Expression.MemberInit(node.NewExpression, newBindings);
            }

            return base.VisitMemberInit(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (RootNode == null &&
                (node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.Equal) &&
                SourceParameter != null)
            {
                //most likely predictate
                RootNode = node;

                var isAnonymousType = SourceParameter.Type.IsAnonymousType();
                var isDictionaryType = SourceParameter.Type.IsDictionaryType();

                //SetUsePredicateOnly = true;

                //sort out predicate
                Expression predicateExpression = null;

                //split the binary expressions
                var predicateVisitor = new PredicateExpressionVisitor
                {
                    MakeRightNodeReplacements = true,
                    RightNodeReplacements = new Dictionary<Expression, Expression>
                    {
                        {
                            SourceParameter,
                            ExpressionUtilities.GetVarsGetExpressionFor(SourceParameter.Name, SourceParameter.Type)
                        }
                    }
                };

                predicateVisitor.Visit(node);

                PredicateAssignments = predicateVisitor.Assignments;

                var predicateInfoDict = new Dictionary<EntityMemberInfo, Expression>();

                foreach (var assignment in PredicateAssignments)
                {
                    var keyExpr = assignment.Key;
                    var valueExpr = assignment.Value.UncastBox(out var castRemoved);

                    var retrieved = ExpressionUtilities.GetSimpleMemberAccessStretch
                        (EntityService, keyExpr, out var valExpr);

                    //build the entity member infos
                    EntityMemberInfo currentMember = null;
                    var currentType = valExpr.Type;
                    var foundRoot = false;

                    foreach (var item in retrieved)
                    {
                        if (item == valExpr)
                        {
                            foundRoot = true;
                            continue;
                        }

                        if (!foundRoot)
                            continue;

                        if (item is MemberExpression memberExpression)
                        {
                            currentMember = new EntityMemberInfo(EntityService, memberExpression.Member, currentMember,
                                !currentType.IsAnonymousType() ? currentType : null);
                        }
                        else if (isDictionaryType
                                 && item is MethodCallExpression methodCallExpr
                                 && methodCallExpr.Method is var methodInfo
                                 && !methodInfo.IsExtensionMethod()
                                 && methodCallExpr.Object is var methodObjectExpr
                                 && methodObjectExpr?.Type.IsDictionaryType() == true
                                 && methodObjectExpr.Type
                                     .GetProperties(
                                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                     .Where(p => p.GetIndexParameters().Any())
                                     .Select(p => p.GetGetMethod())
                                     .Contains(methodInfo))
                        {
                            var dictKey = methodCallExpr.Arguments[0].ExecuteExpression<object>().ToString();
                            currentMember = new EntityMemberInfo(EntityService, dictKey, valueExpr.Type, currentMember);
                        }
                        else if (currentMember != null &&
                                 currentMember.MemberFinalType.IsGenericAssignableFrom(item.Type))
                        {
                            currentMember.MemberFinalType = item.Type;
                        }

                        currentType = currentMember?.MemberFinalType ?? item.Type;
                    }

                    if (currentMember == null)
                        //throw error
                        throw new InvalidOperationException(string.Format(Messages.AmbiguousExpressionError, keyExpr));

                    predicateInfoDict[currentMember] = valueExpr;
                }

                var dictItems = new List<ElementInit>();

                //now convert the predicateinfodict to actual dictionary items
                foreach (var item in predicateInfoDict)
                {
                    var keyInfo = item.Key;
                    var valueExpr = item.Value;

                    if (!isDictionaryType && !isAnonymousType &&
                        (keyInfo.ReflectedType != null || keyInfo.HasComplexParent))
                    {
                        try
                        {
                            if (!ExpressionUtilities.IsSpecialNode(FuncsVisitor, valueExpr,
                                    out var actualValue, out var hasVars, out var hasFuncs, out var hasDummy))
                                //write the value straight to instance
                                valueExpr = Expression.Constant(actualValue, valueExpr?.Type);
                        }
                        catch (Exception e)
                        {
                            throw new InvalidOperationException(
                                string.Format(Messages.AmbiguousExpressionError, valueExpr), e);
                        }

                        keyInfo.ResolveNames(Resolver, Serializer);

                        //seal it with an nfp so the name is constant
                        valueExpr = Expression.Call(
                            Utils.Utilities.GetGenericMethodInfo(Defaults.NfpExtMethodInfo, valueExpr.Type), valueExpr);
                    }

                    AddDictionaryElementInit(dictItems, keyInfo, ref valueExpr, PendingAssignments);
                }

                //return a dictionary expression
                predicateExpression = Expression.ListInit(Expression.New(Defaults.DictStringObjectType), dictItems);
                return predicateExpression;
            }

            return base.VisitBinary(node);
        }


        protected IList<MemberBinding> InternalVisitBindings(IList<MemberBinding> bindings, EntityMemberInfo parent,
            out bool isNewResult)
        {
            isNewResult = false;
            var newBindings = new List<MemberBinding>(bindings);

            for (int i = 0, l = bindings.Count; i < l; i++)
            {
                var binding = bindings[i];
                var newBinding = binding;

                var currentMember = new EntityMemberInfo(EntityService, binding.Member, parent);

                switch (binding)
                {
                    case MemberAssignment assignment:
                        {
                            if (ExpressionUtilities.IsSpecialNode(FuncsVisitor, assignment.Expression,
                                out var value, out var hasVars, out var hasFunctions, out var hasDummyMethod))
                            {
                                currentMember.MemberFinalType = assignment.Expression.Type;

                                var expansions = ExpandComplexExpression(assignment.Expression, currentMember);

                                var memberType = currentMember.MemberFinalType;

                                if (!memberType.IsComplex())
                                    newBinding = Expression.Bind(assignment.Member,
                                        Expression.Constant(memberType.GetDefaultValue(),
                                            memberType));
                                else
                                    //complex properties shouldn't be left null
                                    newBinding = Expression.Bind(assignment.Member,
                                        Expression.Constant(Utils.Utilities.CreateInstance(memberType),
                                            memberType));

                                if (expansions?.Count > 0)
                                {
                                    foreach (var item in expansions) PendingAssignments[item.Key] = item.Value;

                                    currentMember = null;
                                }
                            }

                            break;
                        }
                    case MemberListBinding listBinding:
                        {
                            if (listBinding.Initializers.Any(init =>
                                init.Arguments.Any(a =>
                                    ExpressionUtilities.IsSpecialNode(FuncsVisitor, a,
                                        out var aValue, out var hasVars, out var hasFunctions, out var hasDummyMethod))))
                                newBinding = Expression.ListBind(listBinding.Member);

                            break;
                        }
                    case MemberMemberBinding memberMemberBinding:
                        {
                            var childBindings = InternalVisitBindings(memberMemberBinding.Bindings, currentMember,
                                out var isNewChildResult);
                            if (isNewChildResult)
                            {
                                newBinding = Expression.MemberBind(memberMemberBinding.Member, childBindings);
                                currentMember = null; //don't store for membermemberbinding
                            }

                            break;
                        }
                }

                if (binding != newBinding)
                {
                    newBindings[i] = newBinding;
                    isNewResult = isNewResult || true;

                    if (currentMember != null)
                        PendingAssignments[currentMember] = binding;
                }
            }

            return newBindings;
        }

        /// <summary>
        ///     Converts an anonymous new expression to dictionary items
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual List<ElementInit> GetNewExpressionItems(NewExpression node)
        {
            //transform the anonymous type expression to a dictionary expression
            var dictItems = new List<ElementInit>();

            for (var i = 0; i < node.Arguments.Count; i++)
            {
                //generate the expression entry
                var argument = node.Arguments[i];
                var member = node.Members[i].Name;

                if (argument != null && member != null)
                    dictItems.Add(Expression.ElementInit
                    (Defaults.DictStringObjectAddMethod, Expression.Constant(member),
                        argument.Type != Defaults.ObjectType
                            ? Expression.Convert(argument, Defaults.ObjectType)
                            : argument));
            }

            return dictItems;
        }

        protected List<(EntityMemberInfo Key, Expression Value)> ExpandComplexExpression(Expression expression,
            EntityMemberInfo parentInfo)
        {
            if (expression.Type.IsComplex())
            {
                var result = new List<(EntityMemberInfo Key, Expression Value)>();

                //it's complex so resolve in the possible best way.
                var expandedArgs = ExpressionUtilities.ExplodeComplexTypeMemberAccess
                    (EntityService, expression, out var argInversePaths);

                if (expandedArgs.Count > 0)
                {
                    //has complex members
                    //use the scalars instead
                    var memberInfoMaps
                        = new Dictionary<MemberInfo, EntityMemberInfo>();

                    for (int ai = 0, al = argInversePaths.Count; ai < al; ai++)
                    {
                        var inversePath = argInversePaths[ai];

                        //create the respective entity infos
                        inversePath.Reverse();

                        var lastInfo = parentInfo;

                        foreach (var item in inversePath)
                        {
                            if (!memberInfoMaps.TryGetValue(item, out var itemInfo))
                            {
                                itemInfo = new EntityMemberInfo(EntityService, item, lastInfo);
                                memberInfoMaps[item] = itemInfo;
                            }

                            lastInfo = itemInfo;
                        }

                        var arg = expandedArgs[ai];

                        result.Add((lastInfo, arg));
                        lastInfo.MemberFinalType = arg.Type;
                    }
                }

                return result;
            }

            return null;
        }

        protected virtual ListInitExpression ResolveDictListInitExpression(ListInitExpression node)
        {
            if (node.NewExpression.Type.IsDictionaryType())
                //generate dictionary expression
                node = GetDictListInitExpression(node.Initializers);

            return node;
        }

        protected virtual ListInitExpression GetDictListInitExpression(IEnumerable<ElementInit> initializers)
        {
            var dictMembers = new Dictionary<EntityMemberInfo, Expression>();

            //transform expression to a dictionary<string, object> expression
            var dictItems = new List<ElementInit>();

            foreach (var initializer in initializers)
            {
                //expand members if complex type
                //if there's a cast, remove it.
                var key = initializer.Arguments[0].Uncast(out var keyCastType);
                var baseMemberName = key.ExecuteExpression<object>().ToString();

                var initArgument = initializer.Arguments[1];
                var value = initArgument.UncastBox(out var argCastType);

                var baseMemberJsonName = new string(baseMemberName.ToCharArray());

                var baseMemberInfo = new EntityMemberInfo(EntityService, baseMemberName, value.Type);
                baseMemberInfo.JsonName = baseMemberJsonName;

                AddDictionaryElementInit(dictItems, baseMemberInfo, ref value, PendingAssignments);
            }

            //generate dictionary expression
            return Expression.ListInit(Expression.New(Defaults.DictStringObjectType), dictItems);
        }

        protected virtual void AddDictionaryElementInit
        (List<ElementInit> dictItems, EntityMemberInfo baseKeyMemberInfo, ref Expression valueExpr,
            Dictionary<EntityMemberInfo, object> pendingAssignments)
        {
            ResolveDictionaryMemberAssignment
            (baseKeyMemberInfo, ref valueExpr,
                out var members, out var arguments,
                out var isMemberAccess, out var isComplexType,
                out var hasNfpEscape);

            var baseMemberJsonName = baseKeyMemberInfo.ComplexJsonName;

            if (arguments?.Count > 0)
                //generate the expression entry
                for (int j = 0, jl = arguments.Count; j < jl; j++)
                {
                    var member = members[j];
                    var argument = arguments[j];

                    if (argument != null && member != null)
                    {
                        //uncast the box around the argument
                        argument = argument.UncastBox(out var argCast);

                        member.ResolveNames(AnnotationsContext.EntityResolver, Serializer);

                        if (ExpressionUtilities.IsSpecialNode(FuncsVisitor, argument,
                            out var aValue, out var hasVars, out var hasFunctions, out var hasDummyMethod))
                        {
                            //add the pending assignments
                            pendingAssignments[member] = argument;
                            //make argument default value
                            argument = Expression.Constant(argument.Type.GetDefaultValue(), argument.Type);
                        }

                        dictItems.Add(Expression.ElementInit
                        (Defaults.DictStringObjectAddMethod, Expression.Constant(member.ComplexJsonName),
                            argument.Type != Defaults.ObjectType
                                ? Expression.Convert(argument, Defaults.ObjectType)
                                : argument
                        ));
                    }
                }
        }

        protected virtual void ResolveDictionaryMemberAssignment
        (EntityMemberInfo baseKeyMemberInfo, ref Expression valuExpr,
            out List<EntityMemberInfo> members, out List<Expression> arguments,
            out bool isMemberAccess, out bool isComplexType, out bool hasNfpEscape)
        {
            members = new List<EntityMemberInfo> { baseKeyMemberInfo };
            arguments = new List<Expression> { valuExpr };

            isMemberAccess = false;

            hasNfpEscape = Utils.Utilities.HasNfpEscape(valuExpr);

            var valueType = valuExpr.Type;
            isComplexType = valueType.IsComplex();

            isMemberAccess = valuExpr.NodeType == ExpressionType.MemberAccess
                             && (valuExpr as MemberExpression).Member.Name ==
                             baseKeyMemberInfo.Name; //test first level first

            if (!isMemberAccess && valuExpr is UnaryExpression)
            {
                //try second level
                var memberExpr = valuExpr.Uncast(out var valCast) as MemberExpression;

                if (memberExpr != null)
                    //test and accept second level too
                    isMemberAccess = memberExpr.NodeType == ExpressionType.MemberAccess
                                     && memberExpr.Member.Name == baseKeyMemberInfo.Name;
            }

            if (isMemberAccess)
            {
                valuExpr = valuExpr.Cast(out var valueCast); //take advantage of the object passed where available.
                valueType = valuExpr.Type;
            }

            object entity = null;

            var memberComplexJsonName = baseKeyMemberInfo.ComplexName;

            if (!hasNfpEscape
                && (isMemberAccess || isComplexType)
                && !baseKeyMemberInfo.HasComplexParent
                && baseKeyMemberInfo.ReflectedType?.IsAnonymousType() !=
                false //that is, it's either an anonymous class or null
                )
                //this should force a serialization that would get our json name maps populated;
                memberComplexJsonName = GetMemberComplexJsonName(valuExpr, out entity, true);

            if (!hasNfpEscape && !string.IsNullOrWhiteSpace(memberComplexJsonName))
                baseKeyMemberInfo.ComplexJsonName = baseKeyMemberInfo.JsonName = memberComplexJsonName;
            //else //don't touch the name when it is escaped
            //{
            //    baseMemberInfo.ComplexJsonName = baseMemberInfo.JsonName = baseMemberInfo.ComplexName;
            //}

            if (isComplexType)
            {
                //we need the scalars
                members.Clear();
                arguments.Clear();

                var expansions = ExpandComplexExpression(valuExpr, baseKeyMemberInfo);

                if (expansions?.Count > 0)
                    foreach (var item in expansions)
                    {
                        members.Add(item.Key);
                        arguments.Add(item.Value);
                    }
            }
        }

        protected virtual string GetMemberComplexJsonName(Expression argument, out object entity,
            bool forceGetEntity = false)
        {
            string jsonName = null;
            entity = null;

            var retrievedExprs =
                ExpressionUtilities.GetSimpleMemberAccessStretch(EntityService, argument, out var entityExpr);

            if (retrievedExprs?.Count > 1)
            {
                entity = null;
                forceGetEntity = forceGetEntity || Resolver == null && Serializer != null;

                if (forceGetEntity && entity == null)
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

                //get the names
                var currentIndex =
                    ExpressionUtilities.GetFirstMemberAccessIndex(retrievedExprs, entityExpr,
                        out var firstAccess); //retrievedExprs.IndexOf(entityExpr) + 1;
                Type cast = null;
                var entityType = entity?.GetType() ??
                                 (currentIndex <
                                  retrievedExprs
                                      .Count //check if the next expression to entity expression was just a cast, and instead use the cast type as entity type
                                  && retrievedExprs[currentIndex]?.Uncast(out cast) == entityExpr
                                  && cast != null
                                     ? cast
                                     : entityExpr.Type);

                var memberJsonNames = ExpressionUtilities.GetEntityPathNames
                (EntityService, ref entity, ref entityType, retrievedExprs,
                    ref currentIndex, Resolver, Serializer,
                    out var entityMembers, out var lastType);

                jsonName = memberJsonNames?.LastOrDefault(); //we are only interested in the last member name.
            }

            return jsonName;
        }
    }
}