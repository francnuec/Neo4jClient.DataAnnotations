using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using Neo4jClient.DataAnnotations.Cypher;
using System.Text;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Expressions;
using Newtonsoft.Json.Linq;
using System.Collections;
using Neo4jClient.Cypher;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using Neo4jClient.DataAnnotations.Cypher.Extensions;

namespace Neo4jClient.DataAnnotations
{
    public class Utilities
    {
        /// <summary>
        /// Gets all the <see cref="TableAttribute"/> names on the class inheritance as labels.
        /// Should none be gotten, the type name is used instead by default.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> GetLabels(Type type, bool useTypeNameIfEmpty = true)
        {
            var labels = new List<string>();
            var typeInfo = type.GetTypeInfo();

            while (typeInfo != null)
            {
                labels.Add(GetLabel(typeInfo.AsType(), useTypeNameIfEmpty: false));
                typeInfo = typeInfo.BaseType?.GetTypeInfo();
            }

            labels = labels.Distinct().Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

            if (labels.Count > 0 || !useTypeNameIfEmpty)
            {
                return labels;
            }

            return new List<string>() { type.Name };
        }

        /// <summary>
        /// Gets the <see cref="TableAttribute"/> name on the specified class as a label.
        /// Should none be gotten, the type name is used instead by default.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetLabel(Type type, bool useTypeNameIfEmpty = true)
        {
            var typeInfo = type.GetTypeInfo();
            var attribute = typeInfo.GetCustomAttributes<TableAttribute>()?.FirstOrDefault();

            if (attribute != null)
            {
                var ret = attribute.Name;

                if (ret != null)
                    return ret;
            }

            return useTypeNameIfEmpty ? type.Name : "";
        }

        public static PropertyInfo GetPropertyInfoFrom<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            return GetPropertyInfoFrom(propertyLambda.Body, typeof(TSource), typeof(TProperty));
        }

        public static PropertyInfo GetPropertyInfoFrom(Expression expr, Type sourceType, Type propertyType,
             bool includeIEnumerableArg = true, bool acceptObjectTypeCast = false)
        {
            MemberExpression memberExpr = (!acceptObjectTypeCast ? expr :
                expr?.Uncast(out var cast, castToRemove: Defaults.ObjectType)) as MemberExpression;

            if (memberExpr == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    expr.ToString()));

