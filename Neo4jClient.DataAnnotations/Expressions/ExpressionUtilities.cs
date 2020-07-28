using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Utils;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public static class ExpressionUtilities
    {
        /// <summary>
        ///     Expecting:
        ///     a =&gt; a.Property //member access = "a.property"//
        ///     a =&gt; new { a.Property1, a.Property2 } //multiple member accesses = "a.property1", "a.property2"//
        ///     a =&gt; a.Property.ToString() //complex expression (i.e., member access with function calls) =
        ///     "toString(a.property)"//
        ///     a =&gt; new { a.Property1, Property2 = a.Property2.Length } //multiple complex expressions = "a.property1",
        ///     "size(a.property2)"//
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressions"></param>
        /// <param name="isMemberAccess">If false, the variable is not included</param>
        internal static string[] GetVariableExpressions<T>(Expression<Func<T, object>> expressions,
            QueryContext queryContext,
            bool isMemberAccess = true, string variable = null,
            FunctionVisitorContext visitorContext = null)
        {
            return GetVariableExpressions(typeof(T), expressions, queryContext, isMemberAccess, variable,
                visitorContext);
        }

        /// <summary>
        ///     Expecting:
        ///     a =&gt; a.Property //member access = "a.property"//
        ///     a =&gt; new { a.Property1, a.Property2 } //multiple member accesses = "a.property1", "a.property2"//
        ///     a =&gt; a.Property.ToString() //complex expression (i.e., member access with function calls) =
        ///     "toString(a.property)"//
        ///     a =&gt; new { a.Property1, Property2 = a.Property2.Length } //multiple complex expressions = "a.property1",
        ///     "size(a.property2)"//
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

            //create a CypherVariables.Get call for this variable
            var varsGetCallExpr = GetVarsGetExpressionFor(randomVar, sourceType);

            //get the body without an object cast if present
            var bodyExpr = expressions.Body.Uncast(out var cast, typeof(object));

            //replace the parameter with the a CypherVariables.Get call
            var newBodyExpr = new ParameterReplacerVisitor(
                new Dictionary<string, Expression> { { parameterExpr.Name, varsGetCallExpr } }
            ).Visit(bodyExpr);

            var variableExpressions = new List<Expression> { newBodyExpr };

            if (newBodyExpr.Type.IsAnonymousType())
            {
                //that is, new { a.Property1, a.Property2 }
                variableExpressions.Clear();
                variableExpressions.AddRange((newBodyExpr as NewExpression).Arguments);
            }

            var values = new List<string>();
            var funcsVisitor = new FunctionExpressionVisitor(queryContext, visitorContext);

            Action<Expression> visit = expr =>
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
                        foreach (var item in accesses) visit(item);

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
            //create a CypherVariables.Get call for this variable
            var varsGetMethodInfo =
                Utils.Utilities.GetGenericMethodInfo(
                    Utils.Utilities.GetMethodInfo(() => CypherVariables.Get<object>(null)), type);
            //now CypherVariables.Get<type>(variable)
            var varsGetCallExpr =
                Expression.Call(varsGetMethodInfo, Expression.Constant(variable, Defaults.StringType));

            return varsGetCallExpr;
        }

        public static MethodCallExpression GetVarsGetExpressionFor<TSource, TResult>(
            Expression<Func<TSource, TResult>> selector)
        {
            return GetVarsGetExpressionFor(selector, typeof(TSource), typeof(TResult));
        }

        public static MethodCallExpression GetVarsGetExpressionFor(LambdaExpression selector, Type source, Type result)
        {
            //create a CypherVariables.Get call for this selector
            var varsGetMethodInfo = Utils.Utilities.GetGenericMethodInfo(
                Utils.Utilities.GetMethodInfo(() => CypherVariables.Get<object, object>(null)), source, result);
            //now CypherVariables.Get<source, result>(selector)
            var varsGetCallExpr = Expression.Call(varsGetMethodInfo, selector);

            return varsGetCallExpr;
        }

        internal static string BuildProjectionQueryExpression(LambdaExpression expression,
            QueryContext queryContext, FunctionExpressionVisitor functionVisitor,
            bool isOutputQuery,
            out CypherResultMode resultMode, out CypherResultFormat resultFormat)
        {
            //expecting:
            //u => u //parameter
            //u => u.Name //memberaccess if complextype, should return object
            //(u,v) => new { u, v } //new anonymous expression
            //(u, v) => new User(){ Name = u.Id() } //member init

            resultFormat = CypherResultFormat.DependsOnEnvironment;

            var originalExpression = expression;

            var bodyExpr = expression.Body.Uncast(out var bodyCast, Defaults.ObjectType);

            //use final properties method
            //we have to convert all expressions to dictionary, theen process it that way

            //but first replace all parameters with vars.get call
            var replacements = new Dictionary<Expression, Expression>();
            var replacerVisitor = new ReplacerExpressionVisitor(replacements);
            if (expression.Parameters?.Count > 0)
            {
                foreach (var p in expression.Parameters) replacements.Add(p, GetVarsGetExpressionFor(p.Name, p.Type));
                //make parameter replacements now
                bodyExpr = replacerVisitor.Visit(bodyExpr);
                replacements.Clear();
            }

            var nfpInfo = Defaults.NfpExtMethodInfo;

            var dictItems = new List<ElementInit>();
            var members = new List<string>();

            Dictionary<string, Expression> complexTypeMembers = null;

            Action<string, Expression> AddDictItem = (member, argument) =>
            {
                if (argument.Type.IsComplex())
                {
                    complexTypeMembers = complexTypeMembers ?? new Dictionary<string, Expression>();
                    complexTypeMembers[member] = argument;
                }

                //add the name escape method
                argument = Expression.Call(Utils.Utilities.GetGenericMethodInfo(nfpInfo, argument.Type), argument);

                dictItems.Add(Expression.ElementInit
                (Defaults.DictStringObjectAddMethod, Expression.Constant(member),
                    argument.Type != Defaults.ObjectType
                        ? Expression.Convert(argument, Defaults.ObjectType)
                        : argument));

                members.Add(member);
            };

            var isProjection = false;

            switch (bodyExpr)
            {
                case MemberExpression memberExpression:
                    {
                        //u => u.Name //memberaccess if complextype, should return object
                        AddDictItem(memberExpression.Member.Name, memberExpression);
                        isProjection = false;

                        break;
                    }
                case MemberInitExpression memberInitExpression:
                    {
                        //(u, v) => new User(){ Name = u.Id() } //member init

                        //only the declared members matter
                        foreach (var binding in memberInitExpression.Bindings)
                            switch (binding)
                            {
                                case MemberAssignment assignment:
                                    {
                                        AddDictItem(assignment.Member.Name, assignment.Expression);
                                        break;
                                    }
                                default:
                                    {
                                        throw new InvalidOperationException(string.Format(Messages.InvalidProjectionError,
                                            binding.Member.Name));
                                    }
                            }

                        isProjection = true;

                        break;
                    }
                case NewExpression newExpression when bodyExpr.Type.IsAnonymousType():
                    {
                        //(u,v) => new { u, v } //new anonymous expression
                        for (var i = 0; i < newExpression.Arguments.Count; i++)
                        {
                            //generate the expression entry
                            var argument = newExpression.Arguments[i];
                            var member = newExpression.Members[i].Name;

                            if (argument != null && member != null) AddDictItem(member, argument);
                        }

                        isProjection = true;

                        break;
                    }
                default:
                    {
                        //use a generic member name
                        AddDictItem("projection", bodyExpr);
                        isProjection = false;

                        break;
                    }
            }

            expression =
                Expression.Lambda(Expression.ListInit(Expression.New(Defaults.DictStringObjectType), dictItems));

            string result = null;

            var finalProperties = CypherUtilities.GetFinalProperties(expression, queryContext, out var hasFunctions);
            if (finalProperties == null)
                //trouble
                throw new InvalidOperationException(Messages.InvalidICypherResultItemExpressionError);

            var serializer = queryContext.SerializeCallback;
            var buildStrategy = queryContext.CurrentBuildStrategy ?? PropertiesBuildStrategy.WithParams;
            var hasQueryWriter = queryContext.CurrentQueryWriter != null;

            Func<JToken, JToken> getProcessedJToken = jToken =>
            {
                if (hasQueryWriter && jToken.Type != JTokenType.Raw)
                    //i.e. a constant/literal
                    //we don't just write constants directly to the stream
                    //so consider the build strategy here
                    switch (buildStrategy)
                    {
                        case PropertiesBuildStrategy.WithParams:
                        case PropertiesBuildStrategy.WithParamsForValues:
                            {
                                //use a parameter to reference the value
                                jToken = queryContext.CurrentQueryWriter.CreateParameter(jToken);
                                jToken = new JRaw(Utils.Utilities.GetNewNeo4jParameterSyntax((string)jToken));
                                break;
                            }
                    }

                return jToken;
            };

            //first check for complex properties
            var simplifiedComplexMembers = false;
            if ((isOutputQuery || isProjection) && complexTypeMembers?.Count > 0)
            {
                simplifiedComplexMembers = true;
                var entityService = queryContext.EntityService;

                foreach (var complexAssignment in complexTypeMembers)
                {
                    var complexNamePrefix = complexAssignment.Key + Defaults.ComplexTypeNameSeparator;

                    var allPossibleJProps = finalProperties.Properties()
                        .Where(jp => jp.Name.StartsWith(complexNamePrefix))
                        .ToArray();

                    if (allPossibleJProps.Length == 0)
                        continue; //take no actions

                    //make them into a jobject;

                    var derivedTypeInfos = entityService.GetDerivedEntityTypeInfos(complexAssignment.Value.Type);
                    var allPossibleMemberNames = derivedTypeInfos.SelectMany(ti => ti.JsonNamePropertyMap.Keys)
                        .Select(mn => mn.ComplexJson ?? mn.Json).Distinct().ToList();

                    var isFirst = true;
                    var complexNamePrefixLength = complexNamePrefix.Length;

                    var complexJProperty = new JProperty(complexAssignment.Key, null);
                    var complexValueStr = "";

                    foreach (var jp in allPossibleJProps)
                    {
                        var actualName = jp.Name.Substring(complexNamePrefixLength);

                        if (!allPossibleMemberNames.Contains(actualName))
                            continue;

                        if (isFirst)
                        {
                            isFirst = false;

                            if (finalProperties.Properties().FirstOrDefault(_jp => _jp.Name == complexJProperty.Name) is
                                    JProperty existing
                                && existing?.Name != null)
                                //if it already contains this value then we have a problem
                                throw new InvalidOperationException(string.Format(Messages.DuplicateProjectionKeyError,
                                    originalExpression, complexJProperty.Name));

                            jp.AddAfterSelf(complexJProperty);
                            complexValueStr = $"{actualName}: {getProcessedJToken(jp.Value)}";
                        }
                        else
                        {
                            complexValueStr += $", {actualName}: {getProcessedJToken(jp.Value)}";
                        }

                        jp.Remove();
                    }

                    complexJProperty.Value = new JRaw($"{{ {complexValueStr} }}");
                }
            }

            var asterisk = "*";

            result = finalProperties.Properties()
                .Where(jp =>
                    members.Contains(jp.Name) || !simplifiedComplexMembers && members.Any(m => jp.Name.StartsWith(m)))
                //the names must always match for a projection. we want nothing less.
                //if not projection, i.e., we omit names, then check that its key at least starts with one of the member names.
                //this would enable us escape metadata properties
                .Select(jp =>
                {
                    var jToken = jp.Value; //serializer(jp.Value);

                    //the asterisk wild-card cannot have a name associated with it, and should ideally be the first parameter
                    if (jToken.Type == JTokenType.Raw && ((JRaw)jToken).Value.ToString() == asterisk)
                        return asterisk;

                    jToken = getProcessedJToken(jToken);

                    if (isProjection) return $"{jToken} AS {jp.Name}";

                    return (string)jToken;
                }).Aggregate((first, second) => $"{first}, {second}");

            //result = "u.PhoneNumber_Value AS PhoneNumber_Value, u.PhoneNumber_ConfirmationRecord_Instant AS PhoneNumber_ConfirmationRecord_Instant, u.UserName AS UserName";
            //result = "{ Value: u.PhoneNumber_Value, ConfirmationRecord_Instant: u.PhoneNumber_ConfirmationRecord_Instant } AS PhoneNumber, u.UserName as UserName";

            resultMode = !isProjection ? CypherResultMode.Set : CypherResultMode.Projection;

            //if (bodyExpr.NodeType == ExpressionType.MemberInit
            //    || bodyExpr.NodeType == ExpressionType.New)
            //{


            //    Dictionary<MemberInfo, Expression> complexTypeMembers = new Dictionary<MemberInfo, Expression>();

            //    //then we need to seal all assignments with the nfp method ("_") so that they don't affect the final properties key names.
            //    //the original key/member names set are important to the deserialization method (and to the rest of the user code)
            //    //hence why a change here won't be appropriate and we need to block such changes
            //    //as a result, all complex property member accesses must ne made to the last property in projection queries.
            //    if (bodyExpr is NewExpression newExpr)
            //    {
            //        int i = -1;

            //        foreach (var arg in newExpr.Arguments)
            //        {
            //            var newArg = arg;

            //            i++;

            //            if (arg.Type.IsComplex())
            //            {
            //                complexTypeMembers[newExpr.Members[i]] = arg;
            //                //replace arg with default value instead
            //                newArg = Expression.Constant(Utils.Utilities.CreateInstance(arg.Type), arg.Type);
            //            }

            //            //i.e., arg._()
            //            replacements.Add(arg, Expression.Call(Utils.Utilities.GetGenericMethodInfo(nfpInfo, newArg.Type), newArg));
            //        }
            //    }
            //    else if (bodyExpr is MemberInitExpression memberInitExpr)
            //    {
            //        Action<MemberBinding> addBinding = null;
            //        addBinding = (binding) =>
            //        {
            //            switch (binding)
            //            {
            //                case MemberAssignment assignment:
            //                    {
            //                        var newAssignmentExpr = assignment.Expression;

            //                        if (assignment.Expression.Type.IsComplex())
            //                        {
            //                            complexTypeMembers[assignment.Member] = assignment.Expression;
            //                            //newAssignmentExpr = Expression.Constant
            //                            //    (Utils.Utilities.CreateInstance(assignment.Expression.Type),
            //                            //    assignment.Expression.Type);
            //                        }

            //                        //i.e., a = b._()
            //                        replacements.Add(assignment.Expression,
            //                            Expression.Call(Utils.Utilities.GetGenericMethodInfo(nfpInfo, newAssignmentExpr.Type), newAssignmentExpr));

            //                        break;
            //                    }
            //                default:
            //                    {
            //                        throw new InvalidOperationException(string.Format(Messages.InvalidProjectionError, binding.Member.Name));
            //                    }
            //                //case MemberMemberBinding memberMemberBinding:
            //                //    {
            //                //        foreach (var childBinding in memberMemberBinding.Bindings)
            //                //        {
            //                //            addBinding(childBinding);
            //                //        }
            //                //        break;
            //                //    }
            //            }
            //        };

            //        foreach (var binding in memberInitExpr.Bindings)
            //        {
            //            addBinding(binding);
            //        }
            //    }

            //    //make the remaining replacements
            //    bodyExpr = replacerVisitor.Visit(bodyExpr);
            //    //create new lambda as we don't even need the parameters anymore.
            //    //i.e., () => bodyExpr
            //    expression = Expression.Lambda(bodyExpr);

            //    var finalProperties = CypherUtilities.GetFinalProperties(expression, queryContext, out bool hasFunctions);
            //    if (finalProperties == null)
            //        //trouble
            //        throw new InvalidOperationException(Messages.InvalidICypherResultItemExpressionError);

            //    string asterisk = "*";
            //    var serializer = queryContext.SerializeCallback;

            //    var buildStrategy = queryContext.CurrentBuildStrategy ?? PropertiesBuildStrategy.WithParams;
            //    var hasQueryWriter = queryContext.CurrentQueryWriter != null;
            //    var hasComplexTypeMembers = complexTypeMembers?.Count > 0;

            //    result = finalProperties.Properties().Select(jp =>
            //    {
            //        var jToken = jp.Value; //serializer(jp.Value);

            //        //the asterisk wild-card cannot have a name associated with it, and should ideally be the first parameter
            //        if (jToken.Type == JTokenType.Raw && ((JRaw)jToken).Value.ToString() == asterisk)
            //            return asterisk;

            //        if (hasComplexTypeMembers
            //            && complexTypeMembers.FirstOrDefault(m => m.Key.Name == jp.Name) is var complexAssignment
            //            && complexAssignment.Key != null)
            //        {
            //            //get the final properties for this member
            //            //first make if a constraint
            //            Func<string, bool> f = (_) => _.ToString() != null;
            //            var param = Expression.Parameter(complexAssignment.Key.ReflectedType, "_");
            //            var paramMemberAccess = Expression.MakeMemberAccess(param, complexAssignment.Key);
            //            var equalityExpr = Expression.Equal(paramMemberAccess, complexAssignment.Value);
            //            var predicateExpr = Expression.Lambda(equalityExpr, param);

            //            var properties = CypherUtilities.GetConstraintsAsPropertiesLambda(predicateExpr, param.Type);

            //            var newValue = CypherUtilities.BuildFinalProperties
            //                (queryContext, jp.Name, properties, 
            //                ref buildStrategy, out var parameter, out var newProperties,
            //                out var finalPropsHasFunctions, separator: ": ",
            //                parameterSeed: $"{jp.Name}_prj",
            //                useVariableMemberAccessAsKey: false,
            //                useVariableAsParameter: false,
            //                wrapValueInJsonObjectNotation: true);

            //            jp.Value = new JRaw($"{{ {newValue} }}");
            //        }
            //        else if (hasQueryWriter && jp.Value.Type != JTokenType.Raw)
            //        {
            //            //i.e. a constant/literal
            //            //we don't just write constants directly to the stream
            //            //so consider the build strategy here
            //            switch (buildStrategy)
            //            {
            //                case PropertiesBuildStrategy.WithParams:
            //                case PropertiesBuildStrategy.WithParamsForValues:
            //                    {
            //                        //use a parameter to reference the value
            //                        jToken = queryContext.CurrentQueryWriter.CreateParameter(jToken);
            //                        jToken = Utils.Utilities.GetNewNeo4jParameterSyntax((string)jToken);
            //                        break;
            //                    }
            //            }
            //        }

            //        return $"{jToken} AS {jp.Name}";

            //    }).Aggregate((first, second) => $"{first}, {second}");

            //    //result = "u.PhoneNumber_Value AS PhoneNumber_Value, u.PhoneNumber_ConfirmationRecord_Instant AS PhoneNumber_ConfirmationRecord_Instant, u.UserName AS UserName";
            //    //result = "{ Value: u.PhoneNumber_Value, ConfirmationRecord_Instant: u.PhoneNumber_ConfirmationRecord_Instant } AS PhoneNumber, u.UserName as UserName";

            //    resultMode = CypherResultMode.Projection;
            //}
            //else
            //{
            //    //visit the expression directly
            //    var visitor = functionVisitor;
            //    visitor.Clear();
            //    var newBodyExpr = visitor.Visit(bodyExpr);
            //    result = visitor.Builder.ToString();

            //    resultMode = CypherResultMode.Set;
            //}

            return result;
        }

        internal static void TraverseEntityPath
        (EntityService entityService,
            object entity, Type entityType, List<Expression> pathExpressions,
            ref int index, out Type lastType,
            out Dictionary<EntityMemberInfo, int>
                pathTraversed, //Dictionary<EntityMemberInfo, (int PathIndex, Type LastType)> pathTraversed,
            bool buildPath)
        {
            lastType = null;

            pathTraversed = new Dictionary<EntityMemberInfo, int>(); //(int Item1, Type Item2)>();

            object currentInstance = null;
            var nextInstance = entity;

            EntityMemberInfo memberInfo = null;
            PropertyInfo propInfo = null;
            FieldInfo fieldInfo = null;

            var breakLoop = false;

            if (index < 0) index = 0; //maybe shouldn't auto-handle this.

            //List<MemberInfo> currentMemberInfoGroup = null;

            for (int i = index, l = pathExpressions.Count; i < l; i++)
            {
                //if (lastType == null || !lastType.IsComplex())
                //{
                //    //as long as we still have complex type, keep the previous group
                //    //else create new group
                //    currentMemberInfoGroup.Clear();
                //}

                var expr = pathExpressions[i];

                switch (expr.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        {
                            var memberAccessExpr = expr as MemberExpression;
                            memberInfo = new EntityMemberInfo
                            (entityService, memberAccessExpr.Member,
                                lastType?.IsComplex() == true ? memberInfo : null, lastType ?? entityType);
                            propInfo = memberInfo.MemberInfo as PropertyInfo;
                            fieldInfo = memberInfo.MemberInfo as FieldInfo;

                            lastType = memberInfo.MemberFinalType ??
                                       lastType; /*propInfo?.PropertyType ?? fieldInfo?.FieldType ?? lastType*/
                            ;

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

                if (breakLoop || propInfo == null && fieldInfo == null || lastType == null) break;

                //currentMemberInfoGroup.Add(memberInfo);
                pathTraversed[memberInfo] = i; //(i, lastType);
                memberInfo.MemberFinalType = lastType;

                index = i; //update the current index always

                if (buildPath)
                {
                    //get the existing member instance
                    nextInstance = propInfo != null
                        ? propInfo.GetValue(currentInstance)
                        : fieldInfo.GetValue(currentInstance);

                    if (!lastType.GetTypeInfo().IsInterface && !lastType.GetTypeInfo().IsAbstract)
                    {
                        var nextType = nextInstance?.GetType();
                        if (nextInstance == null || nextType != lastType && !lastType.IsInstanceOfType(nextInstance))
                        {
                            if (lastType.IsArray)
                            {
                                var rank = lastType.GetArrayRank();
                                var lengths = new int[rank];
                                for (var j = 0; j < rank; j++) lengths[j] = 1;

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
                                propInfo.SetValue(currentInstance, nextInstance);
                            else if (fieldInfo != null) fieldInfo.SetValue(currentInstance, nextInstance);
                        }
                    }
                }
            }
        }

        //internal static string[] GetEntityPathNames
        //    (IEntityService entityService,
        //    ref object entity, ref Type entityType,
        //    List<Expression> expressions,
        //    ref int currentIndex, EntityResolver resolver, Func<object, string> serializer,
        //    out Dictionary<EntityMemberInfo, int> members, //Dictionary<List<MemberInfo>, (int Item1, Type Item2)> members,
        //    out Type lastType, bool useResolvedJsonName = true)
        //{
        //    string[] memberNames = new string[0];

        //    entityType = entity?.GetType() ?? entityType;
        //    var entityInfo = entityService.GetEntityTypeInfo(entityType);

        //    //do this to avoid create instances every time we call this method for a particular type
        //    bool buildPath = useResolvedJsonName && resolver == null && entityInfo.JsonNamePropertyMap.Count == 0;

        //    int index = currentIndex; //store this index incase of a repeat

        //    repeatBuild:
        //    if (buildPath && entity == null)
        //    {
        //        //most likely using the EntityConverter
        //        //create new instance
        //        entity = Utils.Utilities.CreateInstance(entityType);
        //    }

        //    currentIndex = index;

        //    TraverseEntityPath(entityService, entity, entityType, expressions, ref currentIndex, out lastType,
        //        out members, buildPath: buildPath);

        //    if (members.Count > 0)
        //    {
        //        if (buildPath)
        //        {
        //            //take care of the entity's complex properties and those of its children
        //            Utils.Utilities.InitializeComplexTypedProperties(entity, entityService);

        //            bool entityTypeAutoAdded = false;

        //            if (entityTypeAutoAdded = !entityService.EntityTypes.Contains(entityType))
        //            {
        //                entityService.AddEntityType(entityType); //just in case it wasn't already added.
        //            }

        //            //serialize the entity so the jsonnames would be set
        //            var serializedEntity = serializer(entity);

        //            if (entityTypeAutoAdded)
        //            {
        //                //remove it
        //                entityService.RemoveEntityType(entityType);
        //            }
        //        }

        //        if (resolver != null)
        //        {
        //            //force the propertymap to be set
        //            entityInfo.WithJsonResolver(resolver);
        //        }

        //        //first get the members that are not complex typed.
        //        //this is because their child members would be the actual member of interest
        //        //if none found, or name returns empty, try all members then.

        //        var membersToUse = members
        //            //.Where(m => ((m.Key.Last() as PropertyInfo)?.PropertyType ?? (m.Key.Last() as FieldInfo)?.FieldType)?.IsComplex() != true)
        //            .Where(m => m.Key.MemberFinalType.IsComplex() != true)
        //            .OrderBy(m => m.Value) //.Item1)
        //            .ToList();

        //        bool repeatedMemberNames = false;

        //        repeatMemberNames:
        //        bool gotoRepeatBuild = false;


        //        memberNames = membersToUse.Select((m, idx) =>
        //        {
        //            string complexJsonName = null;
        //            string nonJsonComplexName = m.Key.ComplexName; //.Select(_m => _m.Name).Aggregate((x, y) => $"{x}{Defaults.ComplexTypeNameSeparator}{y}");
        //            //bool isComplex = m.Key.IsComplex; //.Count > 1;

        //            if (useResolvedJsonName)
        //            {
        //                Type parentType = null;

        //                //try the entityInfo first
        //                var jsonPropMap = CypherUtilities.FilterPropertyMap(nonJsonComplexName, null, entityInfo.JsonNamePropertyMap);
        //                complexJsonName = CypherUtilities.GetMemberComplexJsonName(jsonPropMap, null, 
        //                    m.Key.ComplexRoot.MemberInfo, m.Key.ComplexRoot.MemberInfo.Name,
        //                    m.Key.MemberInfo, m.Key.MemberInfo.Name);
        //                //name = CypherUtilities.GetMemberJsonName(jsonPropMap, null, m.Key.First(), m.Key.First().Name, m.Key.Last(), m.Key.Last().Name);
        //                //name = entityInfo.JsonNamePropertyMap.FirstOrDefault(pm => pm.Value.IsEquivalentTo(m.Key)).Key?.Json;

        //                if (string.IsNullOrWhiteSpace(complexJsonName))
        //                {
        //                    //try to get name from entity info
        //                    if (idx >= 1)
        //                    {
        //                        parentType = membersToUse[idx - 1].Key.MemberFinalType; //.Value.Item2;
        //                    }

        //                    parentType = parentType ?? //m.Value?.Item2?.DeclaringType ?? 
        //                        m.Key.MemberFinalType.DeclaringType ??
        //                        m.Key.ComplexParent?.MemberFinalType; //Last().DeclaringType;

        //                    //we do this because MemberInfo.ReflectedType is not public yet in the .NET Core API.
        //                    var infos = entityService.GetDerivedEntityTypeInfos(parentType);

        //                    if (resolver != null)
        //                    {
        //                        foreach (var info in infos)
        //                        {
        //                            info.WithJsonResolver(resolver);
        //                        }
        //                    }

        //                    jsonPropMap = CypherUtilities.FilterPropertyMap(nonJsonComplexName, null, infos.SelectMany(info => info.JsonNamePropertyMap));
        //                    complexJsonName = CypherUtilities.GetMemberComplexJsonName(jsonPropMap, null,
        //                        m.Key.ComplexRoot.MemberInfo, m.Key.ComplexRoot.MemberInfo.Name,
        //                        m.Key.MemberInfo, m.Key.MemberInfo.Name);
        //                    //name = CypherUtilities.GetMemberJsonName(jsonPropMap, null, m.Key.First(), m.Key.First().Name, m.Key.Last(), m.Key.Last().Name);

        //                    //var result = infos.SelectMany(info => info.JsonNamePropertyMap)
        //                    //.ExactOrEquivalentMember((pair) => pair.Value, new KeyValuePair<MemberName, MemberInfo>(MemberName.Empty, m.Key))
        //                    //.FirstOrDefault();

        //                    //name = result.Key?.Json;
        //                }

        //                if (!buildPath
        //                && resolver == null //don't doubt the result if the EntityResolver was present
        //                && (string.IsNullOrWhiteSpace(complexJsonName)
        //                || (complexJsonName == nonJsonComplexName //m.Key.Name
        //                    && (parentType?.IsComplex() == true
        //                    || m.Key.DeclaringType.IsComplex() //Last().DeclaringType.IsComplex()
        //                    || m.Key.MemberFinalType.IsComplex() //Value.Item2.IsComplex()
        //                    )))
        //                )
        //                {
        //                    //maybe we were wrong and the jsonMaps are empty
        //                    //or just to be sure we have the actual name,
        //                    //repeat with a fully built path and serialization
        //                    buildPath = true;
        //                    gotoRepeatBuild = true;
        //                }
        //                else if (string.IsNullOrWhiteSpace(complexJsonName))
        //                {
        //                    //fallback for when name couldn't be resolved in all attempts
        //                    complexJsonName = nonJsonComplexName;
        //                }
        //            }
        //            else
        //            {
        //                complexJsonName = nonJsonComplexName;
        //            }

        //            return complexJsonName;
        //        }).Where(n => !string.IsNullOrWhiteSpace(n)).ToArray();

        //        if (gotoRepeatBuild)
        //        {
        //            goto repeatBuild;
        //        }

        //        if (!repeatedMemberNames && memberNames.Length == 0)
        //        {
        //            //repeat with all members
        //            membersToUse = members.OrderBy(m => m.Value).ToList(); //.Item1).ToList();
        //            repeatedMemberNames = true;
        //            goto repeatMemberNames;
        //        }
        //    }

        //    return memberNames;
        //}

        internal static string[] GetEntityPathNames
        (EntityService entityService,
            ref object entity, ref Type entityType,
            List<Expression> expressions,
            ref int currentIndex, EntityResolver resolver, Func<object, string> serializer,
            out Dictionary<EntityMemberInfo, int> members, //Dictionary<List<MemberInfo>, (int Item1, Type Item2)> members,
            out Type lastType, bool useResolvedJsonName = true)
        {
            var memberNames = new string[0];

            entityType = entity?.GetType() ?? entityType;
            var entityInfo = entityService.GetEntityTypeInfo(entityType);

            //do this to avoid create instances every time we call this method for a particular type
            var buildPath = useResolvedJsonName && resolver == null && entityInfo.JsonNamePropertyMap.Count == 0;

            var index = currentIndex; //store this index incase of a repeat

        repeatBuild:
            if (buildPath && entity == null)
                //most likely using the EntityConverter
                //create new instance
                entity = Utils.Utilities.CreateInstance(entityType);

            currentIndex = index;

            TraverseEntityPath(entityService, entity, entityType, expressions, ref currentIndex, out lastType,
                out members, buildPath);

            if (members.Count > 0)
            {
                if (buildPath)
                {
                    //take care of the entity's complex properties and those of its children
                    Utils.Utilities.InitializeComplexTypedProperties(entity, entityService);

                    var entityTypeAutoAdded = false;

                    if (entityTypeAutoAdded = !entityService.EntityTypes.Contains(entityType))
                        entityService.AddEntityType(entityType); //just in case it wasn't already added.

                    //serialize the entity so the jsonnames would be set
                    var serializedEntity = serializer(entity);

                    if (entityTypeAutoAdded)
                        //remove it
                        entityService.RemoveEntityType(entityType);
                }

                if (resolver != null)
                    //force the propertymap to be set
                    entityInfo.ResolveJsonPropertiesUsing(resolver);

                var membersToUse = members.OrderBy(m => m.Value).ToArray();
                var membersLength = membersToUse.Length;

                //for complex typed members, we are actually interested in their own members
                //hence, check their complex parents here and remove by filter any complex parent that already has a child in the list
                //if none found, or name returns empty, try all members then.

                membersToUse = membersToUse
                    .Where((m, i) =>
                        i + 1 < membersLength
                            ? membersToUse[i + 1].Key.ComplexParent != m.Key
                            : true) //!m.Key.MemberFinalType.IsComplex())
                    .ToArray();

                var repeatedMemberNames = false;

            repeatMemberNames:
                var gotoRepeatBuild = false;


                memberNames = membersToUse.Select((m, idx) =>
                {
                    string complexJsonName = null;
                    var nonJsonComplexName = m.Key.ComplexName;

                    if (useResolvedJsonName)
                    {
                        m.Key.ResolveNames(resolver, serializer);
                        //Type parentType = m.Key.ReflectedType;
                        complexJsonName = m.Key.ComplexJsonName;
                    }
                    else
                    {
                        complexJsonName = nonJsonComplexName;
                    }

                    return complexJsonName;
                }).Where(n => !string.IsNullOrWhiteSpace(n)).ToArray();

                if (gotoRepeatBuild) goto repeatBuild;

                if (!repeatedMemberNames && memberNames.Length == 0)
                {
                    //repeat with all members
                    membersToUse = members.OrderBy(m => m.Value).ToArray(); //.Item1).ToList();
                    repeatedMemberNames = true;
                    goto repeatMemberNames;
                }
            }

            return memberNames;
        }

        internal static List<Expression> ExplodeComplexTypeMemberAccess
        (EntityService entityService,
            Expression expression, out List<List<MemberInfo>> inversePaths)
        {
            return ExplodeComplexTypeAndMemberAccess(entityService, ref expression,
                expression.Type, out inversePaths);
        }

        internal static void ExplodeComplexType
            (EntityService entityService, Type type, out List<List<MemberInfo>> inversePaths)
        {
            Expression empty = null;
            ExplodeComplexTypeAndMemberAccess(entityService, ref empty, type, out inversePaths, false);
        }

        internal static List<Expression> ExplodeComplexTypeAndMemberAccess
        (EntityService entityService,
            ref Expression expression, Type type,
            out List<List<MemberInfo>> inversePaths, bool shouldTryCast = true)
        {
            inversePaths = new List<List<MemberInfo>>();

            var exprNotNull = expression != null;

            type = expression?.Type ?? type;

            if (shouldTryCast && exprNotNull)
                //this allows us use the runtime type of the executed expression
                try
                {
                    expression = expression.Cast(out var newType);
                    type = newType ?? type;
                }
                catch
                {
                    shouldTryCast = false;
                }

            if (type == null || !type.IsComplex())
                return new List<Expression>();

            var result = new List<Expression>();

            //add it to entity types
            entityService.AddEntityType(type);

            var info = entityService.GetEntityTypeInfo(type);

            foreach (var prop in info.AllProperties)
            {
                Expression memberExpr = exprNotNull ? Expression.MakeMemberAccess(expression, prop) : null;

                if (Utils.Utilities.IsScalarType(prop.PropertyType, entityService))
                {
                    result.Add(memberExpr);
                    inversePaths.Add(new List<MemberInfo> { prop });
                }
                else
                {
                    //recursively check till we hit the last scalar property
                    var memberRes = ExplodeComplexTypeAndMemberAccess(entityService,
                        ref memberExpr, prop.PropertyType, out var members, shouldTryCast && exprNotNull);
                    inversePaths.AddRange(members.Select(ml =>
                    {
                        ml.Add(prop);
                        return ml;
                    }));

                    result.AddRange(memberRes);
                }
            }

            return result;
        }

        public static string BuildSimpleVarsCall(List<Expression> expressions, QueryContext queryContext,
            bool? useResolvedJsonName = null)
        {
            //typeReturned = null;
            if (!Utils.Utilities.HasVars(expressions, out var methodExpr))
                return null;

            if (useResolvedJsonName == null)
                //check the last expression for nfp escape
                useResolvedJsonName = !Utils.Utilities.HasNfpEscape(expressions.Last().Uncast(out var castType));

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
                        out var members, out var lastType, useResolvedJsonName.Value);

                    if (memberNames.Length > 0)
                        memberJsonName = memberNames.Aggregate((first, second) => $"{first}.{second}").Trim('.');
                    //typeReturned = lastType ?? typeReturned;
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
                    builder.Append("." + memberJsonName);
                else
                    //restore last index less 1.
                    currentIndex = tmpIdx - 1;
            }

            return builder.ToString().Trim();
        }

        public static List<Expression> GetSimpleMemberAccessStretch
            (EntityService entityService, Expression expression, out Expression entityBestGuess)
        {
            return GetSimpleMemberAccessStretch(entityService, expression, out entityBestGuess, out var isContinuous);
        }

        public static List<Expression> GetSimpleMemberAccessStretch
        (EntityService entityService,
            Expression expression, out Expression entityBestGuess, out bool isContinuous)
        {
            isContinuous = true;

            var filtered = new List<Expression>();

            var currentExpression = expression;

            Expression localExpr = null,
                entityDisjointExpr = null,
                nfpExpr = null,
                methodExpr = null,
                parentEntityExpr = null,
                paramExpr = null,
                constExpr = null;

            while (currentExpression != null)
            {
                filtered.Add(currentExpression);

                var currentExprIsEntity = currentExpression.IsEntity(entityService);

                if (currentExprIsEntity)
                {
                    if (entityDisjointExpr != null)
                        //this would be a lie now, because we have a parent expression that is an entity
                        //so set to null.
                        entityDisjointExpr = null;

                    if (currentExpression != expression)
                        parentEntityExpr = currentExpression;
                }

                switch (currentExpression.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        {
                            var memberExpr = currentExpression as MemberExpression;

                            if (memberExpr.IsLocalMember())
                                localExpr = memberExpr;
                            else if (memberExpr.Expression == null
                                     || currentExprIsEntity
                                     && !memberExpr.Expression.Type.IsAnonymousType()
                                     && !memberExpr.Expression.IsEntity(entityService)
                            )
                                entityDisjointExpr = memberExpr;
                            else if (memberExpr.Expression.IsEntity(entityService))
                                parentEntityExpr = memberExpr.Expression;

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
                            var methodCallExpr = currentExpression as MethodCallExpression;
                            currentExpression = methodCallExpr.Object;

                            if (currentExpression == null //maybe extension method
                                && (methodCallExpr.Method.IsExtensionMethod()
                                    || methodCallExpr.Method.DeclaringType ==
                                    Defaults.CypherFuncsType //checking for our functions
                                    && (methodCallExpr.Method.Name.StartsWith("_") //our dummy method
                                        || Defaults.CypherExtensionFuncsType.GetMethods()
                                            .Any(p => p.Name == methodCallExpr.Method.Name))
                                )
                            )
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
            entityBestGuess = nfpExpr ?? constExpr ?? paramExpr ?? entityDisjointExpr ??
                localExpr ?? methodExpr ?? parentEntityExpr ?? filtered.FirstOrDefault();

            return filtered;
        }

        internal static int GetFirstMemberAccessIndex(List<Expression> exprs, Expression entityVal,
            out Expression firstAccessExpr)
        {
            firstAccessExpr = exprs.SkipWhile(a => a == entityVal || a.NodeType != ExpressionType.MemberAccess)
                .FirstOrDefault();
            return exprs.IndexOf(firstAccessExpr);
        }

        internal static bool IsSpecialNode(FunctionExpressionVisitor funcsVisitor, Expression node,
            out object value, out bool hasVars, out bool hasFunctions, out bool hasDummyMethod,
            bool isFuncsExprVisitorCalling = false)
        {
            value = null;
            hasVars = false;
            hasFunctions = false;
            hasDummyMethod = false;

            if (node == null)
                return false;

            var isSpecialNode = false;

            //first see if we can successfully execute the expression
            try
            {
                value = node.ExecuteExpression<object>();
            }
            catch (Exception e)
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

            if (!isSpecialNode && value?.GetType() == Defaults.JRawType)
            {
                //never directly delegate jraw to the serializer. use the functions visitor for it instead
                isSpecialNode = true;
                hasFunctions = true;
            }

            if (!isSpecialNode && value == null)
            {
                //try a full search then
                var filtered = GetSimpleMemberAccessStretch(funcsVisitor.EntityService, node, out var filteredVal,
                    out var isContinuous);
                var referenceNode = filtered.LastOrDefault();

                //if (Utils.Utilities.HasVars(filtered))
                //{
                //    //found a special node.
                //    isSpecialNode = true;
                //}
                //else 
                if (referenceNode?.Type == Defaults.JRawType
                ) //never directly delegate jraw to the serializer. use the functions visitor for it instead
                {
                    hasFunctions = true;
                    isSpecialNode = true;
                }
                else if (!isFuncsExprVisitorCalling && isContinuous &&
                         filtered.Count > 0 //maybe one of our functions //check if it can be handled
                         && funcsVisitor.CanHandle(referenceNode, out var handler)
                ) // && referenceNode?.NodeType == ExpressionType.Call)
                {
                    //found a special node
                    hasFunctions = true;
                    isSpecialNode = true;
                }
            }

            return isSpecialNode;
        }
    }
}