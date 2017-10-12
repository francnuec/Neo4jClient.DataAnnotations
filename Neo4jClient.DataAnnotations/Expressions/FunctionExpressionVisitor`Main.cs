using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public partial class FunctionExpressionVisitor : ExpressionVisitor
    {
        public FunctionExpressionVisitor(QueryUtilities queryUtilities, FunctionVisitorContext context = null)
        {
            QueryUtilities = queryUtilities;
            Context = context ?? new FunctionVisitorContext();
            Context.Visitor = this;
        }

        public FunctionVisitorContext Context { get; }

        public StringBuilder Builder { get; } = new StringBuilder();

        public QueryUtilities QueryUtilities { get; internal set; }

        public EntityResolver Resolver => QueryUtilities?.Resolver;

        public Func<object, string> Serializer => QueryUtilities?.SerializeCallback;

        protected List<Expression> IgnoredNodes { get; private set; } = new List<Expression>();
        protected List<Expression> UnhandledNodes { get; private set; } = new List<Expression>();

        private FunctionHandlerContext handlerTestContext = null;
        private List<Func<FunctionHandlerContext, Func<Expression>>> canHandleHandlers
                = new List<Func<FunctionHandlerContext, Func<Expression>>>();

        protected virtual void NotHandled(Expression node)
        {
            if (node != null && !UnhandledNodes.Contains(node))
            {
                UnhandledNodes.Add(node);
            }
        }

        protected virtual void Handled(Expression node)
        {
            if (node != null)
                UnhandledNodes.Remove(node);
        }

        public virtual void Ignore(Expression node)
        {
            if (node != null && !IgnoredNodes.Contains(node))
            {
                IgnoredNodes.Add(node);
            }

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
        }

        /// <summary>
        /// Don't call this method directly unless you wan't to force this process
        /// </summary>
        public virtual void ProcessUnhandledSimpleVars(Expression currentNode)
        {
            if (UnhandledNodes.Count == 0)
                return;

            //filter the nodes for only the onces of interest to us
            Func<Expression, bool> filter = (n) =>
            {
                switch (n.NodeType)
                {
                    case ExpressionType.MemberAccess:
                    case ExpressionType.TypeAs:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.Unbox:
                    case ExpressionType.Call when (n is MethodCallExpression callExpr
                    && ((callExpr.Method.Name.StartsWith("_As") 
                    && callExpr.Method.DeclaringType == Defaults.ObjectExtensionsType) //_As and _AsList()
                    || (callExpr.Method.Name == "Get" && callExpr.Method.DeclaringType == Defaults.VarsType) //Vars.Get()
                    || callExpr.Method.IsEquivalentTo(Defaults.CypherObjectIndexerInfo) //Vars.Get("")[""]
                    )):
                        {
                            return true;
                        }
                }

                return false;
            };

            List<Expression> _unhandledNodes = UnhandledNodes
                .AsEnumerable()
                .Reverse()
                .TakeWhile(filter)
                .ToList();

            if (_unhandledNodes.Count == 0)
                return;

            bool removeVariable = false;

            if (!Utilities.HasVars(_unhandledNodes))
            {
                if (!_unhandledNodes.Any(n => n.NodeType == ExpressionType.MemberAccess))
                    return; //we need at least one memberaccess for this to work

                //it doesn't have a vars get call, so add one
                var randomVar = "_fev_" + Utilities.GetRandomVariableFor("fevr");

                //create a Vars.Get call for this variable
                var varsGetCallExpr = Utilities.GetVarsGetExpressionFor(randomVar, currentNode.Type);

                _unhandledNodes.Insert(0, varsGetCallExpr); //this is just going to be the header

                removeVariable = true;
            }

            var value = Utilities.BuildSimpleVars(_unhandledNodes, QueryUtilities, useResolvedJsonName: Context.UseResolvedJsonName);

            if (string.IsNullOrWhiteSpace(value))
            {
                //error
                throw new InvalidOperationException(
                    string.Format(Messages.InvalidVariableExpressionError,
                    _unhandledNodes.Last().ToString(), value ?? ""));
            }

            if (removeVariable)
            {
                var dotIdx = value.IndexOf(".");
                dotIdx = dotIdx > 0 ? dotIdx : value.Length;
                if (dotIdx > 0)
                    value = value.Remove(0, dotIdx);
            }

            Builder.Append(value);

            foreach (var node in _unhandledNodes)
            {
                Handled(node);
            }
        }

        public virtual bool CanHandle(Expression node, out Func<Expression> handler)
        {
            return CanHandle(node, out handler, out var handlerContext);
        }

        protected virtual bool CanHandle(Expression node, out Func<Expression> handler, out FunctionHandlerContext context)
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
            {
                canHandleHandlers.AddRange(memberHandlers);
            }
            else if (node is MethodCallExpression methodNode
                && Context.MemberInfoHandlers.Where(mhp => mhp.Key.Name == methodNode.Method.Name)
                .Select(mhp => mhp.Value).ToArray() is var methodHandlers
                && methodHandlers?.Length > 0)
            {
                canHandleHandlers.AddRange(methodHandlers);
            }

            if (Context.NodeTypeHandlers.TryGetValue(node.NodeType, out var nodeTypeHandlers)
               && nodeTypeHandlers?.Count > 0)
            {
                canHandleHandlers.AddRange(nodeTypeHandlers);
            }

            var type = node.GetType();

            if (Context.TypeHandlers
                .GroupBy(tp => tp.Key == type ? 1 : (tp.Key.IsAssignableFrom(type) ? 2 : 0)) //prioritize exact type matches over base matches
                .Where(tpg => tpg.Key > 0)
                .SelectMany(tpg => tpg.SelectMany(tp => tp.Value)) is var typeHandlers
                && typeHandlers?.Count() > 0)
            {
                canHandleHandlers.AddRange(typeHandlers);
            }

            if (canHandleHandlers.Count > 0)
            {
                if (handlerTestContext == null)
                {
                    handlerTestContext = new FunctionHandlerContext()
                    {
                        VisitorContext = Context
                    };
                }

                handlerTestContext.Expression = node;

                handler = canHandleHandlers.Select(h => h.Invoke(handlerTestContext)).Where(f => f != null).FirstOrDefault();
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
        /// This will invoke the specified handler.
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

        public virtual bool WriteArgument(Expression argExpr, bool isRawValue = false, bool writeOnlySimpleValue = false)
        {
            bool isContinuous = false, hasSimpleValue = false;
            string argument = null;

            try
            {
                var _arg = argExpr.Uncast(out var cast);
                isRawValue = isRawValue || _arg.Type == typeof(Newtonsoft.Json.Linq.JRaw);

                var result = _arg.ExecuteExpression<object>();

                argument = Serializer(result);

                if (isRawValue
                    && _arg.Type != typeof(Newtonsoft.Json.Linq.JRaw)
                    && !string.IsNullOrWhiteSpace(argument)
                    && argument.StartsWith("\"")
                    && argument.EndsWith("\""))
                {
                    //remove the quotation
                    argument = argument.Substring(0, argument.Length - 1).Remove(0, 1);
                }

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
                var exprs = Utilities.GetSimpleMemberAccessStretch(argExpr, out var entity, out isContinuous);
            }

            if (!isContinuous)
                //cover it in brackets
                Builder.Append("(");

            if (hasSimpleValue)
            {
                if (!isRawValue //never use params for raw values.
                    && Context.BuildStrategy != PropertiesBuildStrategy.NoParams
                    && Context.CreateParameter != null)
                {
                    argument = Context.CreateParameter(argument);
                    argument = Utilities.GetNewNeo4jParameterSyntax(argument);
                }

                Builder.Append(argument);
                Handled(argExpr);
            }
            else
            {
                Visit(argExpr);
            }

            if (!isContinuous)
                Builder.Append(")");

            return true;
        }

        public virtual void WriteOperation(string neo4jOperator, Expression left, Expression operatorNode, Expression right)
        {
            WriteOperation(neo4jOperator, left, true, operatorNode, true, right);
        }

        public virtual void WriteOperation(string neo4jOperator, 
            Expression left, bool leftSpace, Expression operatorNode, bool rightSpace, Expression right)
        {
            Handled(operatorNode);

            if (left != null)
            {
                WriteArgument(left);

                if (leftSpace)
                    Builder.Append(" ");
            }

            Builder.Append(neo4jOperator);

            if (right != null)
            {
                if (rightSpace)
                    Builder.Append(" ");

                WriteArgument(right);
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

                NotHandled(node);

                var newNode = node;

                Func<Expression> continuation = () => base.Visit(node);

                if (CanHandle(node, out var handler, out var handlerContext))
                {
                    //invoke handler
                    newNode = InvokeHandler(handlerContext, continuation);
                }
                else
                {
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
                                if (WriteArgument(node, writeOnlySimpleValue: true))
                                {
                                    break;
                                }

                                goto default;
                            }
                        default:
                            {
                                newNode = continuation();
                                break;
                            }
                    }
                }

                Handled(node);
                ProcessUnhandledSimpleVars(node);

                return newNode;
            }

            return base.Visit(node);
        }
    }
}