            PropertyInfo propInfo = memberExpr.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    expr.ToString()));

            if (sourceType != null)
            {
                if (sourceType != propInfo.DeclaringType &&
                !propInfo.DeclaringType.IsAssignableFrom(sourceType))
                    throw new ArgumentException(string.Format(
                        "Expresion '{0}' refers to a property that is not from type '{1}'.",
                        expr.ToString(),
                        sourceType));
            }

            if (propertyType != null)
            {
                var isPropType = propInfo.PropertyType.IsMatch(propertyType, includeIEnumerableArg: includeIEnumerableArg);

                if (!isPropType)
                {
                    throw new ArgumentException($"Expected property of type '{propertyType}'.");
                }
            }

            return propInfo;
        }

        public static Type GetEnumerableGenericType(Type type)
        {
            var iEnumerableType = typeof(IEnumerable<>);

            var interfaces = type.GetInterfaces()?.ToList();

            if (interfaces == null)
            {
                if (!type.GetTypeInfo().IsInterface)
                    return null;

                interfaces = new List<Type>();
            }

            if (type.GetTypeInfo().IsInterface)
            {
                interfaces.Insert(0, type);
            }

            var interfaceType = interfaces?.Where(i => i.GetTypeInfo().IsGenericType
            && i.GetGenericTypeDefinition() == iEnumerableType).FirstOrDefault();

            return interfaceType?.GetGenericArguments().SingleOrDefault();
        }

        public static Type GetNullableUnderlyingType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ??
                (type.GetTypeInfo().IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
                type.GetGenericArguments().FirstOrDefault() : null);
        }

        public static bool IsScalarType(Type type)
        {
            if (Neo4jAnnotations.processedScalarTypes.TryGetValue(type, out var isScalar))
                return isScalar;

            Func<Type, bool> isNav = (_type) =>
            {
                var _typeInfo = _type?.GetTypeInfo();
                var ret = (_typeInfo.IsClass
                || _typeInfo.IsInterface
                || _typeInfo.IsDefined(Defaults.NeoNonScalarType)
                || Neo4jAnnotations.KnownNonScalarTypes.Contains(_type))
                && !Neo4jAnnotations.KnownScalarTypes.Contains(_type)
                && !_typeInfo.IsDefined(Defaults.NeoScalarType);

                return ret;
            };

            if (type != null && isNav(type))
            {
                //check for an array/iEnumerable/nullable before concluding
                Type genericArgument = GetEnumerableGenericType(type) ?? GetNullableUnderlyingType(type);

                if (genericArgument == null || isNav(genericArgument))
                {
                    try
                    {
                        Neo4jAnnotations.processedScalarTypes[type] = false;
                    }
                    catch
                    {

                    }

                    return false;
                }
            }

            if (type != null)
            {
                try
                {
                    Neo4jAnnotations.processedScalarTypes[type] = true;
                }
                catch
                {

                }

                return true;
            }

            return false;
        }

        public static MethodInfo GetMethodInfo(Expression<Action> expression)
        {
            var member = expression.Body as MethodCallExpression;
            
            if (member != null)
            {
                var methodInfo = member.Method;
                return methodInfo;
            }

            throw new ArgumentException("Expression is not a method", "expression");
        }

        public static MethodInfo GetGenericMethodInfo(MethodInfo genericMethodInfo, params Type[] typeArguments)
        {
            if (genericMethodInfo != null && genericMethodInfo.IsGenericMethod)
            {
                if (typeArguments?.Length > 0)
                    genericMethodInfo = genericMethodInfo.GetGenericMethodDefinition().MakeGenericMethod(typeArguments);

                return genericMethodInfo;
            }

            throw new ArgumentException("Expression is not a generic method", "expression");
        }

        public static PropertyInfo GetPropertyInfo(Expression<Func<object>> expression)
        {
            var memberExpr = expression.Body.Uncast(out var castRemoved) as MemberExpression;
            var member = memberExpr?.Member as PropertyInfo;

            return member ?? throw new ArgumentException("Expression is not a property", "expression");
        }

        public static FieldInfo GetFieldInfo(Expression<Func<object>> expression)
        {
            var memberExpr = expression.Body.Uncast(out var castRemoved) as MemberExpression;
            var member = memberExpr?.Member as FieldInfo;

            return member ?? throw new ArgumentException("Expression is not a field", "expression");
        }

        internal static void InitializeComplexTypedProperties(object entity)
        {
            var entityInfo = Neo4jAnnotations.GetEntityTypeInfo(entity.GetType());

            var complexProps = entityInfo.ComplexTypedProperties;

            object instance;

            foreach (var complexProp in complexProps)
            {
                try
                {
                    instance = complexProp.GetValue(entity);
                }
                catch
                {
                    instance = null;
                }

                if (instance == null && complexProp.CanWrite)
                {
                    instance = Utilities.CreateInstance(complexProp.PropertyType);
                    complexProp.SetValue(entity, instance);
                }

                //recursively do this for its own properties
                if (instance != null)
                    InitializeComplexTypedProperties(instance);
            }
        }

        internal static void TraverseEntityPath
            (object entity, List<Expression> pathExpressions,
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
                            else if (IsScalarType(lastType))
                            {
                                nextInstance = lastType.GetDefaultValue();
                            }
                            else
                            {
                                nextInstance = Utilities.CreateInstance(lastType);
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

        internal static string[] GetEntityPathNames(ref object entity, ref Type entityType,
            List<Expression> expressions,
            ref int currentIndex, EntityResolver resolver, Func<object, string> serializer,
            out Dictionary<MemberInfo, Tuple<int, Type>> members, out Type lastType, bool useResolvedJsonName = true)
        {
            string[] memberNames = new string[0];

            entityType = entity?.GetType() ?? entityType;
            var entityInfo = Neo4jAnnotations.GetEntityTypeInfo(entityType);

            //do this to avoid create instances every time we call this method for a particular type
            bool buildPath = useResolvedJsonName && resolver == null && entityInfo.JsonNamePropertyMap.Count == 0;

            int index = currentIndex; //store this index incase of a repeat

            repeatBuild:
            if (buildPath && entity == null)
            {
                //most likely using the EntityConverter
                //create new instance
                entity = Utilities.CreateInstance(entityType);
            }

            currentIndex = index;

            TraverseEntityPath(entity, expressions, ref currentIndex, out lastType,
                out members, buildPath: buildPath);

            if (members.Count > 0)
            {
                if (buildPath)
                {
                    //take care of the entity's complex properties and those of its children
                    InitializeComplexTypedProperties(entity);

                    bool entityTypeAutoAdded = false;

                    if (entityTypeAutoAdded = !Neo4jAnnotations.EntityTypes.Contains(entityType))
                    {
                        Neo4jAnnotations.AddEntityType(entityType); //just in case it wasn't already added.
                    }

                    //serialize the entity so the jsonnames would be set
                    var serializedEntity = serializer(entity);

                    if (entityTypeAutoAdded)
                    {
                        //remove it
                        Neo4jAnnotations.RemoveEntityType(entityType);
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

                var membersToUse = members.Where(m =>
                    !((m.Key as PropertyInfo)?.PropertyType ?? 
                    (m.Key as FieldInfo)?.FieldType)?.IsComplex() == true)
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
                        name = entityInfo.JsonNamePropertyMap.FirstOrDefault(pm => pm.Value.IsEquivalentTo(m.Key)).Key;

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
                            var infos = Neo4jAnnotations.GetDerivedEntityTypeInfos(parentType);

                            if (resolver != null)
                            {
                                foreach (var info in infos)
                                {
                                    info.WithJsonResolver(resolver);
                                }
                            }

                            var result = infos.SelectMany(info => info.JsonNamePropertyMap)
                            .ExactOrEquivalentMember((pair) => pair.Value, new KeyValuePair<string, MemberInfo>("", m.Key))
                            .FirstOrDefault();

                            name = result.Key;
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

        internal static List<Expression> ExplodeComplexTypeMemberAccess(Expression expression, out List<List<MemberInfo>> inversePaths)
        {
            return ExplodeComplexTypeAndMemberAccess(ref expression, expression.Type, out inversePaths, shouldTryCast: true);
        }

        internal static void ExplodeComplexType(Type type, out List<List<MemberInfo>> inversePaths)
        {
            Expression empty = null;
            ExplodeComplexTypeAndMemberAccess(ref empty, type, out inversePaths, shouldTryCast: false);
        }

        internal static List<Expression> ExplodeComplexTypeAndMemberAccess(ref Expression expression, Type type,
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

            var info = Neo4jAnnotations.GetEntityTypeInfo(type);

            foreach (var prop in info.AllProperties)
            {
                Expression memberExpr = exprNotNull ? Expression.MakeMemberAccess(expression, prop) : null;

                if (IsScalarType(prop.PropertyType))
                {
                    result.Add(memberExpr);
                    inversePaths.Add(new List<MemberInfo>() { prop });
                }
                else
                {
                    //recursively check till we hit the last scalar property
                    var memberRes = ExplodeComplexTypeAndMemberAccess(ref memberExpr, prop.PropertyType, out var members, shouldTryCast: shouldTryCast && exprNotNull);
                    inversePaths.AddRange(members.Select(ml => { ml.Add(prop); return ml; }));

                    result.AddRange(memberRes);
                }
            }

            return result;
        }

        /// <summary>
        /// No further processing escape
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public static bool HasNfpEscape(Expression expression)
        {
            expression = expression?.Uncast(out var cast, Defaults.ObjectType); //in case this is coming from a dictionary
            MethodCallExpression methodExpr = null;
            return expression != null && (methodExpr = expression as MethodCallExpression) != null
                && methodExpr.Method.IsEquivalentTo("_", Defaults.ObjectExtensionsType);
        }

        public static bool HasVars(List<Expression> expressions)
        {
            return HasVars(expressions, out var methodExpr);
        }

        public static bool HasVars(List<Expression> expressions, out MethodCallExpression methodExpr)
        {
            methodExpr = null;
            return expressions != null && expressions.Count > 0
                && (methodExpr = expressions[0] as MethodCallExpression) != null
                && methodExpr.Method.IsEquivalentTo("Get", Defaults.VarsType);
        }

        public static string BuildSimpleVars(List<Expression> expressions, QueryUtilities queryUtilities, bool? useResolvedJsonName = null)
        {
            //typeReturned = null;
            if (!HasVars(expressions, out var methodExpr))
                return null;

            if (useResolvedJsonName == null)
            {
                //check the last expression for nfp escape
                useResolvedJsonName = !HasNfpEscape(expressions.Last().Uncast(out var castType));
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
                var visitor = new FunctionExpressionVisitor(queryUtilities);
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

                    var memberNames = GetEntityPathNames(ref entity, ref entityType, expressions, ref currentIndex, 
                        queryUtilities.Resolver, queryUtilities.SerializeCallback,
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

        public static List<Expression> GetSimpleMemberAccessStretch(Expression expression, out Expression entityBestGuess)
        {
            return GetSimpleMemberAccessStretch(expression, out entityBestGuess, out var isContinuous);
        }

        public static List<Expression> GetSimpleMemberAccessStretch
            (Expression expression, out Expression entityBestGuess, out bool isContinuous)
        {
            isContinuous = true;

            var filtered = new List<Expression>();

            var currentExpression = expression;

            Expression localExpr = null, entityDisjointExpr = null, nfpExpr = null,
                methodExpr = null, parentEntityExpr = null, paramExpr = null, constExpr = null;

            while (currentExpression != null)
            {
                filtered.Add(currentExpression);

                bool currentExprIsEntity = currentExpression.IsEntity();

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
                                && !memberExpr.Expression.IsEntity())
                                )
                            {
                                entityDisjointExpr = memberExpr;
                            }
                            else if (memberExpr.Expression.IsEntity())
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
                                if (HasNfpEscape(methodCallExpr))
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

        /// <summary>
        /// Placeholder method for <see cref="Vars"/> class calls in expressions.
        /// </summary>
        /// <typeparam name="TReturn">The last return type of the contiguous access stretch.</typeparam>
        /// <param name="index">The index of the actual expression in the store.</param>
        /// <returns>A default value of the return type.</returns>
        internal static TReturn GetValue<TReturn>(int index)
        {
            return (TReturn)typeof(TReturn).GetDefaultValue();
        }

        public static bool HasSet(Expression expression)
        {
            return HasSet(expression, out var methodExpr);
        }

        public static bool HasSet(Expression expression, out MethodCallExpression methodExpr)
        {
            methodExpr = null;
            return (methodExpr = expression as MethodCallExpression) != null
                && methodExpr.Method.IsEquivalentTo("_Set", Defaults.ObjectExtensionsType);
        }

        public static void CheckIfComplexTypeInstanceIsNull(object instance, string propertyName, Type declaringType)
        {
            if (instance == null)
            {
                //Complex types cannot be null. A value must be provided always.
                throw new InvalidOperationException(string.Format(Messages.NullComplexTypePropertyError, propertyName, declaringType.Name));
            }
        }

        public static object GetComplexTypeInstance(
            string name, ref Type type, Type declaringType,
            object existingInstance, out bool isNew,
            bool hasComplexChild = false, string childName = null,
            Type childType = null, Type childDeclaringType = null)
        {
            type = childDeclaringType ?? type;

            isNew = false;

            var instance = existingInstance;

            if (instance == null)
            {
                instance = Utilities.CreateInstance(type);
                isNew = true;
            }

            if (!isNew && hasComplexChild)
            {
                var members = instance.GetType().GetMembers(Defaults.MemberSearchBindingFlags).Where(m => m is FieldInfo || m is PropertyInfo);

                //check if the instance has the child as a member
                if (members?.Where(m => m.IsEquivalentTo(childName, childDeclaringType,
                    childType)).FirstOrDefault() == null)
                {
                    //it doesn't, so create new instance from child declaring type
                    existingInstance = instance;
                    instance = Utilities.CreateInstance(type);
                    isNew = true;

                    //now copy the values from old instance unto this new one
                    foreach (var member in members)
                    {
                        var field = member as FieldInfo;
                        var property = member as PropertyInfo;

                        try
                        {
                            if (property?.CanWrite == true)
                                property.SetValue(instance, property.GetValue(existingInstance));
                            else
                                field?.SetValue(instance, field.GetValue(existingInstance));
                        }
                        catch
                        {

                        }
                    }
                }
            }

            return instance;
        }

        public static JObject GetFinalProperties(
            LambdaExpression lambdaExpr, QueryUtilities queryUtilities,
            out bool hasFunctionsInProperties)
        {
            hasFunctionsInProperties = false;

            //get the properties expression
            if (lambdaExpr != null && queryUtilities.SerializeCallback != null)
            {
                var resolver = queryUtilities.Resolver;
                var serializer = queryUtilities.SerializeCallback;

                //visit the expressions
                var entityVisitor = new EntityExpressionVisitor(queryUtilities);

                var instanceExpr = entityVisitor.Visit(lambdaExpr.Body);
                var predicateExpr = entityVisitor.SetPredicateNode;
                var predicateMemberAssignments = entityVisitor.SetPredicateMemberAssignments;
                var predicateDictionaryAssignments = entityVisitor.SetPredicateDictionaryAssignments;
                var usePredicateOnly = entityVisitor.SetUsePredicateOnly;

                //get the instance
                var instance = instanceExpr.ExecuteExpression<object>();

                var instanceType = instance.GetType();
                var instanceIsDictionary = instanceType.IsDictionaryType();
                var sourceIsAnonymous = entityVisitor.Source.Type.IsAnonymousType();
                var rootIsDictionary = entityVisitor.RootNode.Type.IsDictionaryType();

                var instanceInfo = Neo4jAnnotations.GetEntityTypeInfo(instanceType);

                var dictMemberNames = entityVisitor.DictMemberNames;
                Dictionary<string, MemberInfo> jsonNamePropertyMap = null;
                Dictionary<object, Expression> predicateAssignments = null;
                List<Tuple<object, Expression, Type, List<JProperty>>> predicateComplexAssignments = null;
                string instanceJson = null;

                if (!instanceIsDictionary)
                {
                    if (!instanceType.IsAnonymousType())
                        Neo4jAnnotations.AddEntityType(instanceType); //just in case it was omitted

                    if (resolver != null)
                    {
                        instanceInfo.WithJsonResolver(resolver);
                    }
                    else
                    {
                        try
                        {
                            if (usePredicateOnly)
                            {
                                //initialize complex properties in case they were omitted
                                //however, avoid initilizing on behalf of the user unless it's a predicate
                                InitializeComplexTypedProperties(instance);
                            }

                            //serialize the instance to force converter to enumerate jsonNames
                            instanceJson = serializer(instance);
                        }
                        catch
                        {

                        }
                    }

                    jsonNamePropertyMap = instanceInfo.JsonNamePropertyMap;
                }
                else if (sourceIsAnonymous)
                {
                    //get the mapping from dictionary member names
                    jsonNamePropertyMap = entityVisitor.DictMemberNames.SelectMany(item => item.Value)
                        .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2.FirstOrDefault());
                }

                object predicateInstance = null;
                JObject predicateJObject = null;

                //check if it has a "set" node
                if (predicateExpr != null)
                {
                    if (!usePredicateOnly)
                    {
                        //has a separate predicate instance
                        predicateInstance = predicateExpr.ExecuteExpression<object>();
                    }
                    else
                    {
                        predicateInstance = instance;
                    }

                    if (!instanceIsDictionary)
                    {
                        //initialize complex properties in case they were omitted
                        InitializeComplexTypedProperties(predicateInstance);
                    }

                    //serialize the predicate
                    var predicateJson = instanceJson != null && predicateInstance == instance ? 
                        instanceJson : serializer(predicateInstance);

                    predicateJObject = JObject.Parse(predicateJson);

                    //filter out the predicate assignments
                    var filteredProps = new List<JProperty>();

                    predicateAssignments = new Dictionary<object, Expression>();

                    if (predicateMemberAssignments != null && predicateMemberAssignments.Count > 0)
                    {
                        var assignments = predicateMemberAssignments.ToDictionary(item => (object)item.Key, item => item.Value);

                        foreach (var item in assignments)
                            predicateAssignments.Add(item.Key, item.Value);
                    }

                    if (predicateDictionaryAssignments != null && predicateDictionaryAssignments.Count > 0)
                    {
                        var assignments = predicateDictionaryAssignments.ToDictionary(item => (object)item.Key, item => item.Value);

                        foreach (var item in assignments)
                            predicateAssignments.Add(item.Key, item.Value);
                    }

                    if (predicateAssignments.Count > 0)
                    {
                        //use member assignments
                        //for each member assignment, find the corresponding jsonname, and jsonproperty
                        filteredProps.AddRange(ResolveAssignments
                            (jsonNamePropertyMap, dictMemberNames, 
                            predicateAssignments, predicateJObject, instanceType.Name,
                            out predicateComplexAssignments));
                    }

                    if (filteredProps.Count > 0)
                    {
                        //create new JObject
                        predicateJObject = new JObject();

                        foreach (var prop in filteredProps)
                        {
                            predicateJObject.Add(prop.Name, prop.Value);
                        }
                    }
                }

                //now resolve instance
                JObject instanceJObject = predicateJObject; //we just assume this first

                if (predicateInstance != instance)
                {
                    instanceJson = instanceJson ?? serializer(instance);
                    instanceJObject = JObject.Parse(instanceJson);

                    if (predicateJObject != null)
                    {
                        if (predicateComplexAssignments != null && predicateComplexAssignments.Count > 0)
                        {
                            //these are complex properties of the instanceType that were directly assigned in the predicate and not in the original instance
                            //so remove those expanded properties found on instance but not on predicate
                            //this would usually happen when the complex type assigned on instance is a derived type of the complex type assigned on predicate
                            foreach (var complexAssignment in predicateComplexAssignments)
                            {
                                //get the baseMemberJsonName
                                var itemName = complexAssignment.Item4.First().Name;
                                var sepIdx = itemName.IndexOf(Defaults.ComplexTypeNameSeparator);
                                var baseMemberJsonName = sepIdx > 0 ? itemName.Substring(0, sepIdx) : new string(itemName.ToCharArray());

                                //find all jproperties on instanceJObject starting with this name
                                var complexJProps = instanceJObject.Properties().Where(jp => jp.Name.StartsWith(baseMemberJsonName)).ToArray();

                                if (complexJProps.Length > 0)
                                {
                                    foreach (var complexJProp in complexJProps)
                                    {
                                        if (!complexAssignment.Item4.Any(jp => jp.Name == complexJProp.Name))
                                        {
                                            //candidate for removal, but confirm it isn't an actual property complexly named first
                                            bool dontRemove = jsonNamePropertyMap != null
                                                && jsonNamePropertyMap.TryGetValue(complexJProp.Name, out var complexPropInfo)
                                                && instanceInfo.AllProperties.Contains(complexPropInfo);

                                            if (!dontRemove && rootIsDictionary && dictMemberNames != null)
                                            {
                                                //check dictionary names
                                                try
                                                {
                                                    if (dictMemberNames.TryGetValue(complexJProp.Name, out var values))
                                                    {
                                                        //key was deliberately set by user in instance dictionary so keep the property
                                                        dontRemove = true;
                                                        break;
                                                    }
                                                }
                                                catch
                                                {

                                                }
                                            }

                                            if (!dontRemove)
                                            {
                                                //remove it
                                                complexJProp.Remove();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                        //apply the predicate values to instance
                        foreach (var predicateProp in predicateJObject)
                        {
                            instanceJObject[predicateProp.Key] = predicateProp.Value; //should generate exception if the key is not found
                        }
                    }
                }

                //now replace values with neo functions where appropriate
                var functionNodes = entityVisitor.SpecialNodePaths.Where(pair => pair.Item2.Type == SpecialNodeType.Function).ToArray();

                if (functionNodes.Length > 0)
                {
                    //for vars:
                    //a member is identified by the last MemberAssignment, or the first argument of an ElementInit of the first Dictionary<string, object>
                    //title: a.title (assigned to a member)
                    //roles: [a.roles[0]] (ElementInit of an assignment to a member)
                    //roles: [a.roles[b.index]] (This scenario is same as previous, except with recursive vars)
                    //in other words, direct assignment, and arrays are supported

                    var propertyKeyToVarNodes = new List<Tuple<string, IEnumerable<object>, SpecialNode, object>>();

                    foreach (var varNode in functionNodes)
                    {
                        object varBuiltValue = varNode.Item2.ConcreteValue; //as string;
                        object referenceItem = null;
                        string propertyKey = null;

                        var paths = varNode.Item1;

                        MemberAssignment assignment = null;
                        MemberListBinding listBinding = null;
                        ElementInit dictElementInit = null;
                        ListInitExpression dictListInit = null;


                        //from the top of the list, the first memberassignment or memberlistbinding is our guy
                        foreach (var item in paths)
                        {
                            if ((assignment = item as MemberAssignment) != null
                                || (listBinding = item as MemberListBinding) != null)
                            {
                                break;
                            }

                            if (dictElementInit == null)
                                dictElementInit = item as ElementInit;

                            if (dictElementInit != null
                                && (dictListInit = item as ListInitExpression) != null
                                && dictListInit.NewExpression.Type != EntityExpressionVisitor.DictType)
                            {
                                break;
                            }
                        }

                        var memberBinding = assignment ?? (MemberBinding)listBinding;

                        referenceItem = memberBinding ?? (object)dictListInit;

                        if (referenceItem == null)
                        {
                            throw new InvalidOperationException(string.Format(Messages.AmbiguousVarsPathError, varBuiltValue));
                        }

                        if (memberBinding != null)
                        {
                            //find property key
                            JProperty jProperty = null;
                            try
                            {
                                jProperty = ResolveAssignments(jsonNamePropertyMap, dictMemberNames, new Dictionary<object, Expression>()
                                {
                                    { memberBinding.Member, assignment?.Expression }
                                }, instanceJObject, instanceType.Name, out var complexAssignments).FirstOrDefault();
                            }
                            catch (Exception e)
                            {
                                throw new InvalidOperationException(string.Format(Messages.AmbiguousVarsPathError, varBuiltValue), e);
                            }

                            if (jProperty == null)
                            {
                                throw new InvalidOperationException(string.Format(Messages.AmbiguousVarsPathError, varBuiltValue));
                            }

                            propertyKey = jProperty.Name;
                            referenceItem = memberBinding;
                        }
                        else if (dictElementInit != null && dictListInit.Initializers.Contains(dictElementInit))
                        {
                            //for dictionaries
                            propertyKey = dictElementInit.Arguments[0].ExecuteExpression<string>();
                            referenceItem = dictListInit;
                        }

                        if (propertyKey == null
                            || (varNode.Item2.FoundWhileVisitingPredicate && predicateJObject[propertyKey] == null) //avoid invalid assignments
                            )
                        {
                            //trouble
                            throw new InvalidOperationException(string.Format(Messages.AmbiguousVarsPathError, varBuiltValue));
                        }

                        propertyKeyToVarNodes.Add(new Tuple<string, IEnumerable<object>, SpecialNode, object>
                                (propertyKey,
                                paths.Take(paths.IndexOf(referenceItem) + 1),
                                varNode.Item2,
                                varBuiltValue));
                    }

                    foreach (var item in propertyKeyToVarNodes)
                    {
                        //find the value and replace with function where appropriate
                        var key = item.Item1;
                        var pathsLeft = item.Item2.ToArray();
                        var specialNode = item.Item3;
                        var varBuiltValue = item.Item4;

                        var getParamsExpr = pathsLeft[0] as MethodCallExpression;

                        var instanceJValue = instanceJObject[key];

                        var finalValue = new JRaw(varBuiltValue);

                        //value should be one of two things
                        //array or normal literal

                        //test for JArray first
                        if (instanceJValue.Type == JTokenType.Array)
                        {
                            var jArray = instanceJValue as JArray;

                            //find the index of this array to set
                            var nextObj = pathsLeft[1];
                            int index = -1;

                            NewArrayExpression arrayExpr = nextObj as NewArrayExpression;

                            if (arrayExpr != null)
                            {
                                //found it
                                index = arrayExpr.Expressions.IndexOf(getParamsExpr);
                            }
                            else
                            {
                                //if the above failed, we are dealing with a list init or list binding
                                var elementInit = nextObj as ElementInit;
                                var initializers = (pathsLeft[2] as ListInitExpression)?.Initializers ?? (pathsLeft[2] as MemberListBinding)?.Initializers;

                                if (elementInit != null && initializers != null)
                                {
                                    index = initializers.IndexOf(elementInit);
                                }
                            }

                            if (index < 0 || index >= jArray.Count)
                            {
                                //yawa don gas :)
                                throw new InvalidOperationException(string.Format(Messages.AmbiguousVarsPathError, varBuiltValue));
                            }

                            //replace the value
                            jArray[index] = finalValue;
                            hasFunctionsInProperties = true;
                            continue;
                        }

                        //assign
                        instanceJObject[key] = finalValue;
                        hasFunctionsInProperties = true;
                    }
                }

                return instanceJObject;
            }

            return null;
        }

        private static JProperty ResolveJPropertyFromAssignment
            (Dictionary<string, MemberInfo> jsonNamePropertyMap,
            Dictionary<string, List<Tuple<string, List<MemberInfo>>>> dictMemberNames,
            JObject jObject, MemberInfo assignmentInfo, string assignmentName,
            MemberInfo actual, string actualName, string instanceTypeName)
        {
            string memberJsonName = null;

            if (jsonNamePropertyMap != null)
            {
                var memberJsonNameMap = jsonNamePropertyMap
                .Where(item => item.Value.IsEquivalentTo(actual))
                .FirstOrDefault();

                if (memberJsonNameMap.Value != null)
                    memberJsonName = memberJsonNameMap.Key;
            }

            if (dictMemberNames != null && string.IsNullOrWhiteSpace(memberJsonName))
            {
                //try dictmembernames
                if (actual != null)
                {
                    var tuple = dictMemberNames.SelectMany(item => item.Value)
                        .FirstOrDefault(item => item.Item2?.FirstOrDefault()?.IsEquivalentTo(actual) == true);

                    if (tuple != null)
                    {
                        memberJsonName = tuple.Item1;
                    }
                }

                if (string.IsNullOrWhiteSpace(memberJsonName))
                {
                    //still empty
                    //use the name to search dict keys
                    actualName = actualName ?? actual?.Name;

                    if (dictMemberNames.TryGetValue(actualName, out var values) 
                        || ((assignmentName = assignmentName ?? assignmentInfo?.Name) != null 
                        &&  dictMemberNames.TryGetValue(assignmentName, out values)))
                    {
                        memberJsonName = values?.FirstOrDefault(v => v.Item1 == actualName)?.Item1 ?? (values?.Count == 1 ? values[0].Item1 : null);
                    }
                }
            }

            if (memberJsonName == null)
            {
                //we have a problem
                throw new Exception(string.Format(Messages.InvalidMemberAssignmentError, assignmentInfo?.Name ?? assignmentName));
            }

            //get the jproperty
            var jProp = jObject.Properties().FirstOrDefault(jp => jp.Name == memberJsonName);

            if (jProp == null)
            {
                //another problem
                throw new Exception(string.Format(Messages.JsonPropertyNotFoundError, memberJsonName, instanceTypeName));
            }

            return jProp;
        }

        private static List<JProperty> ResolveAssignments (Dictionary<string, MemberInfo> jsonNamePropertyMap,
            Dictionary<string, List<Tuple<string, List<MemberInfo>>>> dictMemberNames,
            Dictionary<object, Expression> assignments, JObject jObject, string instanceTypeName,
            out List<Tuple<object, Expression, Type, List<JProperty>>> complexAssignments)
        {
            var filteredProps = new List<JProperty>();
            complexAssignments = null;

            foreach (var assignment in assignments)
            {
                var assignmentKey = assignment.Key;
                var assignmentKeyInfo = assignmentKey as MemberInfo;
                var assignmentKeyName = assignmentKey as string ?? assignmentKeyInfo?.Name;

                var assignmentValue = assignment.Value;
                var type = (assignmentKey as PropertyInfo)?.PropertyType 
                    ?? (assignmentKey as FieldInfo)?.FieldType
                    ?? assignmentValue?.Type;

                if (type.IsComplex() && (assignmentValue == null || !HasNfpEscape(assignmentValue)))
                {
                    var complexProps = new List<JProperty>();

                    //is complex type
                    //find the edges
                    ExplodeComplexTypeAndMemberAccess(ref assignmentValue, type, out var inversePaths, shouldTryCast: true);

                    foreach (var inversePath in inversePaths)
                    {
                        var actualMember = inversePath[0];
                        complexProps.Add(ResolveJPropertyFromAssignment
                            (jsonNamePropertyMap, dictMemberNames, jObject,
                            assignmentKeyInfo, assignmentKeyName,
                            actualMember, actualMember.Name, instanceTypeName));
                    }

                    filteredProps.AddRange(complexProps);

                    if (complexAssignments == null)
                        complexAssignments = new List<Tuple<object, Expression, Type, List<JProperty>>>();

                    complexAssignments.Add(new Tuple<object, Expression, Type, List<JProperty>>
                        (assignmentKey, assignmentValue, assignmentValue?.Type ?? type, complexProps));
                }
                else
                {
                    filteredProps.Add(ResolveJPropertyFromAssignment
                        (jsonNamePropertyMap, dictMemberNames, jObject,
                        assignmentKeyInfo, assignmentKeyName,
                        assignmentKeyInfo, assignmentKeyName, instanceTypeName));
                }
            }

            return filteredProps;
        }

        public static string BuildPaths(ref ICypherFluentQuery query, 
            IEnumerable<Expression<Func<IPathBuilder, IPathExtent>>> pathBuildExpressions,
            PropertiesBuildStrategy patternBuildStrategy)
        {
            StringBuilder stringBuilder = new StringBuilder();

            var pathBuilds = new List<string>();

            foreach(var pathExpr in pathBuildExpressions)
            {
                pathBuilds.Add(new PathBuilder(query, pathExpr)
                {
                    PatternBuildStrategy = patternBuildStrategy
                }.Build(ref query));
            }

            var pathsText = pathBuilds.Aggregate((first, second) => $"{first}, {second}");

            stringBuilder.Append(pathsText);

            return stringBuilder.ToString();
        }

        internal static QueryUtilities GetQueryUtilities(ICypherFluentQuery query)
        {
            var queryUtilities = new QueryUtilities();

            var client =  (query as IAttachedReference)?.Client;

            var _serializer = client?.Serializer ?? new CustomJsonSerializer()
            {
                JsonContractResolver = client?.JsonContractResolver ?? GraphClient.DefaultJsonContractResolver,
                JsonConverters = client?.JsonConverters ?? (IEnumerable<JsonConverter>)GraphClient.DefaultJsonConverters
            };

            var customJsonSerializer = _serializer as CustomJsonSerializer;

            var serializer = _serializer;

            var resolver = client?.JsonContractResolver as EntityResolver ??
                (serializer as CustomJsonSerializer)?.JsonContractResolver as EntityResolver;

            var converters = new List<JsonConverter>((IEnumerable<JsonConverter>)client?.JsonConverters ?? new JsonConverter[0]);
            converters.AddRange((serializer as CustomJsonSerializer)?.JsonConverters ?? new JsonConverter[0]);

            var converter = converters.FirstOrDefault(c => c is EntityConverter) as EntityConverter;

            Func<object, string> actualSerializer = null;

            if (_serializer != null)
            {
                actualSerializer = (obj) =>
                {
                    NullValueHandling? nullHandling = null;
                    if (customJsonSerializer != null)
                    {
                        nullHandling = customJsonSerializer.NullHandling; //save it.
                        customJsonSerializer.NullHandling = NullValueHandling.Include; //change it. we need all values
                    }

                    var serialized = _serializer.Serialize(obj);

                    if (customJsonSerializer != null)
                        customJsonSerializer.NullHandling = nullHandling.Value; //restore it.

                    return serialized;
                };
            }
            else
            {
                actualSerializer = null;
            }

            return new QueryUtilities()
            {
                Client = client,
                Converter = converter,
                ISerializer = serializer,
                Resolver = resolver,
                SerializeCallback = actualSerializer,
                CurrentQueryWriter = QueryUtilities.QueryWriterGetter(query),
                CurrentBuildStrategy = QueryUtilities.BuildStrategyGetter(query)
            };
        }

        internal static LambdaExpression GetConstraintsAsPropertiesLambda(LambdaExpression constraints, Type type)
        {
            var setMethod = GetGenericMethodInfo(GetMethodInfo(() => ObjectExtensions._Set<object>(null, null, true)), type);

            return Expression.Lambda(Expression.Call(setMethod, Expression.Constant(type.GetDefaultValue(), type),
                constraints, Expression.Constant(true) //i.e, usePredicateOnly: true
                ));
        }

        internal static string GetRandomVariableFor(string entity)
        {
            Random random;

            return "___"
                + $"{entity}"
                + (random = new Random(DateTime.UtcNow.Millisecond)).Next(1, 100)
                + random.Next(1, 100);
        }

        internal static object CreateInstance(Type type, bool nonPublic = true, object[] parameters = null)
        {
            object o = null;

            try
            {
                o = Activator.CreateInstance(type, parameters);
            }
            catch (MissingMemberException e)
            {
                if (nonPublic)
                {
                    var flags = BindingFlags.Instance | BindingFlags.NonPublic;

                    parameters = parameters ?? new object[0];

                    var constructors = type.GetConstructors(flags)
                        .Where(c => (c.GetParameters()?.Length ?? 0) == parameters.Length);

                    if (constructors?.Count() > 0)
                    {
                        foreach (var constructor in constructors)
                        {
                            try
                            {
                                o = constructor.Invoke(parameters);
                                break;
                            }
                            catch
                            {

                            }
                        }
                    }

                    if (o == null)
                        throw e;
                }
                else
                    throw e;
            }

            return o;
        }

        public static string SerializeMetadata(Metadata metadata)
        {
            //only distinct values
            metadata.NullProperties = metadata.NullProperties.Distinct().ToList();
            return JsonConvert.SerializeObject(metadata);
        }

        public static Metadata DeserializeMetadata(string metadataJson)
        {
            return JsonConvert.DeserializeObject<Metadata>(metadataJson);
        }

        internal static void EnsureRightJObject(ref JObject valueJObject)
        {
            //the neo4jclient guys really messed things up here
            //so use heuristics to determine if we are passing the right data or not, and then get the right data
            //this is for deserialization only

            //example json received
            /*
             {
              "extensions": {},
              "metadata": {
                "id": 176,
                "labels": [
                  "IdentityUser"
                ]
              },
              "paged_traverse": "http://localhost:7474/db/data/node/176/paged/traverse/{returnType}{?pageSize,leaseTime}",
              "outgoing_relationships": "http://localhost:7474/db/data/node/176/relationships/out",
              "outgoing_typed_relationships": "http://localhost:7474/db/data/node/176/relationships/out/{-list|&|types}",
              "labels": "http://localhost:7474/db/data/node/176/labels",
              "create_relationship": "http://localhost:7474/db/data/node/176/relationships",
              "traverse": "http://localhost:7474/db/data/node/176/traverse/{returnType}",
              "all_relationships": "http://localhost:7474/db/data/node/176/relationships/all",
              "all_typed_relationships": "http://localhost:7474/db/data/node/176/relationships/all/{-list|&|types}",
              "property": "http://localhost:7474/db/data/node/176/properties/{key}",
              "self": "http://localhost:7474/db/data/node/176",
              "incoming_relationships": "http://localhost:7474/db/data/node/176/relationships/in",
              "properties": "http://localhost:7474/db/data/node/176/properties",
              "incoming_typed_relationships": "http://localhost:7474/db/data/node/176/relationships/in/{-list|&|types}",
              "data": {
                actual data ...
              }
            } 
             */

            var expectedProps = new Dictionary<string, JTokenType>()
            {
                //{ "data", JTokenType.Object },
                { "metadata", JTokenType.Object },
                { "self", JTokenType.String },
            };

            var jObject = valueJObject;

            if (expectedProps.All(prop => jObject[prop.Key]?.Type == prop.Value))
            {
                //hopefully we are right
                //replace the jObject with "data"
                valueJObject = jObject["data"] as JObject;
            }
        }

        internal static void EnsureSerializerInstance(ref JsonSerializer serializer)
        {
            if (serializer == null)
            {
                //this is strange, but the Neo4jClient folks forgot to pass a serializer to this method
                serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings()
                {
                    Converters = GraphClient.DefaultJsonConverters?.Reverse().ToList(),
                    ContractResolver = GraphClient.DefaultJsonContractResolver,
                    ObjectCreationHandling = ObjectCreationHandling.Auto,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                });
            }
        }

        internal static void RemoveThisConverter(Type converterType, JsonSerializer serializer, out List<Tuple<JsonConverter, int>> convertersRemoved)
        {
            convertersRemoved = new List<Tuple<JsonConverter, int>>();

            for (int i = 0, l = serializer.Converters.Count; i < l; i++)
            {
                var converter = serializer.Converters[i];
                if (converterType.IsAssignableFrom(converter.GetType()))
                {
                    convertersRemoved.Add(new Tuple<JsonConverter, int>(converter, i));
                }
            }

            foreach (var converter in convertersRemoved)
            {
                serializer.Converters.Remove(converter.Item1);
            }
        }

        internal static void RestoreThisConverter(JsonSerializer serializer, List<Tuple<JsonConverter, int>> convertersRemoved,
            bool clearConvertersRemovedList = true)
        {
            foreach (var converter in convertersRemoved)
            {
                try
                {
                    serializer.Converters.Insert(converter.Item2, converter.Item1);
                }
                catch
                {
                    serializer.Converters.Add(converter.Item1);
                }
            }

            if (clearConvertersRemovedList)
                convertersRemoved.Clear();
        }

        internal static string BuildWithParamsForValues(JObject finalProperties, Func<object, string> serializer, 
            Func<string, string> getKey, string separator, Func<string, string> getValue, out bool hasRaw, out JObject newFinalProperties)
        {
            bool _hasRaw = false;
            JObject _newFinalProperties = null;

            var value = finalProperties.Properties()
                .Select(jp =>
                {
                    if (jp.Value?.Type == JTokenType.Raw)
                    {
                        //most likely a query variable
                        //do not use a parameter in this case
                        //write directly instead
                        //and remove from properties
                        _hasRaw = true;
                        if(_newFinalProperties == null)
                            _newFinalProperties = finalProperties.DeepClone() as JObject;

                        _newFinalProperties.Remove(jp.Name);
                        return $"{getKey(jp.Name)}{separator}{serializer(jp.Value)}";
                    }

                    return $"{getKey(jp.Name)}{separator}{getValue(jp.Name)}";
                })
                .Aggregate((first, second) => $"{first}, {second}");

            hasRaw = _hasRaw;
            newFinalProperties = _newFinalProperties;

            return value;
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
        /// <param name="resolver"></param>
        /// <param name="converter"></param>
        /// <param name="serializer"></param>
        /// <param name="isMemberAccess">If false, the variable is not included</param>
        internal static string[] GetVariableExpressions<T>(Expression<Func<T, object>> expressions, 
            QueryUtilities queryUtilities,
            bool isMemberAccess = true, string variable = null, 
            FunctionVisitorContext visitorContext = null)
        {
            return GetVariableExpressions(typeof(T), expressions, queryUtilities, isMemberAccess, variable, visitorContext);
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
        /// <param name="resolver"></param>
        /// <param name="converter"></param>
        /// <param name="serializer"></param>
        /// <param name="isMemberAccess">If false, the variable is not included</param>
        internal static string[] GetVariableExpressions(Type sourceType,
            LambdaExpression expressions, QueryUtilities queryUtilities, 
            bool isMemberAccess = true, string variable = null,
            FunctionVisitorContext visitorContext = null)
        {
            if (expressions == null)
                throw new ArgumentNullException(nameof(expressions));

            //a => a.Property
            //a => new { a.Property1, a.Property2 }

            var parameterExpr = expressions.Parameters?.FirstOrDefault();
            variable = variable ?? parameterExpr.Name;
            var randomVar = $"_ve_{GetRandomVariableFor(variable)}_rdm_";

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
            var funcsVisitor = new FunctionExpressionVisitor(queryUtilities, visitorContext);

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
                    var accesses = ExplodeComplexTypeMemberAccess (expr, out var inversePaths);
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
            var varsGetMethodInfo = GetGenericMethodInfo(GetMethodInfo(() => Vars.Get<object>(null)), type);
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
            var varsGetMethodInfo = GetGenericMethodInfo(GetMethodInfo(() => Vars.Get<object, object>(null)), source, result);
            //now Vars.Get<source, result>(selector)
            var varsGetCallExpr = Expression.Call(varsGetMethodInfo, selector);

            return varsGetCallExpr;
        }

        internal static string BuildProjectionQueryExpression(LambdaExpression expression,
            QueryUtilities queryUtilities, FunctionExpressionVisitor functionVisitor,
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
                    foreach(var p in expression.Parameters)
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
                var nfpInfo = GetMethodInfo(() => ObjectExtensions._<object>(null));
                if (bodyExpr is NewExpression newExpr)
                {
                    foreach(var arg in newExpr.Arguments)
                    {
                        //i.e., arg._()
                        replacements.Add(arg, Expression.Call(GetGenericMethodInfo(nfpInfo, arg.Type), arg));
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
                                        Expression.Call(GetGenericMethodInfo(nfpInfo, assignment.Expression.Type), assignment.Expression));
                                    break;
                                }
                            case MemberMemberBinding memberMemberBinding:
                                {
                                    foreach(var childBinding in memberMemberBinding.Bindings)
                                    {
                                        addBinding(childBinding);
                                    }
                                    break;
                                }
                        }
                    };

                    foreach(var binding in memberInitExpr.Bindings)
                    {
                        addBinding(binding);
                    }
                }

                //make the remaining replacements
                bodyExpr = replacerVisitor.Visit(bodyExpr);
                //create new lambda as we don't even need the parameters anymore.
                //i.e., () => bodyExpr
                expression = Expression.Lambda(bodyExpr);

                var finalProperties = GetFinalProperties(expression, queryUtilities, out bool hasFunctions);
                if (finalProperties == null)
                    //trouble
                    throw new InvalidOperationException(Messages.InvalidICypherResultItemExpressionError);

                string asterisk = "*";
                var serializer = queryUtilities.SerializeCallback;

                var buildStrategy = queryUtilities.CurrentBuildStrategy ?? PropertiesBuildStrategy.WithParams;
                var hasQueryWriter = queryUtilities.CurrentQueryWriter != null;

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
                                    value = queryUtilities.CurrentQueryWriter.CreateParameter(value);
                                    value = GetNewNeo4jParameterSyntax(value);
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

        public static string GetNewNeo4jParameterSyntax(string parameter)
        {
            if (parameter.StartsWith("{") && parameter.EndsWith("}"))
            {
                //this is the old parameter style
                //use the new one that uses $ sign
                parameter = $"${parameter.Substring(0, parameter.Length - 1).Remove(0, 1)}";
            }

            return parameter;
        }
    }
}
