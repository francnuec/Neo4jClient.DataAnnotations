using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Utils;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public partial class FunctionExpressionVisitor : ExpressionVisitor, IHaveAnnotationsContext
    {
        private readonly List<Func<FunctionHandlerContext, Func<Expression>>> canHandleHandlers
            = new List<Func<FunctionHandlerContext, Func<Expression>>>();

        private FunctionHandlerContext handlerTestContext;

        //public StringBuilder Builder { get; } = new StringBuilder();

        private FunctionsVisitorBuilder mBuilder;

        public FunctionExpressionVisitor(QueryContext queryContext, FunctionVisitorContext context = null)
        {
            QueryContext = queryContext ?? throw new ArgumentNullException(nameof(queryContext));
            Context = context ?? new FunctionVisitorContext();
            Context.Visitor = this;
        }

        public FunctionVisitorContext Context { get; }

        public FunctionsVisitorBuilder Builder
        {
            get
            {
                if (mBuilder == null) mBuilder = new FunctionsVisitorBuilder(this);

                return mBuilder;
            }
        }

        public QueryContext QueryContext { get; internal set; }

        public EntityResolver Resolver => QueryContext?.Resolver;

        public Func<object, string> Serializer => QueryContext?.SerializeCallback;

        protected List<Expression> IgnoredNodes { get; } = new List<Expression>();
        protected List<Expression> UnhandledNodes { get; } = new List<Expression>();

        public AnnotationsContext AnnotationsContext => QueryContext.AnnotationsContext;

        public EntityService EntityService => AnnotationsContext.EntityService;

        protected virtual void NotHandled(Expression node)
        {
            if (node != null && !UnhandledNodes.Contains(node)) UnhandledNodes.Add(node);
        }

        protected virtual void Handled(Expression node)
        {
            if (node != null)
                UnhandledNodes.Remove(node);
        }

        public virtual void Ignore(Expression node)
        {
            if (node != null && !IgnoredNodes.Contains(node)) IgnoredNodes.Add(node);

            Handled(node);
        }

        public virtual void Allow(Expression node)
        {
            if (node != null)
                IgnoredNodes.Remove(node);

            //NotHandled(node);
        }

        public virtual void Clear()
        {
            IgnoredNodes.Clear();
            UnhandledNodes.Clear();
            Builder.Clear();
            Builder.Monitors.Clear();
        }

        /// <summary>
        ///     Don't call this method directly unless you wan't to force this process
        /// </summary>
        public virtual void ProcessUnhandledSimpleVars(Expression currentNode, bool considerInits = false)
        {
            if (UnhandledNodes.Count == 0)
                return;

            Expression initExpression = null;

            //filter the nodes for only the onces of interest to us
            Func<Expression, bool> filter = n =>
            {
                switch (n.NodeType)
                {
                    case ExpressionType.MemberAccess:
                    case ExpressionType.TypeAs:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.Unbox:
                    case ExpressionType.Call when n is MethodCallExpression callExpr
                                                  && (
                                                      callExpr.Method.Name.StartsWith("_As") &&
                                                      (callExpr.Method.DeclaringType == Defaults.CypherFuncsType
                                                       || callExpr.Method.DeclaringType ==
                                                       Defaults.CypherExtensionFuncsType) //._As and ._AsList()
                                                      || callExpr.Method.Name == "Get" &&
                                                      callExpr.Method.DeclaringType ==
                                                      Defaults.VarsType //CypherVariables.Get()
                                                      || callExpr.Method.IsEquivalentTo(
                                                          Defaults
                                                              .CypherObjectIndexerInfo) //CypherVariables.Get("")[""]
                                                                                        //|| (callExpr.Method.Name == "_" && callExpr.Method.DeclaringType == Defaults.ObjectExtensionsType) //._()
                                                  ):
                        {
                            return true;
                        }
                    case ExpressionType.MemberInit:
                    case ExpressionType.New:
                    case ExpressionType.ListInit when n.Type.IsDictionaryType():
                        {
                            if (considerInits && initExpression == null)
                            {
                                //we only need one type init
                                initExpression = n;
                                return true;
                            }

                            return false;
                        }
                }

                return false;
            };

            //repeatFilter:

            var _unhandledNodes = UnhandledNodes
                .AsEnumerable()
                .Reverse()
                .TakeWhile(filter)
                .ToList();

            if (_unhandledNodes.Count == 0) return;

            var hasMemberAccess = _unhandledNodes.Any(n => n.NodeType == ExpressionType.MemberAccess);

            JObject initJObject = null;
            JToken initJValue = null;

            var initExprIdx = -1;

            string builtValue = null;

            if (considerInits && initExpression != null)
            {
                //remove it from the list
                initExprIdx = _unhandledNodes.IndexOf(initExpression);
                if (initExprIdx >= 0)
                    _unhandledNodes.Remove(initExpression);

                if (!hasMemberAccess)
                    try
                    {
                        var executedValue = initExpression.ExecuteExpression<object>();
                        if (executedValue is JToken token) initJValue = token;
                    }
                    catch
                    {
                    }
            }

            if (initJValue == null)
            {
                var removeVariable = false;

                if (!Utils.Utilities.HasVars(_unhandledNodes))
                {
                    if (!hasMemberAccess) //!_unhandledNodes.Any(n => n.NodeType == ExpressionType.MemberAccess))
                        return; //we need at least one memberaccess for this to work

                    //it doesn't have a vars get call, so add one
                    var randomVar = "_fev_" + Utils.Utilities.GetRandomVariableFor("fevr");

                    //create a CypherVariables.Get call for this variable
                    var varsGetCallExpr = ExpressionUtilities.GetVarsGetExpressionFor(randomVar, currentNode.Type);

                    _unhandledNodes.Insert(0, varsGetCallExpr); //this is just going to be the header

                    removeVariable = true;
                }

                builtValue =
                    ExpressionUtilities.BuildSimpleVarsCall(_unhandledNodes, QueryContext, Context.UseResolvedJsonName);

                if (string.IsNullOrWhiteSpace(builtValue))
                    //error
                    throw new InvalidOperationException(
                        string.Format(Messages.InvalidVariableExpressionError,
                            _unhandledNodes.Last(), builtValue ?? ""));

                if (removeVariable)
                {
                    var dotIdx = builtValue.IndexOf(".");
                    dotIdx = dotIdx > 0 ? dotIdx : builtValue.Length;
                    if (dotIdx > 0)
                        builtValue = builtValue.Remove(0, dotIdx);

                    //remove the dummy vars get node added earlier
                    _unhandledNodes.RemoveAt(0);
                }

                if (considerInits && initExpression != null && initJObject == null && initJValue == null)
                {
                    //build the init independently
                    //but first check if we have a cached value already
                    if (!QueryContext.FuncsCachedJTokens.TryGetValue(initExpression, out var existingJToken)
                        || !(existingJToken is JObject existingJObject))
                    {
                        initJObject = CypherUtilities.GetFinalProperties
                            (Expression.Lambda(initExpression), QueryContext, out var hasFuncsInProps);

                        if (existingJToken == null)
                            //cache this for later use in same query
                            QueryContext.FuncsCachedJTokens[initExpression] = initJObject;
                    }
                    else
                    {
                        initJObject = existingJObject;
                    }
                }

                if (initJObject != null && initJObject.Count > 0)
                    try
                    {
                        initJValue = initJObject.SelectToken($"$.{builtValue.Trim().TrimStart('.')}");
                    }
                    catch (Exception e)
                    {
                    }
            }

            if (considerInits && initJValue == null)
                //we didn't get what we came here for
                return;

            if (considerInits && initExprIdx >= 0)
                //replace it
                _unhandledNodes.Insert(initExprIdx, initExpression);

            if (initJValue != null)
                WriteArgument(Expression.Constant(initJValue, initJValue.GetType()), _unhandledNodes.LastOrDefault());
            else
                Builder.Append(builtValue, _unhandledNodes.LastOrDefault());

            foreach (var node in _unhandledNodes) Handled(node);
        }

        public virtual bool CanHandle(Expression node, out Func<Expression> handler)
        {
            return CanHandle(node, out handler, out var handlerContext);
        }

        protected virtual bool CanHandle(Expression node, out Func<Expression> handler,
            out FunctionHandlerContext context)
        {
            handler = null;
            context = null;

            //start from the most specific to the least specific
            //so first check memberexpression
            //then methodcallexpression
            //then node types
            //then expression class types

            if (node is MemberExpression memberNode
                && Context.MemberInfoHandlers.Where(mhp => mhp.Key.Name == memberNode.Member.Name)
                    .Select(mhp => mhp.Value).ToArray() is var memberHandlers
                && memberHandlers?.Length > 0)
                canHandleHandlers.AddRange(memberHandlers);
            else if (node is MethodCallExpression methodNode
                     && Context.MemberInfoHandlers.Where(mhp => mhp.Key.Name == methodNode.Method.Name)
                         .Select(mhp => mhp.Value).ToArray() is var methodHandlers
                     && methodHandlers?.Length > 0)
                canHandleHandlers.AddRange(methodHandlers);

            if (Context.NodeTypeHandlers.TryGetValue(node.NodeType, out var nodeTypeHandlers)
                && nodeTypeHandlers?.Count > 0)
                canHandleHandlers.AddRange(nodeTypeHandlers);

            var type = node.GetType();

            if (Context.TypeHandlers
                    .GroupBy(tp =>
                        tp.Key == type ? 1 :
                        tp.Key.IsAssignableFrom(type) ? 2 : 0) //prioritize exact type matches over base matches
                    .Where(tpg => tpg.Key > 0)
                    .SelectMany(tpg => tpg.SelectMany(tp => tp.Value)) is var typeHandlers
                && typeHandlers?.Count() > 0)
                canHandleHandlers.AddRange(typeHandlers);

            if (canHandleHandlers.Count > 0)
            {
                if (handlerTestContext == null)
                    handlerTestContext = new FunctionHandlerContext
                    {
                        VisitorContext = Context
                    };

                handlerTestContext.Expression = node;

                handler = canHandleHandlers.Select(h => h.Invoke(handlerTestContext)).Where(f => f != null)
                    .FirstOrDefault();
            }

            var hasHandler = handler != null;

            if (hasHandler)
            {
                //retire testing context
                context = handlerTestContext;
                handlerTestContext = null;

                context.Visitor = this;
                context.Handler = handler;
            }
            else if (handlerTestContext != null)
            {
                handlerTestContext.Expression = null;
            }

            canHandleHandlers.Clear();

            return hasHandler;
        }

        /// <summary>
        ///     This will invoke the specified handler.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="node"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        protected virtual Expression InvokeHandler(FunctionHandlerContext handlerContext, Func<Expression> continuation)
        {
            handlerContext.Visitor = this;
            handlerContext.Continuation = continuation;

            var newNode = handlerContext.Handler.Invoke();
            return newNode;
        }

        public virtual bool WriteArgument(Expression argExpr, Expression callerNode, bool isRawValue = false,
            bool writeOnlySimpleValue = false)
        {
            bool isContinuous = false, hasSimpleValue = false;
            object argumentObj = null;

            Func<string> serializedArgumentGetter = null;

            try
            {
                var _arg = argExpr.Uncast(out var cast);
                isRawValue = isRawValue || _arg.Type == typeof(JRaw);

                argumentObj = _arg.ExecuteExpression<object>();

                serializedArgumentGetter = () =>
                {
                    var _argument = Serializer(argumentObj);

                    if (isRawValue
                        && _arg.Type != typeof(JRaw)
                        && !string.IsNullOrWhiteSpace(_argument)
                        && _argument.StartsWith("\"")
                        && _argument.EndsWith("\""))
                        //remove the quotation
                        _argument = _argument.Substring(0, _argument.Length - 1).Remove(0, 1);

                    return _argument;
                };

                isContinuous = hasSimpleValue = true;
            }
            catch
            {
            }

            if (writeOnlySimpleValue && !hasSimpleValue)
                return false;

            if (!hasSimpleValue)
            {
                //maybe a vars call
                var exprs = ExpressionUtilities.GetSimpleMemberAccessStretch(EntityService, argExpr, out var entity,
                    out isContinuous);
            }

            if (!isContinuous)
                //cover it in brackets
                Builder.Append("(", callerNode ?? argExpr);

            if (hasSimpleValue)
            {
                string argument = null;
                if (argumentObj == null) argument = serializedArgumentGetter();

                if (!isRawValue //never use params for raw values.
                    && Context.BuildStrategy != PropertiesBuildStrategy.NoParams
                    && Context.CreateParameter != null)
                {
                    argument = Context.CreateParameter(argumentObj ?? argument);
                    argument = Utils.Utilities.GetNewNeo4jParameterSyntax(argument);
                }
                else
                {
                    if (argument == null)
                        argument = serializedArgumentGetter();
                }

                Builder.Append(argument, callerNode ?? argExpr);
                Handled(argExpr);
            }
            else
            {
                Visit(argExpr);
            }

            if (!isContinuous)
                Builder.Append(")", callerNode ?? argExpr);

            return true;
        }

        public virtual void WriteOperation(string neo4jOperator, Expression left,
            Expression operatorNode, Expression right, Expression callerNode)
        {
            WriteOperation(neo4jOperator, left, true, operatorNode, true, right, callerNode);
        }

        public virtual void WriteOperation(string neo4jOperator,
            Expression left, bool leftSpace,
            Expression operatorNode,
            bool rightSpace, Expression right,
            Expression callerNode)
        {
            Handled(operatorNode);

            if (left != null)
            {
                WriteArgument(left, callerNode);

                if (leftSpace)
                    Builder.Append(" ", callerNode);
            }

            Builder.Append(neo4jOperator, callerNode);

            if (right != null)
            {
                if (rightSpace)
                    Builder.Append(" ", callerNode);

                WriteArgument(right, callerNode);
            }
        }

        public override Expression Visit(Expression node)
        {
            if (node != null)
            {
                if (IgnoredNodes.Contains(node))
                {
                    //we only ignore once to ensure that same node used in unrelated expressions will be parsed
                    IgnoredNodes.Remove(node);
                    return node;
                }

                if (QueryContext.FuncsCachedBuilds.TryGetValue(node, out var visitResult)
                    && !string.IsNullOrEmpty(visitResult.Build))
                {
                    //this has been executed before so don't do it again
                    Builder.Append(visitResult.Build, node);
                    return visitResult.NewNode;
                }

                var nodeBuilder = new StringBuilder();
                var nodeMonitor = new FunctionsVisitorBuilder.Monitor
                {
                    Node = node,
                    DidAppend = (builder, caller, value) =>
                    {
                        if (value is null)
                            nodeBuilder.Append((object)null);
                        else
                            nodeBuilder.Append(value);
                    },
                    DidClearAll = builder => { nodeBuilder.Clear(); }
                };

                Builder.Monitors.Add(nodeMonitor);

                NotHandled(node);

                var newNode = node;

                Func<Expression> continuation = () => base.Visit(node);

                if (CanHandle(node, out var handler, out var handlerContext))
                    //invoke handler
                    newNode = InvokeHandler(handlerContext, continuation);
                else
                    switch (node.NodeType)
                    {
                        case ExpressionType.MemberAccess:
                        case ExpressionType.TypeAs:
                        case ExpressionType.Convert:
                        case ExpressionType.ConvertChecked:
                        case ExpressionType.Unbox:
                        case ExpressionType.Call:
                            {
                                //try to see if it can be executed to write a simple value first & write this value is all okay
                                if (WriteArgument(node, node, writeOnlySimpleValue: true)) break;

                                goto default;
                            }
                        default:
                            {
                                newNode = continuation();
                                break;
                            }
                    }

                Handled(node);

                //remove the monitor and cache results
                Builder.Monitors.Remove(nodeMonitor);

                //cache the results for this query
                var nodeBuild = nodeBuilder.ToString();
                if (!string.IsNullOrEmpty(nodeBuild))
                    lock (QueryContext.FuncsCachedBuilds)
                    {
                        QueryContext.FuncsCachedBuilds[node] = (newNode, nodeBuild);
                    }

                ProcessUnhandledSimpleVars(node);

                return newNode;
            }

            return base.Visit(node);
        }
    }
}