using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Neo4jClient.DataAnnotations.Cypher.Functions;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public static class FunctionHandlers
    {
        public static Expression HandleArrayIndexValue(Expression indexExpr, FunctionHandlerContext context)
        {
            context.Visitor.Ignore(indexExpr); //to avoid it being processed by the visitor again
            var expr = context.Continuation();

            //write the index
            context.Visitor.Builder.Append("[", context.Expression);
            context.Visitor.Allow(indexExpr);
            context.Visitor.WriteArgument(indexExpr, context.Expression);
            context.Visitor.Builder.Append("]", context.Expression);

            return expr;
        }

        public static Expression HandleGenericMethodBody(string cypherMethod, FunctionHandlerContext context)
        {
            return HandleGenericMethodBody(cypherMethod, context.Expression, context, (e, c) => c.Continuation());
        }

        public static Expression HandleGenericMethodBody<TExpr>
            (string cypherMethod, TExpr TExpression, FunctionHandlerContext context, Func<TExpr, FunctionHandlerContext, Expression> bodyHandler)
            where TExpr : Expression
        {
            context.Visitor.Builder.Append($"{cypherMethod}(", context.Expression);
            var ret = bodyHandler(TExpression, context);
            context.Visitor.Builder.Append(")", context.Expression);
            return ret;
        }

        public static Expression HandleArray(IEnumerable<Expression> valueExprs, FunctionHandlerContext context)
        {
            //write the array
            context.Visitor.Builder.Append("[", context.Expression);

            bool isFirst = true;

            foreach (var valueExpr in valueExprs)
            {
                if (!isFirst)
                {
                    context.Visitor.Builder.Append(", ", context.Expression);
                }
                else isFirst = false;

                context.Visitor.WriteArgument(valueExpr, context.Expression);
            }

            context.Visitor.Builder.Append("]", context.Expression);

            return context.Expression;
        }

        #region Misc
        public static Func<Expression> GetVariable(FunctionHandlerContext context)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == "Get"
                && methodInfo.DeclaringType == Defaults.VarsType
                && !methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    context.Visitor.ProcessUnhandledSimpleVars(null);
                    return context.Expression;
                };
            }

            return null;
        }

        public static Func<Expression> CypherObjectIndexer(FunctionHandlerContext context)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.IsEquivalentTo(Defaults.CypherObjectIndexerInfo)
                && !methodInfo.IsExtensionMethod()
                && methodCallExpr.Object is MethodCallExpression varsGetCallExpr
                && varsGetCallExpr.Method is var varsGetMethodInfo
                && varsGetMethodInfo.Name == "Get"
                && varsGetMethodInfo.DeclaringType == Defaults.VarsType
                && !varsGetMethodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    foreach (var arg in methodCallExpr.Arguments)
                        context.Visitor.Ignore(arg);

                    return context.Continuation(); //so that the inner get call can finish the job
                };
            }

            return null;
        }

        /// <summary>
        /// Rewrites the Parameter Expression as a CypherVariables.Get method call.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> ParameterNodeType(FunctionHandlerContext context)
        {
            if (context.VisitorContext.WriteParameters
                && context.Expression.NodeType == ExpressionType.Parameter
                && context.Expression is ParameterExpression node
                && !string.IsNullOrWhiteSpace(node.Name))
            {
                return () =>
                {
                    //convert this to a vars call
                    var varsGetCallExpr = ExpressionUtilities.GetVarsGetExpressionFor(node.Name, node.Type);

                    //trigger the vars write
                    context.Visitor.Ignore(node);
                    var newExpr = context.Visitor.Visit(varsGetCallExpr);
                    context.Visitor.Allow(node);

                    return newExpr;
                };
            }

            return null;
        }

        /// <summary>
        /// Writes a constant.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> ConstantNodeType(FunctionHandlerContext context)
        {
            if (context.VisitorContext.WriteConstants
                && context.Expression.NodeType == ExpressionType.Constant
                && context.Expression is ConstantExpression node)
            {
                return () =>
                {
                    context.Visitor.WriteArgument(context.Expression, node);
                    return context.Expression;
                };
            }

            return null;
        }

        /// <summary>
        /// Rewrites the Equal Node Type with Null Operand as IS NULL.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> NullOperandEqualNodeType(FunctionHandlerContext context)
        {
            if (context.VisitorContext.WriteOperators
                && context.VisitorContext.RewriteNullEqualityComparisons
                && context.Expression.NodeType == ExpressionType.Equal
                && context.Expression is BinaryExpression node
                && (node.Left.Uncast(out var leftCast) as ConstantExpression ?? 
                node.Right.Uncast(out var rightCast) as ConstantExpression) is var nullNode
                && nullNode != null
                && nullNode.Value == null)
            {
                return () =>
                {
                    //found our node
                    //== should become IS NULL
                    //!= should become IS NOT NULL

                    Expression operandNode = node.Left != nullNode ? node.Left : node.Right;

                    var genericMethodInfo = Utils.Utilities.GetGenericMethodInfo(Defaults.IsNullExtMethodInfo, operandNode.Type);
                    MethodCallExpression methodCall = Expression.Call(genericMethodInfo, operandNode);

                    //trigger the IS NULL write
                    context.Visitor.Ignore(node);
                    var newExpr = context.Visitor.Visit(methodCall);
                    context.Visitor.Allow(node);

                    return newExpr;
                };
            }

            return null;
        }

        /// <summary>
        /// Rewrites the NotEqual Node Type with Null Operand as IS NOT NULL.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> NullOperandNotEqualNodeType(FunctionHandlerContext context)
        {
            if (context.VisitorContext.WriteOperators
                && context.VisitorContext.RewriteNullEqualityComparisons
                && context.Expression.NodeType == ExpressionType.NotEqual
                && context.Expression is BinaryExpression node
                && (node.Left.Uncast(out var leftCast) as ConstantExpression ??
                node.Right.Uncast(out var rightCast) as ConstantExpression) is var nullNode
                && nullNode != null
                && nullNode.Value == null)
            {
                return () =>
                {
                    //found our node
                    //== should become IS NULL
                    //!= should become IS NOT NULL

                    Expression operandNode = node.Left != nullNode ? node.Left : node.Right;

                    var genericMethodInfo = Utils.Utilities.GetGenericMethodInfo(Defaults.IsNotNullExtMethodInfo, operandNode.Type);
                    MethodCallExpression methodCall = Expression.Call(genericMethodInfo, operandNode);

                    //trigger the IS NOT NULL write
                    context.Visitor.Ignore(node);
                    var newExpr = context.Visitor.Visit(methodCall);
                    context.Visitor.Allow(node);

                    return newExpr;
                };
            }

            return null;
        }

        public static Func<Expression> MathPower(FunctionHandlerContext context)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == "Pow"
                && methodInfo.DeclaringType == Defaults.MathType
                && !methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    context.Visitor.WriteOperation("^", methodCallExpr.Arguments[0], methodCallExpr, methodCallExpr.Arguments[1], methodCallExpr);
                    return context.Expression;
                };
            }

            return null;
        }

        /// <summary>
        /// Writes the <code>IS NULL</code> neo4j function
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> IsNull(FunctionHandlerContext context)
        {
            //if (context.Expression is MethodCallExpression methodCallExpr
            //    && methodCallExpr.Method is var methodInfo
            //    && methodInfo.Name == "IsNull"
            //    && methodInfo.DeclaringType == Defaults.ObjectExtensionsType
            //    && methodInfo.IsExtensionMethod())

            if (IsFuncsMethod(context, "IsNull", out var methodCallExpr, out var methodInfo))
            {
                return () =>
                {
                    var sourceExpr = methodCallExpr.Arguments[0];
                    context.Visitor.WriteOperation("IS NULL", sourceExpr, methodCallExpr, null, methodCallExpr);
                    return methodCallExpr;
                };
            }

            return null;
        }

        /// <summary>
        /// Writes the <code>IS NOT NULL</code> neo4j function
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> IsNotNull(FunctionHandlerContext context)
        {
            //if (context.Expression is MethodCallExpression methodCallExpr
            //    && methodCallExpr.Method is var methodInfo
            //    && methodInfo.Name == "IsNotNull"
            //    && methodInfo.DeclaringType == Defaults.ObjectExtensionsType
            //    && methodInfo.IsExtensionMethod())

            if (IsFuncsMethod(context, "IsNotNull", out var methodCallExpr, out var methodInfo))
            {
                return () =>
                {
                    var sourceExpr = methodCallExpr.Arguments[0];
                    context.Visitor.WriteOperation("IS NOT NULL", sourceExpr, methodCallExpr, null, methodCallExpr);
                    return methodCallExpr;
                };
            }

            return null;
        }

        public static Func<Expression> _AsRaw(FunctionHandlerContext context)
        {
            //if (context.Expression is MethodCallExpression methodCallExpr
            //    && methodCallExpr.Method is var methodInfo
            //    && methodInfo.Name == "_AsRaw"
            //    && methodInfo.DeclaringType == Defaults.HelperExtensionsType
            //    && methodInfo.IsExtensionMethod())
            if (IsFuncsMethod(context, "_AsRaw", out var methodCallExpr, out var methodInfo))
            {
                return () =>
                {
                    //output raw value
                    var sourceExpr = methodCallExpr.Arguments[0];
                    context.Visitor.WriteArgument(sourceExpr, methodCallExpr, isRawValue: true);
                    return context.Expression;
                };
            }

            return null;
        }

        /// <summary>
        /// Writes the binary operators
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> BinaryType(FunctionHandlerContext context)
        {
            if (context.VisitorContext.WriteOperators
                && context.Expression is BinaryExpression node
                && context.VisitorContext.BinaryOperators.TryGetValue(node.NodeType, out var neo4jOperator)
                && neo4jOperator != null)
            {
                return () =>
                {
                    //write the operation
                    context.Visitor.WriteOperation(neo4jOperator, node.Left, node, node.Right, node);

                    return context.Expression;
                };
            }

            return null;
        }

        /// <summary>
        /// Writes the unary operators
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> UnaryType(FunctionHandlerContext context)
        {
            string leftOperator = null, rightOperator = null;

            if (context.VisitorContext.WriteOperators
                && context.Expression is UnaryExpression node
                && (context.VisitorContext.LeftAlignedUnaryOperators.TryGetValue(node.NodeType, out leftOperator)
                || context.VisitorContext.LeftAlignedUnaryOperators.TryGetValue(node.NodeType, out rightOperator))
                && (leftOperator != null || rightOperator != null))
            {
                return () =>
                {
                    //write the operation
                    if (leftOperator != null)
                    {
                        context.Visitor.WriteOperation(leftOperator, null, node, node.Operand, node);
                    }
                    else
                    {
                        context.Visitor.WriteOperation(rightOperator, node.Operand, false, node, false, null, node);
                    }

                    return context.Expression;
                };
            }

            return null;
        }

        /// <summary>
        /// Treats a <see cref="MemberInitExpression"/>, dictionary <see cref="ListInitExpression"/>, or a generic <see cref="NewExpression"/> as a constant, or nothing, as may be appropriate.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> New_Dictionary_MemberInitType(FunctionHandlerContext context)
        {
            if (context.VisitorContext.WriteConstants
                && context.Expression is Expression node
                && (node.NodeType == ExpressionType.MemberInit
                || (node.NodeType == ExpressionType.ListInit && node.Type.IsDictionaryType())
                || node.NodeType == ExpressionType.New))
            {
                return () =>
                {
                    //normally, we shouldn't even encounter this node here
                    //but process anyway
                    //process the unhandled nodes now and don't go any deeper (i.e., no continuation)
                    context.Visitor.ProcessUnhandledSimpleVars(node, considerInits: true);
                    return context.Expression;
                };
            }

            return null;
        }

        /// <summary>
        /// Writes a <see cref="NewArrayExpression"/> as a neo4j array.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> NewArrayType(FunctionHandlerContext context)
        {
            if (context.VisitorContext.WriteConstants 
                && context.Expression is NewArrayExpression node
                && node.Type != Defaults.StringType)
            {
                return () =>
                {
                    return HandleArray(node.Expressions, context);
                };
            }

            return null;
        }

        /// <summary>
        /// Skips the No-Further-Processing method (<see cref="HelperExtensions._{T}(T)"/>) in an expression.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> NoFurtherProcessing(FunctionHandlerContext context)
        {
            //if (context.Expression is MethodCallExpression methodCallExpr
            //    && methodCallExpr.Method is var methodInfo
            //    && methodInfo.Name == "_"
            //    && methodInfo.DeclaringType == Defaults.HelperExtensionsType
            //    && methodInfo.IsExtensionMethod())
            if (IsFuncsMethod(context, "_", out var methodCallExpr, out var methodInfo))
            {
                return () =>
                {
                    //this npf method is useless here so just skip
                    context.Visitor.Ignore(methodCallExpr);
                    var expr = context.Continuation();
                    return expr;
                };
            }

            return null;
        }
        #endregion

        #region ICypherResultItem
        public static Func<Expression> ICypherResultItemMethod(FunctionHandlerContext context, string method, string cypherMethod)
        {
            return ICypherResultItemMethod(context, method, cypherMethod, (e, c) => c.Continuation());
        }

        public static Func<Expression> ICypherResultItemMethod(FunctionHandlerContext context, string method, string cypherMethod,
            Func<MethodCallExpression, FunctionHandlerContext, Expression> body)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == method
                && methodInfo.DeclaringType == Defaults.ICypherResultItemType
                && !methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    //usually, we won't need these arguments to be visited
                    foreach (var arg in methodCallExpr.Arguments)
                        context.Visitor.Ignore(arg);

                    return HandleGenericMethodBody(cypherMethod, methodCallExpr, context, body);
                };
            }

            return null;
        }
        #endregion

        #region String Functions
        public static Func<Expression> ToString(FunctionHandlerContext context)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == "ToString"
                && !methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    foreach (var arg in methodCallExpr.Arguments)
                        context.Visitor.Ignore(arg);

                    try
                    {
                        var value = methodCallExpr.ExecuteExpression<string>();
                        context.Visitor.Builder.Append(value, methodCallExpr);
                    }
                    catch
                    {
                        HandleGenericMethodBody("toString", methodCallExpr, context, (m, c) =>
                        {
                            //write the argument
                            context.Visitor.WriteArgument(m.Object, methodCallExpr);
                            return m;
                        });
                    }
                    return methodCallExpr;
                };
            }

            return null;
        }

        public static Func<Expression> StringLength(FunctionHandlerContext context)
        {
            if (context.Expression is MemberExpression memberExpr
                && memberExpr.Member is PropertyInfo propertyInfo
                && propertyInfo.Name == "Length"
                && propertyInfo.DeclaringType == Defaults.StringType)
            {
                return () =>
                {
                    //ignore the length property so it won't be mistaken for an entity scalar property
                    context.Visitor.Ignore(memberExpr);

                    try
                    {
                        var value = memberExpr.ExecuteExpression<string>();
                        context.Visitor.Builder.Append(value, memberExpr);
                    }
                    catch
                    {
                        HandleGenericMethodBody("size", memberExpr, context, (m, c) =>
                        {
                            //write the argument
                            context.Visitor.WriteArgument(m.Expression, memberExpr);
                            return m;
                        });
                    }
                    return context.Expression;
                };
            }

            return null;
        }

        public static Func<Expression> StringMethod(FunctionHandlerContext context, string method, string cypherMethod)
        {
            return StringMethod(context, method, cypherMethod, (e, c) => c.Continuation());
        }

        public static Func<Expression> StringMethod(FunctionHandlerContext context, string method, string cypherMethod,
            Func<MethodCallExpression, FunctionHandlerContext, Expression> body)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == method
                && methodInfo.DeclaringType == Defaults.StringType
                && !methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    //first try to execute this methodcall to see if a value returns
                    //it it doesn't, then do write the function
                    //this is to avoid wasting database resources performing operations that could have been completed here

                    try
                    {
                        var value = methodCallExpr.ExecuteExpression<object>();
                        if(value != null)
                        {
                            context.Visitor.Builder.Append(value, methodCallExpr);
                            return context.Expression;
                        }
                    }
                    catch
                    {

                    }

                    //usually, we won't need these arguments to be visited
                    foreach (var arg in methodCallExpr.Arguments)
                        context.Visitor.Ignore(arg);

                    return HandleGenericMethodBody(cypherMethod, methodCallExpr, context, body);
                };
            }

            return null;
        }

        public static Expression StringReplaceBody(MethodCallExpression expr, FunctionHandlerContext context)
        {
            foreach (var arg in expr.Arguments)
                context.Visitor.Ignore(arg);

            var newExpr = context.Continuation();
            context.Visitor.Builder.Append(", ", context.Expression);

            //write search term
            var searchTermExpr = expr.Arguments[0];
            context.Visitor.Allow(searchTermExpr);
            context.Visitor.WriteArgument(searchTermExpr, context.Expression);

            context.Visitor.Builder.Append(", ", context.Expression);

            //write replacement
            var replacementExpr = expr.Arguments[1];
            context.Visitor.Allow(replacementExpr);
            context.Visitor.WriteArgument(replacementExpr, context.Expression);

            return newExpr;
        }

        public static Expression StringSubstringBody(MethodCallExpression expr, FunctionHandlerContext context)
        {
            foreach (var arg in expr.Arguments)
                context.Visitor.Ignore(arg);

            var newExpr = context.Continuation();
            context.Visitor.Builder.Append(", ", context.Expression);

            //write start
            var startExpr = expr.Arguments[0];
            context.Visitor.Allow(startExpr);
            context.Visitor.WriteArgument(startExpr, context.Expression);

            if (expr.Arguments.Count > 1)
            {
                context.Visitor.Builder.Append(", ", context.Expression);

                //write length
                var lengthExpr = expr.Arguments[1];
                context.Visitor.Allow(lengthExpr);
                context.Visitor.WriteArgument(lengthExpr, context.Expression);
            }

            return newExpr;
        }

        public static Expression StringSplitBody(MethodCallExpression expr, FunctionHandlerContext context)
        {
            foreach (var arg in expr.Arguments)
                context.Visitor.Ignore(arg);

            var newExpr = context.Continuation();
            context.Visitor.Builder.Append(", ", context.Expression);

            var separatorExpr = expr.Arguments[0];

            //check to see if it's a newarrayinit or can be compiled to an array, and pick only the first argument
            if (separatorExpr is NewArrayExpression newArrayExpr)
            {
                //pick only the first context.Expression
                separatorExpr = newArrayExpr.Expressions[0];
            }
            else
            {
                Array separators = null;
                try
                {
                    separators = separatorExpr.ExecuteExpression<object>() as Array;
                }
                catch
                {
                }

                if (separators != null)
                {
                    //pick only the first value
                    separatorExpr = Expression.Constant(separators.GetValue(0));
                }
            }

            //write separator
            context.Visitor.Allow(separatorExpr);
            context.Visitor.WriteArgument(separatorExpr, context.Expression);

            return newExpr;
        }

        public static Func<Expression> Reverse(FunctionHandlerContext context)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == "Reverse"
                && methodInfo.DeclaringType == Defaults.EnumerableType
                && methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    return HandleGenericMethodBody("reverse", context);
                };
            }

            return null;
        }

        /// <summary>
        /// Writes the neo4j string comparison functions like <code>STARTS WITH</code>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> StringComparison(FunctionHandlerContext context, string method, string cypherMethod)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == method
                && methodInfo.DeclaringType == Defaults.StringType
                && !methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    //source.Method(argument)
                    //would be: source cypherMethod argument
                    var argumentExpr = methodCallExpr.Arguments.Single();

                    context.Visitor.Ignore(argumentExpr);
                    var newExpr = context.Continuation();

                    context.Visitor.Builder.Append($" {cypherMethod} ", methodCallExpr);

                    context.Visitor.Allow(argumentExpr);
                    context.Visitor.WriteArgument(argumentExpr, methodCallExpr);
                    return newExpr;
                };
            }

            return null;
        }
        #endregion

        #region List Functions
        public static Func<Expression> ElementAt(FunctionHandlerContext context)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == "ElementAt"
                && methodInfo.DeclaringType == Defaults.EnumerableType
                && methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    //found our extension method.
                    //because it is extension method, the first argument would be the instance (this) argument. so we take the second one.
                    var arrayIndexExpr = methodCallExpr.Arguments[1];
                    return HandleArrayIndexValue(arrayIndexExpr, context);
                };
            }

            return null;
        }

        public static Func<Expression> ArrayIndexNodeType(FunctionHandlerContext context)
        {
            if (context.Expression.NodeType == ExpressionType.ArrayIndex
                && context.Expression is BinaryExpression binExpr)
            {
                return () =>
                {
                    //the index context.Expression is always on the right
                    var arrayIndexExpr = binExpr.Right;
                    return HandleArrayIndexValue(arrayIndexExpr, context);
                };
            }

            return null;
        }

        /// <summary>
        /// cypherMethod(variable IN source WHERE predicate | selector)
        /// </summary>
        /// <param name="cypherMethod"></param>
        /// <param name="predicateExpr"></param>
        /// <param name="context"></param>
        public static Expression CypherListComprehensionBody
            (Expression variableExpr, LambdaExpression predicateExpr, LambdaExpression selectorExpr, FunctionHandlerContext context)
        {
            if (variableExpr == null)
                throw new ArgumentNullException(nameof(variableExpr));

            //Method(source, (variable) => predicate, (variable) => selector)
            //would be: cypherMethod(variable IN source WHERE predicate)
            var expr = predicateExpr ?? selectorExpr;

            if (variableExpr is ParameterExpression parameter)
            {

                //just write it directly
                context.Visitor.Builder.Append(parameter.Name, context.Expression);
            }
            else
            {
                context.Visitor.Allow(variableExpr);
                context.Visitor.WriteArgument(variableExpr, context.Expression);
            }

            context.Visitor.Builder.Append(" IN ", context.Expression);

            //add all to handled nodes so they aren't processed
            context.Visitor.Ignore(variableExpr);
            context.Visitor.Ignore(predicateExpr);
            context.Visitor.Ignore(selectorExpr);

            var newExpr = context.Continuation();

            context.Visitor.Allow(variableExpr);

            if (predicateExpr != null)
            {
                context.Visitor.Builder.Append(" WHERE ", context.Expression);
                //write the predicate
                context.Visitor.Visit(predicateExpr.Body);
            }


            if (selectorExpr != null)
            {
                context.Visitor.Builder.Append(" | ", context.Expression);
                //write the predicate
                context.Visitor.Visit(selectorExpr.Body);
            }

            return newExpr;
        }

        /// <summary>
        /// method(source, (variable) => predicate)
        /// would be: cypherMethod(variable IN source WHERE predicate)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="method"></param>
        /// <param name="cypherMethod"></param>
        /// <returns></returns>
        public static Func<Expression> CypherListPredicate(FunctionHandlerContext context, string method, string cypherMethod, Type methodType = null)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == method
                && methodInfo.DeclaringType == (methodType ?? Defaults.EnumerableType)
                //&& methodInfo.IsExtensionMethod()
                && methodCallExpr.Arguments.Count > 1
                && methodCallExpr.Arguments[1] is LambdaExpression predicateExpr
                && predicateExpr.Parameters.Count == 1)
            {
                return () =>
                {
                    //method(source, (variable) => predicate)
                    //would be: cypherMethod(variable IN source WHERE predicate)
                    return HandleGenericMethodBody(cypherMethod, predicateExpr, context, (expr, ctx) =>
                    {
                        return CypherListComprehensionBody(expr.Parameters.First(), expr, null, ctx);
                    });
                };
            }

            return null;
        }

        /// <summary>
        /// method(source, (variable) => selector)
        /// would be: cypherMethod(variable IN source | selector)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="method"></param>
        /// <param name="cypherMethod"></param>
        /// <returns></returns>
        public static Func<Expression> CypherListSelector(FunctionHandlerContext context, string method, string cypherMethod, Type methodType = null)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == method
                && methodInfo.DeclaringType == (methodType ?? Defaults.EnumerableType)
                //&& methodInfo.IsExtensionMethod()
                && methodCallExpr.Arguments.Count > 1
                && methodCallExpr.Arguments[1] is LambdaExpression selectorExpr
                && selectorExpr.Parameters.Count == 1)
            {
                return () =>
                {
                    //method(source, (variable) => selector)
                    //would be: cypherMethod(variable IN source | selector)
                    return HandleGenericMethodBody(cypherMethod, selectorExpr, context, (expr, ctx) =>
                    {
                        return CypherListComprehensionBody(expr.Parameters.First(), null, expr, ctx);
                    });
                };
            }

            return null;
        }

        /// <summary>
        /// Writes the <code>IN</code> neo4j function.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> CollectionContains(FunctionHandlerContext context)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == "Contains"
                && methodInfo.DeclaringType.GetInterfaces()?.Any(i => i == typeof(ICollection<>)
                || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))) == true)
            {
                return () =>
                {
                    return CypherListComprehensionBody(methodCallExpr.Arguments.First(), null, null, context);
                };
            }

            return null;
        }

        /// <summary>
        /// Writes the <code>reduce</code> neo4j list function.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> Aggregate(FunctionHandlerContext context)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == "Aggregate"
                && methodInfo.DeclaringType == Defaults.EnumerableType
                && methodInfo.IsExtensionMethod()
                && (methodCallExpr.Arguments[1] is LambdaExpression operationExpr
                || (operationExpr = methodCallExpr.Arguments[2] as LambdaExpression) != null)
                && operationExpr.Parameters.Count == 2)
            {
                return () =>
                {
                    //Aggregate(source, (accumulator, variable) => operation)
                    //would be: reduce(accumulator = [default(accumulator)], variable IN source | operation)
                    //Aggregate(source, initial, (accumulator, variable) => operation)
                    //would be: reduce(accumulator = initial, variable IN source | operation)
                    return HandleGenericMethodBody("reduce", operationExpr, context, ((expr, ctx) =>
                    {
                        var accumulatorExpr = expr.Parameters.First();
                        var variableExpr = expr.Parameters.Last();

                        var initialExpr = methodCallExpr.Arguments.Count > 2 ? methodCallExpr.Arguments[1]
                        : Expression.Constant(accumulatorExpr.Type == Defaults.StringType ? "" 
                        : accumulatorExpr.Type.GetDefaultValue(),
                        accumulatorExpr.Type);

                        //write the initial value
                        ctx.Visitor.Builder.Append($"{accumulatorExpr.Name} = ", methodCallExpr);
                        ctx.Visitor.WriteArgument(initialExpr, methodCallExpr);

                        ctx.Visitor.Builder.Append(", ", methodCallExpr);

                        ctx.Visitor.Ignore(initialExpr);
                        return CypherListComprehensionBody(variableExpr, null, operationExpr, context);
                    }));
                };
            }

            return null;
        }

        /// <summary>
        /// Writes the <code>+</code> neo4j list function.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> Concat(FunctionHandlerContext context)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == "Concat"
                && methodInfo.DeclaringType == Defaults.EnumerableType
                && methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    //Concat(source, arg)
                    //would be: source + arg
                    context.Visitor.WriteOperation("+", methodCallExpr.Arguments[0], methodCallExpr, methodCallExpr.Arguments[1], methodCallExpr);
                    return context.Expression;
                };
            }

            return null;
        }

        /// <summary>
        /// Concatenates a list. Same as Concat.
        /// NOTE: THIS IS NOT THE UNION CLAUSE. THIS WORKS ON ONLY LISTS, AND NOT ROWS. IT ALSO DOES NOT GUARANTEE DISTINCT VALUES.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> Union(FunctionHandlerContext context)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == "Union"
                && methodInfo.DeclaringType == Defaults.EnumerableType
                && methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    //Union(source, arg) i.e., Concat(source, arg)
                    //would be: source + arg
                    var sourceGenericType = methodInfo.GetGenericArguments()[0];

                    var concatMethod = Utils.Utilities.GetGenericMethodInfo(Utils.Utilities.GetMethodInfo
                        (() => Enumerable.Concat<object>(null, null)), sourceGenericType);

                    var concatMethodCall = Expression.Call(concatMethod, methodCallExpr.Arguments); //i.e., Concat(source, arg)

                    //now visit new expression
                    context.Visitor.Ignore(methodCallExpr);
                    var newExpr = context.Visitor.Visit(concatMethodCall);
                    context.Visitor.Allow(methodCallExpr);

                    return newExpr;
                };
            }

            return null;
        }

        /// <summary>
        /// Filters each element contained in both lists.
        /// NOTE: THIS WORKS ON ONLY LISTS, AND NOT ROWS. IT ALSO DOES NOT GUARANTEE DISTINCT VALUES.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> Intersect(FunctionHandlerContext context)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == "Intersect"
                && methodInfo.DeclaringType == Defaults.EnumerableType
                && methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    //Intersect(source, arg) i.e., Where(source, randomVar => arg.Contains(randomVar))
                    //would be: filter(randomVar IN source WHERE (randomVar IN arg))
                    var sourceExpr = methodCallExpr.Arguments[0];
                    var argExpr = methodCallExpr.Arguments[1];
                    var sourceGenericType = methodInfo.GetGenericArguments()[0];

                    var randomVar = $"_is_{Utils.Utilities.GetRandomVariableFor("isect")}_rdm_";
                    var parameter = Expression.Parameter(sourceGenericType, randomVar);

                    var containsMethod = Utils.Utilities.GetGenericMethodInfo(
                        Utils.Utilities.GetMethodInfo(() => Enumerable.Contains<object>(null, null)), sourceGenericType);

                    var containsMethodCall = Expression.Call(containsMethod, argExpr, parameter); //i.e., Contains(arg, randomVar)

                    var predicateExpr = Expression.Lambda(containsMethodCall, parameter); //i.e., randomVar => Contains(arg, randomVar)

                    var whereMethod = Utils.Utilities.GetGenericMethodInfo(
                        Utils.Utilities.GetMethodInfo(() => Enumerable.Where<object>(null, f => false)), sourceGenericType);

                    var whereMethodCall = Expression.Call(whereMethod, sourceExpr, predicateExpr); //i.e., Where(source, randomVar => arg.Contains(randomVar))

                    //now visit new expression
                    context.Visitor.Ignore(methodCallExpr);
                    var newExpr = context.Visitor.Visit(whereMethodCall);
                    context.Visitor.Allow(methodCallExpr);

                    return newExpr;
                };
            }

            return null;
        }
        #endregion

        #region Aggregate Functions
        public static Func<Expression> EnumerableMethod(FunctionHandlerContext context, string method, string cypherMethod)
        {
            return EnumerableMethod(context, method, cypherMethod, (e, c) => c.Continuation());
        }

        public static Func<Expression> EnumerableMethod(FunctionHandlerContext context, string method, string cypherMethod,
            Func<MethodCallExpression, FunctionHandlerContext, Expression> body)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == method
                && methodInfo.DeclaringType == Defaults.EnumerableType
                && methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    try
                    {
                        //usually, we won't need these arguments to be visited
                        foreach (var arg in methodCallExpr.Arguments.Skip(1))
                            context.Visitor.Ignore(arg);
                    }
                    catch
                    {
                    }

                    return HandleGenericMethodBody(cypherMethod, methodCallExpr, context, body);
                };
            }

            return null;
        }

        /// <summary>
        /// Writes the <code>DISTINCT</code> neo4j function.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> Distinct(FunctionHandlerContext context)
        {
            if (IsFuncsMethod(context, "Distinct", out var methodCallExpr, out var methodInfo))
            {
                return () =>
                {
                    //Distinct(source)
                    //would be: DISTINCT source
                    var sourceExpr = methodCallExpr.Arguments[0];
                    context.Visitor.WriteOperation("DISTINCT", null, methodCallExpr, sourceExpr, methodCallExpr);
                    return context.Expression;
                };
            }

            return null;
        }
        #endregion

        #region Scalar Functions
        public static bool IsFuncsMethod(FunctionHandlerContext context, string method,
                                    out MethodCallExpression methodCallExpr, out MethodInfo methodInfo)
        {
            methodCallExpr = null;
            methodInfo = null;

            return ((methodCallExpr = context.Expression as MethodCallExpression) != null
                && (methodInfo = methodCallExpr.Method) != null
                && methodInfo.Name == method
                && (methodInfo.DeclaringType == Defaults.CypherExtensionFuncsType
                || methodInfo.DeclaringType == Defaults.CypherFuncsType));
        }

        public static Func<Expression> FuncsMethod(FunctionHandlerContext context, string method, string cypherMethod)
        {
            return FuncsMethod(context, method, cypherMethod, (e, c) =>
            {
                //write the arguments in order
                bool isFirstArg = true;
                foreach (var arg in e.Arguments)
                {
                    if (!isFirstArg)
                        context.Visitor.Builder.Append(", ", context.Expression);
                    else
                        isFirstArg = false;

                    context.Visitor.WriteArgument(arg, context.Expression);
                }

                return e;
            });
        }


        public static Func<Expression> FuncsMethod(FunctionHandlerContext context, string method, string cypherMethod,
            Func<MethodCallExpression, FunctionHandlerContext, Expression> body)
        {
            if (IsFuncsMethod(context, method, out var methodCallExpr, out var methodInfo))
            {
                return () =>
                {
                    //try
                    //{
                    //    //usually, we won't need these arguments to be visited
                    //    foreach (var arg in methodCallExpr.Arguments.Skip(1))
                    //        context.Visitor.Ignore(arg);
                    //}
                    //catch
                    //{
                    //}

                    return HandleGenericMethodBody(cypherMethod, methodCallExpr, context, body);
                };
            }

            return null;
        }

        /// <summary>
        /// Writes the <code>coalesce</code> neo4j scalar function.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> CoalesceNodeType(FunctionHandlerContext context)
        {
            if (context.Expression.NodeType == ExpressionType.Coalesce
                && context.Expression is BinaryExpression node)
            {
                return () =>
                {
                    //left ?? right
                    //would be: coalesce(left, right)
                    return HandleGenericMethodBody("coalesce", node, context, (b, c) =>
                    {
                        //write left
                        context.Visitor.WriteArgument(node.Left, context.Expression);

                        context.Visitor.Builder.Append(", ", context.Expression);

                        //write right
                        context.Visitor.WriteArgument(node.Right, context.Expression);

                        return b;
                    });
                };
            }

            return null;
        }

        public static Func<Expression> ArrayLengthNodeType(FunctionHandlerContext context)
        {
            if (context.Expression.NodeType == ExpressionType.ArrayLength
                && context.Expression is UnaryExpression unaryExpr)
            {
                return () =>
                {
                    return HandleGenericMethodBody("size", context);
                };
            }

            return null;
        }

        /// <summary>
        /// This produces size(). For count() use .Count().
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> CollectionCount(FunctionHandlerContext context)
        {
            if (context.Expression is MemberExpression memberExpr
                && memberExpr.Member is PropertyInfo propertyInfo
                && propertyInfo.Name == "Count"
                && propertyInfo.DeclaringType.GetInterfaces()?.Any(i => i == typeof(ICollection<>)
                || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))) == true)
            {
                return () =>
                {
                    return HandleGenericMethodBody("size", memberExpr, context, (m, c) =>
                    {
                        //ignore the length property so it won't be mistaken for an entity scalar property
                        context.Visitor.Ignore(m);
                        return context.Continuation();
                    });
                };
            }

            return null;
        }

        /// <summary>
        /// Writes the convert neo4j scalar functions.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> ConvertMethod(FunctionHandlerContext context, string method, string cypherMethod)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == method
                && methodInfo.DeclaringType == typeof(Convert)
                && !methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    //Convert.ToMethod(context.Expression)
                    //would be: cypherMethod(context.Expression)
                    return HandleGenericMethodBody(cypherMethod, methodCallExpr, context, ((expr, ctx) =>
                    {
                        //write the argument
                        ctx.Visitor.WriteArgument(expr.Arguments[0], methodCallExpr);
                        return expr;
                    }));
                };
            }

            return null;
        }
        #endregion

        #region Predicate Functions
        /// <summary>
        /// Writes the <code>any</code> neo4j predicate function with a random variable checking for the first non-null object.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> EmptyAny(FunctionHandlerContext context)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == "Any"
                && methodInfo.DeclaringType == Defaults.EnumerableType
                && methodInfo.IsExtensionMethod()
                && methodCallExpr.Arguments.Count == 1)
            {
                return () =>
                {
                    //randomVar IN source WHERE randomVar IS NOT NULL
                    var sourceEnumerableExpr = methodCallExpr.Arguments[0];
                    var sourceType = methodInfo.GetGenericArguments()[0];

                    var randomVar = $"_ea_{Utils.Utilities.GetRandomVariableFor("any")}_rdm_";
                    var parameter = Expression.Parameter(sourceType, randomVar);

                    var isNotNullMethod = Utils.Utilities.GetGenericMethodInfo(
                        Utils.Utilities.GetMethodInfo(() => CypherExtensionFunctions.IsNotNull<object>(null)), sourceType);

                    var isNotNullMethodCall = Expression.Call(isNotNullMethod, parameter); //i.e., randomVar.IsNotNull()

                    var anyWithPredicateMethod = Utils.Utilities.GetGenericMethodInfo(
                        Utils.Utilities.GetMethodInfo(() => Enumerable.Any<object>(null, null)), sourceType);

                    var predicateExpr = Expression.Lambda(isNotNullMethodCall, parameter); //i.e., randomVar => randomVar.IsNotNull()

                    //source.Any(randomvar => randomVar.IsNotNull());
                    var newAnyMethodCallExpr = Expression.Call(anyWithPredicateMethod, sourceEnumerableExpr, predicateExpr);

                    //now visit new expression
                    context.Visitor.Ignore(methodCallExpr);
                    var newExpr = context.Visitor.Visit(newAnyMethodCallExpr);
                    context.Visitor.Allow(methodCallExpr);

                    return newExpr;
                };
            }

            return null;
        }

        /// <summary>
        /// Writes the <code>exists</code> neo4j predicate function.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Func<Expression> EnumerableContains(FunctionHandlerContext context)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == "Contains"
                && methodInfo.DeclaringType == Defaults.EnumerableType
                && methodInfo.IsExtensionMethod()
                && methodCallExpr.Arguments.Count > 1)
            {
                return () =>
                {
                    return CypherListComprehensionBody(methodCallExpr.Arguments[1], null, null, context);
                };
            }

            return null;
        }
        #endregion

        #region MathematicalFunctions
        public static Func<Expression> MathMethod(FunctionHandlerContext context, string method, string cypherMethod)
        {
            return MathMethod(context, method, cypherMethod, (e, c) =>
            {
                //write the arguments in order
                bool isFirstArg = true;
                foreach(var arg in e.Arguments)
                {
                    if (!isFirstArg)
                        context.Visitor.Builder.Append(", ", context.Expression);
                    else
                        isFirstArg = false;

                    context.Visitor.WriteArgument(arg, context.Expression);
                }

                return e;
            });
        }

        public static Func<Expression> MathMethod(FunctionHandlerContext context, string method, string cypherMethod,
            Func<MethodCallExpression, FunctionHandlerContext, Expression> body)
        {
            if (context.Expression is MethodCallExpression methodCallExpr
                && methodCallExpr.Method is var methodInfo
                && methodInfo.Name == method
                && methodInfo.DeclaringType == Defaults.MathType
                && !methodInfo.IsExtensionMethod())
            {
                return () =>
                {
                    //first try to execute this methodcall to see if a value returns
                    //it it doesn't, then do write the function
                    //this is to avoid wasting database resources performing operations that could have been completed here

                    try
                    {
                        var value = methodCallExpr.ExecuteExpression<object>();
                        if (value != null)
                        {
                            context.Visitor.Builder.Append(value, methodCallExpr);
                            return context.Expression;
                        }
                    }
                    catch
                    {

                    }

                    return HandleGenericMethodBody(cypherMethod, methodCallExpr, context, body);
                };
            }

            return null;
        }
        #endregion
    }
}
                    