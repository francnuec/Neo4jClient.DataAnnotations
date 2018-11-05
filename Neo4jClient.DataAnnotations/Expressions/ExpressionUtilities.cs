using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Cypher.Extensions;
using Neo4jClient.DataAnnotations.Serialization;
using Newtonsoft.Json.Linq;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class ExpressionUtilities
    {
        /// <summary>
        /// Expecting:
        /// a =&gt; a.Property //member access = "a.property"//
        /// a =&gt; new { a.Property1, a.Property2 } //multiple member accesses = "a.property1", "a.property2"//
        /// a =&gt; a.Property.ToString() //complex expression (i.e., member access with function calls) = "toString(a.property)"//
        /// a =&gt; new { a.Property1, Property2 = a.Property2.Length } //multiple complex expressions = "a.property1", "size(a.property2)"//
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressions"></param>
        /// <param name="isMemberAccess">If false, the variable is not included</param>
        internal static string[] GetVariableExpressions<T>(Expression<Func<T, object>> expressions,
            QueryContext queryContext,
            bool isMemberAccess = true, string variable = null,
            FunctionVisitorContext visitorContext = null)
        {
            return GetVariableExpressions(typeof(T), expressions, queryContext, isMemberAccess, variable, visitorContext);
        }

        /// <summary>
        /// Expecting:
        /// a =&gt; a.Property //member access = "a.property"//
        /// a =&gt; new { a.Property1, a.Property2 } //multiple member accesses = "a.property1", "a.property2"//
        /// a =&gt; a.Property.ToString() //complex expression (i.e., member access with function calls) = "toString(a.property)"//
        /// a =&gt; new { a.Property1, Property2 = a.Property2.Length } //multiple complex expressions = "a.property1", "size(a.property2)"//
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressions"></param>
        /// <param name="isMemberAccess">If false, the variable is not included</param>
        internal static string[] GetVariableExpressions(Type sourceType,
            LambdaExpression expressions, QueryContext queryContext,
            bool isMemberAccess = true, string variable = null,
            FunctionVisitorContext visitorContext = null)
        {
            if (expressions == null)
                throw new ArgumentNullException(nameof(expressions));

            //a => a.Property
            //a => new { a.Property1, a.Property2 }

            var parameterExpr = expressions.Parameters?.FirstOrDefault();
            variable = variable ?? parameterExpr.Name;
            var randomVar = $"_ve_{Utils.Utilities.GetRandomVariableFor(variable)}_rdm_";

            //create a Vars.Get call for this variable
            var varsGetCallExpr = GetVarsGetExpressionFor(randomVar, sourceType);

            //get the body without an object cast if present
            var bodyExpr = expressions.Body.Uncast(out var cast, castToRemove: typeof(object));

            //replace the parameter with the a Vars.Get call
            var newBodyExpr = new ParameterReplacerVisitor(
                    new Dictionary<string, Expression>() { { parameterExpr.Name, varsGetCallExpr } }
                    ).Visit(bodyExpr);

            var variableExpressions = new List<Expression>() { newBodyExpr };

            if (newBodyExpr.Type.IsAnonymousType())
            {
                //that is, new { a.Property1, a.Property2 }
                variableExpressions.Clear();
                variableExpressions.AddRange((newBodyExpr as NewExpression).Arguments);
            }

            List<string> values = new List<string>();
            var funcsVisitor = new FunctionExpressionVisitor(queryContext, visitorContext);

            Action<Expression> visit = (expr) =>
            {
                funcsVisitor.Clear();
                var newExpr = funcsVisitor.Visit(expr);
                values.Add(funcsVisitor.Builder.ToString());
            };

            foreach (var expr in variableExpressions)
            {
                if (expr.Type.IsComplex())
                {
                    //explode
                    var accesses = ExplodeComplexTypeMemberAccess
                        (queryContext.EntityService, expr, out var inversePaths);
                    if (accesses?.Count > 0)
                    {
                        foreach (var item in accesses)
                        {
                            visit(item);
                        }

                        continue;
                    }
                }

                visit(expr);
            }

            //replace the random variable with the actual one.
            var varReplacement = isMemberAccess ? variable + "." : "";

            return values.Select(v => v.Replace(randomVar + ".", varReplacement)
            .Replace(randomVar, varReplacement.TrimEnd('.'))).ToArray();
        }

        public static MethodCallExpression GetVarsGetExpressionFor(string variable, Type type)
        {
            //create a Vars.Get call for this variable
            var varsGetMethodInfo = Utils.Utilities.GetGenericMethodInfo(Utils.Utilities.GetMethodInfo(() => Vars.Get<object>(null)), type);
            //now Vars.Get<type>(variable)
            var varsGetCallExpr = Expression.Call(varsGetMethodInfo, Expression.Constant(variable, Defaults.StringType));

            return varsGetCallExpr;
        }

        public static MethodCallExpression GetVarsGetExpressionFor<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            return GetVarsGetExpressionFor(selector, typeof(TSource), typeof(TResult));
        }

        public static MethodCallExpression GetVarsGetExpressionFor(LambdaExpression selector, Type source, Type result)
        {
            //create a Vars.Get call for this selector
            var varsGetMethodInfo = Utils.Utilities.GetGenericMethodInfo(Utils.Utilities.GetMethodInfo(() => Vars.Get<object, object>(null)), source, result);
            //now Vars.Get<source, result>(selector)
            var varsGetCallExpr = Expression.Call(varsGetMethodInfo, selector);

            return varsGetCallExpr;
        }

        internal static string BuildProjectionQueryExpression(LambdaExpression expression,
            QueryContext queryContext, FunctionExpressionVisitor functionVisitor,
            out CypherResultMode resultMode, out CypherResultFormat resultFormat)
        {
            //expecting:
            //u => u //parameter
            //u => u.Name //memberaccess
            //(u,v) => new { u, v } //new anonymous expression
            //(u, v) => new User(){ Name = u.Id() } //member init

            resultFormat = CypherResultFormat.DependsOnEnvironment;

            var bodyExpr = expression.Body.Uncast(out var bodyCast, castToRemove: Defaults.ObjectType);

            string result = null;

            if (bodyExpr.NodeType == ExpressionType.MemberInit
                || bodyExpr.NodeType == ExpressionType.New)
            {
                //use final properties method in this case

                //but first replace all parameters with vars.get call
                var replacements = new Dictionary<Expression, Expression>();
                var replacerVisitor = new ReplacerExpressionVisitor(replacements);
                if (expression.Parameters?.Count > 0)
                {
                    foreach (var p in expression.Parameters)
                    {
                        replacements.Add(p, GetVarsGetExpressionFor(p.Name, p.Type));
                    }
                    //make parameter replacements now
                    bodyExpr = replacerVisitor.Visit(bodyExpr);
                    replacements.Clear();
                }

                //then we need to seal all assignments with the nfp method ("_") so that they don't affect the final properties key names.
                //the original key/member names set are important to the deserialization method (and to the rest of the user code)
                //hence why a change here won't be appropriate and we need to block such changes
                //as a result, all complex property member accesses must ne made to the last property in projection queries.
                var nfpInfo = Utils.Utilities.GetMethodInfo(() => ObjectExtensions._<object>(null));
                if (bodyExpr is NewExpression newExpr)
                {
                    foreach (var arg in newExpr.Arguments)
                    {
                        //i.e., arg._()
                        replacements.Add(arg, Expression.Call(Utils.Utilities.GetGenericMethodInfo(nfpInfo, arg.Type), arg));
                    }
                }
                else if (bodyExpr is MemberInitExpression memberInitExpr)
                {
                    Action<MemberBinding> addBinding = null;
                    addBinding = (binding) =>
                    {
                        switch (binding)
                        {
                            case MemberAssignment assignment:
                                {
                                    //i.e., a = b._()
                                    replacements.Add(assignment.Expression,
                                        Expression.Call(Utils.Utilities.GetGenericMethodInfo(nfpInfo, assignment.Expression.Type), assignment.Expression));
                                    break;
                                }
                            case MemberMemberBinding memberMemberBinding:
                                {
                                    foreach (var childBinding in memberMemberBinding.Bindings)
                                    {
                                        addBinding(childBinding);
                                    }
                                    break;
                                }
                        }
                    };

                    foreach (var binding in memberInitExpr.Bindings)
                    {
                        addBinding(binding);
                    }
                }

                //make the remaining replacements
                bodyExpr = replacerVisitor.Visit(bodyExpr);
                //create new lambda as we don't even need the parameters anymore.
                //i.e., () => bodyExpr
                expression = Expression.Lambda(bodyExpr);

                var finalProperties = CypherUtilities.GetFinalProperties(expression, queryContext, out bool hasFunctions);
                if (finalProperties == null)
                    //trouble
                    throw new InvalidOperationException(Messages.InvalidICypherResultItemExpressionError);

                string asterisk = "*";
                var serializer = queryContext.SerializeCallback;

                var buildStrategy = queryContext.CurrentBuildStrategy ?? PropertiesBuildStrategy.WithParams;
                var hasQueryWriter = queryContext.CurrentQueryWriter != null;

                result = finalProperties.Properties().Select(jp =>
                {
                    var value = serializer(jp.Value);

                    //the asterisk wild-card cannot have a name associated with it, and should ideally be the first parameter
                    if (value == asterisk)
                        return asterisk;

                    if (hasQueryWriter && jp.Value.Type != JTokenType.Raw)
                    {
                        //i.e. a constant/literal
                        //we don't just write constants directly to the stream
                        //so consider the build strategy here
                        switch (buildStrategy)
                        {
                            case PropertiesBuildStrategy.WithParams:
                            case PropertiesBuildStrategy.WithParamsForValues:
                                {
                                    //use a parameter to reference the value
                                    value = queryContext.CurrentQueryWriter.CreateParameter(value);
                                    value = Utils.Utilities.GetNewNeo4jParameterSyntax(value);
                                    break;
                                }
                        }
                    }

                    return $"{value} AS {jp.Name}";

                }).Aggregate((first, second) => $"{first}, {second}");

                resultMode = CypherResultMode.Projection;
            }
            else
            {
                //visit the expression directly
                var visitor = functionVisitor;
                visitor.Clear();
                var newBodyExpr = visitor.Visit(bodyExpr);
                result = visitor.Builder.ToString();

                resultMode = CypherResultMode.Set;
            }

            return result;
        }

        internal static void TraverseEntityPath
            (IEntityService entityService,
            object entity, List<Expression> pathExpressions,
            ref int index, out Type lastType,
            out Dictionary<MemberInfo, Tuple<int, Type>> pathTraversed,
            bool buildPath)
        {
            lastType = null;

            pathTraversed = new Dictionary<MemberInfo, Tuple<int, Type>>();

            object currentInstance = null;
            object nextInstance = entity;

            MemberInfo memberInfo = null;
            PropertyInfo propInfo = null;
            FieldInfo fieldInfo = null;

            bool breakLoop = false;

            if (index < 0) index = 0; //maybe shouldn't auto-handle this.

            for (int i = index, l = pathExpressions.Count; i < l; i++)
            {
                var expr = pathExpressions[i];

                switch (expr.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        {
                            var memberAccessExpr = expr as MemberExpression;
                            memberInfo = memberAccessExpr.Member;
                            propInfo = memberInfo as PropertyInfo;
                            fieldInfo = memberInfo as FieldInfo;

                            lastType = propInfo?.PropertyType ?? fieldInfo?.FieldType ?? lastType;

                            currentInstance = nextInstance;
                            nextInstance = null;
                            break;
                        }
                    case ExpressionType.TypeAs:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.Unbox:
                        {
                            lastType = (expr as UnaryExpression).Type ?? lastType;
                            break;
                        }
                    //case ExpressionType.Call when (expr is MethodCallExpression callExpr
                    //&& (callExpr.Method.Name.StartsWith("_As")
                    //&& callExpr.Method.DeclaringType == Defaults.ObjectExtensionsType)):
                    //    {
                    //        lastType = callExpr.Type;
                    //        break;
                    //    }
                    default:
                        {
                            breakLoop = true;
                            break;
                        }
                }

                if (breakLoop || (propInfo == null && fieldInfo == null) || lastType == null)
                {
                    break;
                }

                pathTraversed[memberInfo] = new Tuple<int, Type>(i, lastType);

                index = i; //update the current index always

                if (buildPath)
                {
                    //get the existing member instance
                    nextInstance = propInfo != null ? propInfo.GetValue(currentInstance) : fieldInfo.GetValue(currentInstance);

                    if (!lastType.GetTypeInfo().IsInterface && !lastType.GetTypeInfo().IsAbstract)
                    {
                        var nextType = nextInstance?.GetType();
                        if (nextInstance == null || (nextType != lastType && !lastType.IsInstanceOfType(nextInstance)))
                        {
                            if (lastType.IsArray)
                            {
                                int rank = lastType.GetArrayRank();
                                int[] lengths = new int[rank];
                                for (int j = 0; j < rank; j++)
                                {
                                    lengths[j] = 1;
                                }

                                nextInstance = Array.CreateInstance(lastType.GetElementType(), lengths);
                            }
                            else if (Utils.Utilities.IsScalarType(lastType, entityService))
                            {
                                nextInstance = lastType.GetDefaultValue();
                            }
                            else
                            {
                                nextInstance = Utils.Utilities.CreateInstance(lastType);
                            }

                            //assign the instance
                            if (propInfo != null)
                            {
                                propInfo.SetValue(currentInstance, nextInstance);
                            }
                            else if (fieldInfo != null)
                            {
                                fieldInfo.SetValue(currentInstance, nextInstance);
                            }
                        }
                    }
                }
            }
        }

        internal static string[] GetEntityPathNames
            (IEntityService entityService,
            ref object entity, ref Type entityType,
            List<Expression> expressions,
            ref int currentIndex, EntityResolver resolver, Func<object, string> serializer,
            out Dictionary<MemberInfo, Tuple<int, Type>> members, out Type lastType, bool useResolvedJsonName = true)
        {
            string[] memberNames = new string[0];

            entityType = entity?.GetType() ?? entityType;
            var entityInfo = entityService.GetEntityTypeInfo(entityType);

            //do this to avoid create instances every time we call this method for a particular type
            bool buildPath = useResolvedJsonName && resolver == null && entityInfo.JsonNamePropertyMap.Count == 0;

            int index = currentIndex; //store this index incase of a repeat

            repeatBuild:
            if (buildPath && entity == null)
            {
                //most likely using the EntityConverter
                //create new instance
                entity = Utils.Utilities.CreateInstance(entityType);
            }

            currentIndex = index;

            TraverseEntityPath(entityService, entity, expressions, ref currentIndex, out lastType,
                out members, buildPath: buildPath);

            if (members.Count > 0)
            {
                if (buildPath)
                {
                    //take care of the entity's complex properties and those of its children
                    Utils.Utilities.InitializeComplexTypedProperties(entity, entityService);

                    bool entityTypeAutoAdded = false;

                    if (entityTypeAutoAdded = !entityService.EntityTypes.Contains(entityType))
                    {
                        entityService.AddEntityType(entityType); //just in case it wasn't already added.
                    }

                    //serialize the entity so the jsonnames would be set
                    var serializedEntity = serializer(entity);

                    if (entityTypeAutoAdded)
                    {
                        //remove it
                        entityService.RemoveEntityType(entityType);
                    }
                }

                if (resolver != null)
                {
                    //force the propertymap to be set
                    entityInfo.WithJsonResolver(resolver);
                }

                //first get the members that are not complex typed.
                //this is because their child members would be the actual member of interest
                //if none found, or name returns empty, try all members then.

                var membersToUse = members.Where(m => !(((m.Key as PropertyInfo)?.PropertyType ??(m.Key as FieldInfo)?.FieldType)?.IsComplex() == true))
                    .OrderBy(m => m.Value.Item1).ToList();

                bool repeatedMemberNames = false;

                repeatMemberNames:
                bool gotoRepeatBuild = false;

                memberNames = membersToUse.Select((m, idx) =>
                {
                    string name = null;
                    if (useResolvedJsonName)
                    {
                        Type parentType = null;

                        //try the entityInfo first
                        name = entityInfo.JsonNamePropertyMap.FirstOrDefault(pm => pm.Value.IsEquivalentTo(m.Key)).Key?.Json;

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            //try to get name from entity info
                            if (idx >= 1)
                            {
                                parentType = membersToUse[idx - 1].Value?.Item2;
                            }

                            parentType = parentType ?? //m.Value?.Item2?.DeclaringType ?? 
                                m.Key.DeclaringType;

                            //we do this because MemberInfo.ReflectedType is not public yet in the .NET Core API.
                            var infos = entityService.GetDerivedEntityTypeInfos(parentType);

                            if (resolver != null)
                            {
                                foreach (var info in infos)
                                {
                                    info.WithJsonResolver(resolver);
                                }
                            }

                            var result = infos.SelectMany(info => info.JsonNamePropertyMap)
                            .ExactOrEquivalentMember((pair) => pair.Value, new KeyValuePair<MemberName, MemberInfo>(MemberName.Empty, m.Key))
                            .FirstOrDefault();

                            name = result.Key?.Json;
                        }

                        if (!buildPath
                        && resolver == null //don't doubt the result if the EntityResolver was present
                        && (string.IsNullOrWhiteSpace(name)
                        || (name == m.Key.Name
                            && (parentType?.IsComplex() == true
                            || m.Key.DeclaringType.IsComplex()
                            || m.Value.Item2.IsComplex())))
                        )
                        {
                            //maybe we were wrong and the jsonMaps are empty
                            //or just to be sure we have the actual name,
                            //repeat with a fully built path and serialization
                            buildPath = true;
                            gotoRepeatBuild = true;
                        }
                        else if (string.IsNullOrWhiteSpace(name))
                        {
                            //fallback for when name couldn't be resolved in all attempts
                            name = m.Key.Name;
                        }
                    }
                    else
                    {
                        name = m.Key.Name;
                    }

                    return name;
                }).Where(n => !string.IsNullOrWhiteSpace(n)).ToArray();

                if (gotoRepeatBuild)
                {
                    goto repeatBuild;
                }

                if (!repeatedMemberNames && memberNames.Length == 0)
                {
                    //repeat with all members
                    membersToUse = members.OrderBy(m => m.Value.Item1).ToList();
                    repeatedMemberNames = true;
                    goto repeatMemberNames;
                }
            }

            return memberNames;
        }

        internal static List<Expression> ExplodeComplexTypeMemberAccess
            (IEntityService entityService,
            Expression expression, out List<List<MemberInfo>> inversePaths)
        {
            return ExplodeComplexTypeAndMemberAccess(entityService, ref expression,
                expression.Type, out inversePaths, shouldTryCast: true);
        }

        internal static void ExplodeComplexType
            (IEntityService entityService, Type type, out List<List<MemberInfo>> inversePaths)
        {
            Expression empty = null;
            ExplodeComplexTypeAndMemberAccess(entityService, ref empty, type, out inversePaths, shouldTryCast: false);
        }

        internal static List<Expression> ExplodeComplexTypeAndMemberAccess
            (IEntityService entityService,
            ref Expression expression, Type type,
            out List<List<MemberInfo>> inversePaths, bool shouldTryCast = true)
        {
            inversePaths = new List<List<MemberInfo>>();

            bool exprNotNull = expression != null;

            type = expression?.Type ?? type;

            if (shouldTryCast && exprNotNull)
            {
                try
                {
                    expression = expression.Cast(out var newType);
                    type = newType ?? type;
                }
                catch
                {
                    shouldTryCast = false;
                }
            }

            if (type == null || !type.IsComplex())
                return new List<Expression>();

            var result = new List<Expression>();

            var info = entityService.GetEntityTypeInfo(type);

            foreach (var prop in info.AllProperties)
            {
                Expression memberExpr = exprNotNull ? Expression.MakeMemberAccess(expression, prop) : null;

                if (Utils.Utilities.IsScalarType(prop.PropertyType, entityService))
                {
                    result.Add(memberExpr);
                    inversePaths.Add(new List<MemberInfo>() { prop });
                }
                else
                {
                    //recursively check till we hit the last scalar property
                    var memberRes = ExplodeComplexTypeAndMemberAccess(entityService,
                        ref memberExpr, prop.PropertyType, out var members, shouldTryCast: shouldTryCast && exprNotNull);
                    inversePaths.AddRange(members.Select(ml => { ml.Add(prop); return ml; }));

                    result.AddRange(memberRes);
                }
            }

            return result;
        }

        public static string BuildSimpleVars(List<Expression> expressions, QueryContext queryContext, bool? useResolvedJsonName = null)
        {
            //typeReturned = null;
            if (!Utils.Utilities.HasVars(expressions, out var methodExpr))
                return null;

            if (useResolvedJsonName == null)
            {
                //check the last expression for nfp escape
                useResolvedJsonName = !Utils.Utilities.HasNfpEscape(expressions.Last().Uncast(out var castType));
            }

            var getMethod = methodExpr.Method;

            var entityType = /*typeReturned =*/ getMethod.ReturnType;

            var builder = new StringBuilder();

            var currentIndex = -1;

            //append the variable name first
            var argumentExpr = methodExpr.Arguments[++currentIndex];

            string argument = null;

            try
            {
                argument = argumentExpr.ExecuteExpression<string>();
            }
            catch
            {
                //maybe lamdaexpression
                var visitor = new FunctionExpressionVisitor(queryContext);
                visitor.Context.UseResolvedJsonName = useResolvedJsonName;
                visitor.Visit(argumentExpr);
                argument = visitor.Builder.ToString();
            }

            builder.Append(argument);

            //append member
            if (expressions.Count > ++currentIndex)
            {
                var tmpIdx = currentIndex;

                string memberJsonName = null;

                if (entityType != Defaults.CypherObjectType)
                {
                    //most likely the generic method was called
                    //build the entity through its members accessed
                    object entity = null; //Utilities.CreateInstance(entityType);

                    var memberNames = GetEntityPathNames(queryContext.EntityService,
                        ref entity, ref entityType, expressions, ref currentIndex,
                        queryContext.Resolver, queryContext.SerializeCallback,
                        out var members, out var lastType, useResolvedJsonName: useResolvedJsonName.Value);

                    if (memberNames.Length > 0)
                    {
                        memberJsonName = memberNames.Aggregate((first, second) => $"{first}.{second}").Trim('.');
                        //typeReturned = lastType ?? typeReturned;
                    }
                }
                else
                {
                    //cypherobject
                    //check for a get_Item method call expression (which would be the indexer)
                    var expr = expressions[currentIndex] as MethodCallExpression;

                    if (expr != null)
                    {
                        var memberArgExpr = expr.Arguments[0];
                        memberJsonName = memberArgExpr.ExecuteExpression<string>();

                        //typeReturned = expr.Method.ReturnType;
                    }
                }

                if (!string.IsNullOrWhiteSpace(memberJsonName))
                {
                    builder.Append("." + memberJsonName);
                }
                else
                {
                    //restore last index less 1.
                    currentIndex = tmpIdx - 1;
                }
            }

            return builder.ToString().Trim();
        }

        public static List<Expression> GetSimpleMemberAccessStretch
            (IEntityService entityService, Expression expression, out Expression entityBestGuess)
        {
            return GetSimpleMemberAccessStretch(entityService, expression, out entityBestGuess, out var isContinuous);
        }

        public static List<Expression> GetSimpleMemberAccessStretch
            (IEntityService entityService, 
            Expression expression, out Expression entityBestGuess, out bool isContinuous)
        {
            isContinuous = true;

            var filtered = new List<Expression>();

            var currentExpression = expression;

            Expression localExpr = null, entityDisjointExpr = null, nfpExpr = null,
                methodExpr = null, parentEntityExpr = null, paramExpr = null, constExpr = null;

            while (currentExpression != null)
            {
                filtered.Add(currentExpression);

                bool currentExprIsEntity = currentExpression.IsEntity(entityService);

                if (currentExprIsEntity)
                {
                    if (entityDisjointExpr != null)
                    {
                        //this would be a lie now, because we have a parent expression that is an entity
                        //so set to null.
                        entityDisjointExpr = null;
                    }

                    if (currentExpression != expression)
                        parentEntityExpr = currentExpression;
                }

                switch (currentExpression.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        {
                            var memberExpr = (currentExpression as MemberExpression);

                            if (memberExpr.IsLocalMember())
                            {
                                localExpr = memberExpr;
                            }
                            else if (memberExpr.Expression == null
                                || (currentExprIsEntity
                                && !memberExpr.Expression.Type.IsAnonymousType()
                                && !memberExpr.Expression.IsEntity(entityService))
                                )
                            {
                                entityDisjointExpr = memberExpr;
                            }
                            else if (memberExpr.Expression.IsEntity(entityService))
                            {
                                parentEntityExpr = memberExpr.Expression;
                            }

                            currentExpression = memberExpr.Expression;
                            break;
                        }
                    case ExpressionType.TypeAs:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.Unbox:
                        {
                            currentExpression = (currentExpression as UnaryExpression).Operand;
                            break;
                        }
                    case ExpressionType.Call:
                        {
                            var methodCallExpr = (currentExpression as MethodCallExpression);
                            currentExpression = methodCallExpr.Object;

                            if (currentExpression == null //maybe extension method
                                && methodCallExpr.Method.IsExtensionMethod())
                            {
                                if (Utils.Utilities.HasNfpEscape(methodCallExpr))
                                    nfpExpr = methodCallExpr;

                                //pick first argument
                                currentExpression = methodCallExpr.Arguments[0];
                            }

                            methodExpr = methodCallExpr;

                            break;
                        }
                    case ExpressionType.ArrayIndex:
                        {
                            currentExpression = (currentExpression as BinaryExpression).Left;
                            break;
                        }
                    case ExpressionType.ArrayLength:
                        {
                            currentExpression = (currentExpression as UnaryExpression).Operand;
                            break;
                        }
                    case ExpressionType.Parameter:
                        {
                            paramExpr = currentExpression;
                            currentExpression = null;
                            break;
                        }
                    case ExpressionType.Constant:
                        {
                            constExpr = currentExpression;
                            currentExpression = null;
                            break;
                        }
                    default:
                        {
                            isContinuous = false;
                            currentExpression = null;
                            break;
                        }
                }
            }

            filtered.Reverse();

            //determine where our value is by some heuristics
            entityBestGuess = nfpExpr ?? constExpr ?? paramExpr ?? entityDisjointExpr ?? localExpr ?? methodExpr ?? parentEntityExpr ?? filtered.FirstOrDefault();

            return filtered;
        }

        internal static int GetFirstMemberAccessIndex(List<Expression> exprs, Expression entityVal, out Expression firstAccessExpr)
        {
            firstAccessExpr = exprs.SkipWhile(a => a == entityVal || a.NodeType != ExpressionType.MemberAccess).FirstOrDefault();
            return exprs.IndexOf(firstAccessExpr);
        }
    }
}
