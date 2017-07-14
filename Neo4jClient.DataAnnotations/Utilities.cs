using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using Neo4jClient.DataAnnotations.Cypher;
using System.Text;

namespace Neo4jClient.DataAnnotations
{
    public class Utilities
    {
        /// <summary>
        /// Get all the <see cref="TableAttribute"/> names on the class inheritance as labels.
        /// Should none be gotten, the type name is used instead by default.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> GetLabels(Type type, bool useTypeNameIfEmpty = true)
        {
            var typeInfo = type.GetTypeInfo();
            var attributes = typeInfo.GetCustomAttributes<TableAttribute>(true)?.ToList();

            if (attributes != null && attributes.Count > 0)
            {
                var ret = attributes.Where(a => !string.IsNullOrWhiteSpace(a.Name)).Select(a => a.Name).Distinct().ToList();

                if (ret.Count > 0)
                    return ret;
            }

            return useTypeNameIfEmpty ? new List<string> { type.Name } : new List<string>();
        }

        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            return GetPropertyInfo(propertyLambda, typeof(TSource), typeof(TProperty));
        }

        public static PropertyInfo GetPropertyInfo(LambdaExpression propertyLambda, Type sourceType, Type propertyType,
             bool includeIEnumerableArg = true)
        {
            MemberExpression memberExpr = propertyLambda.Body as MemberExpression;
            if (memberExpr == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = memberExpr.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (sourceType != null)
            {
                if (sourceType != propInfo.DeclaringType &&
                !propInfo.DeclaringType.IsAssignableFrom(sourceType))
                    throw new ArgumentException(string.Format(
                        "Expresion '{0}' refers to a property that is not from type '{1}'.",
                        propertyLambda.ToString(),
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

            var interfaceType = type.GetInterfaces()?.Where(i => i.GetTypeInfo().IsGenericType
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

        public static bool IsTypeScalar(Type type)
        {
            repeat:
            if (type != null && !(Defaults.ScalarTypes.Contains(type)
                || type.GetTypeInfo().IsPrimitive
                || type.GetTypeInfo().IsDefined(Defaults.NeoScalarType)))
            {
                //check if it's an array/iEnumerable before concluding
                Type genericType = null;

                if ((genericType = GetEnumerableGenericType(type)) != null
                    || (genericType = GetNullableUnderlyingType(type)) != null)
                {
                    type = genericType;
                    goto repeat;
                }

                return false;
            }

            return type != null;
        }

        public static MethodInfo GetMethodInfo(Expression<Action> expression)
        {
            var member = expression.Body as MethodCallExpression;

            if (member != null)
                return member.Method;

            throw new ArgumentException("Expression is not a method", "expression");
        }

        public static void InitializeComplexTypedProperties(object entity)
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
                    instance = Activator.CreateInstance(complexProp.PropertyType);
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
                            else if (IsTypeScalar(lastType))
                            {
                                nextInstance = lastType.GetDefaultValue();
                            }
                            else
                            {
                                nextInstance = Activator.CreateInstance(lastType);
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

        internal static string[] GetEntityPathNames(ref object entity, Type entityType,
            List<Expression> expressions,
            ref int currentIndex, EntityResolver resolver, Func<object, string> serializer,
            out Dictionary<MemberInfo, Tuple<int, Type>> members, out Type lastType, bool useResolvedJsonName = true)
        {
            string[] memberNames = new string[0];

            entityType = entityType ?? entity.GetType();
            var entityInfo = Neo4jAnnotations.GetEntityTypeInfo(entityType);

            //do this to avoid create instances every time we call this method for a particular type
            bool buildPath = useResolvedJsonName && resolver == null && entityInfo.JsonNamePropertyMap.Count == 0;

            int index = currentIndex; //store this index incase of a repeat

            repeatBuild:
            if (buildPath && entity == null)
            {
                //most likely using the EntityConverter
                //create new instance
                entity = Activator.CreateInstance(entityType);
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
                    !((m.Key as PropertyInfo)?.PropertyType ?? (m.Key as FieldInfo)?.FieldType)?
                    .GetTypeInfo().IsDefined(Defaults.ComplexType) == true)
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
                            && (parentType?.GetTypeInfo().IsDefined(Defaults.ComplexType) == true
                            || m.Key.DeclaringType.GetTypeInfo().IsDefined(Defaults.ComplexType)
                            || m.Value.Item2.GetTypeInfo().IsDefined(Defaults.ComplexType))))
                        )
                        {
                            //maybe we were wrong and the jsonMaps are empty
                            //or just to be sure we have the actual name,
                            //repeat with a fully built path and serialization
                            buildPath = true;
                            gotoRepeatBuild = true;
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

        internal static List<Expression> ExpandComplexTypeAccess(Expression expression, out List<List<MemberInfo>> inversePaths)
        {
            Type type = expression.Type;

            inversePaths = new List<List<MemberInfo>>();

            if (type == null || !type.GetTypeInfo().IsDefined(Defaults.ComplexType))
                return new List<Expression>();

            var result = new List<Expression>();

            var info = Neo4jAnnotations.GetEntityTypeInfo(type);

            foreach (var prop in info.AllProperties)
            {
                var newExpr = Expression.MakeMemberAccess(expression, prop);

                if (IsTypeScalar(prop.PropertyType))
                {
                    result.Add(newExpr);
                    inversePaths.Add(new List<MemberInfo>() { prop });
                }
                else
                {
                    //recursively check till we hit the last scalar property
                    result.AddRange(ExpandComplexTypeAccess(newExpr, out var members));
                    inversePaths.AddRange(members.Select(ml => { ml.Add(prop); return ml; }));
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
            MethodCallExpression methodExpr = null;
            return expression != null && (methodExpr = expression as MethodCallExpression) != null
                && methodExpr.Method.IsEquivalentTo("_", Defaults.ExtensionsType);
        }

        public static bool HasParams(List<Expression> expressions)
        {
            return HasParams(expressions, out var methodExpr);
        }

        public static bool HasParams(List<Expression> expressions, out MethodCallExpression methodExpr)
        {
            methodExpr = null;
            return expressions != null && expressions.Count > 0
                && (methodExpr = expressions[0] as MethodCallExpression) != null
                && methodExpr.Method.IsEquivalentTo("Get", Defaults.ParamsType);
        }

        public static string BuildParams(List<Expression> expressions, EntityResolver resolver,
            Func<object, string> serializer, out Type typeReturned, bool? useResolvedJsonName = null)
        {
            typeReturned = null;
            if (!HasParams(expressions, out var methodExpr))
                return null;

            if (useResolvedJsonName == null)
            {
                //check the last expression for nfp escape
                useResolvedJsonName = !HasNfpEscape(expressions.Last().Uncast(out var castType));
            }

            var getMethod = methodExpr.Method;

            var entityType = typeReturned = getMethod.ReturnType;

            var builder = new StringBuilder();

            var currentIndex = -1;

            //append the variable name first
            var argumentExpr = methodExpr.Arguments[++currentIndex];
            var argument = argumentExpr.ExecuteExpression<string>();

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
                    object entity = null; //Activator.CreateInstance(entityType);

                    var memberNames = GetEntityPathNames(ref entity, entityType, expressions, ref currentIndex, resolver, serializer,
                        out var members, out var lastType, useResolvedJsonName: useResolvedJsonName.Value);

                    if (memberNames.Length > 0)
                    {
                        memberJsonName = memberNames.Aggregate((first, second) => $"{first}.{second}").Trim('.');
                        typeReturned = lastType ?? typeReturned;
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

                        typeReturned = expr.Method.ReturnType;
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

            //append indexers
            if (expressions.Count > ++currentIndex)
            {
                int tmpIdx = currentIndex;

                Expression arrayIndexExpr = null;

                //expecting either "ElementAt" methodCall, or ArrayIndex expression
                for (int i = currentIndex, l = expressions.Count; i < l; i++)
                {
                    var expr = expressions[i];

                    switch (expr.NodeType)
                    {
                        case ExpressionType.ArrayIndex:
                            {
                                var binExpr = expr as BinaryExpression;
                                arrayIndexExpr = binExpr.Right;

                                typeReturned = binExpr.Type;
                                break;
                            }
                        case ExpressionType.Call:
                            {
                                var callExpr = expr as MethodCallExpression;
                                if (callExpr.Method.Name == "ElementAt"
                                    && callExpr.Method.DeclaringType == typeof(Enumerable))
                                {
                                    //found our extension method.
                                    arrayIndexExpr = callExpr.Arguments[1]; //because it is extension method, the first argument would be the instance (this) argument. so we take the second one.

                                    typeReturned = callExpr.Type;
                                }

                                break;
                            }
                            //case ExpressionType.TypeAs:
                            //case ExpressionType.Convert:
                            //case ExpressionType.ConvertChecked:
                            //case ExpressionType.Unbox:
                            //    {
                            //        typeReturned = (expr as UnaryExpression).Type ?? typeReturned;
                            //        break;
                            //    }
                    }

                    currentIndex = i; //update the current index always

                    if (arrayIndexExpr != null)
                        break;
                }

                string arrayIndexStr = null;

                if (arrayIndexExpr != null)
                {
                    //try executing it first
                    //if it fails, maybe we have a nested Params call
                    try
                    {
                        var arrayIndex = arrayIndexExpr.ExecuteExpression<int?>();
                        arrayIndexStr = arrayIndex?.ToString();
                    }
                    catch (NotImplementedException e) when (e.Message == Messages.ParamsGetError)
                    {
                        //check if params
                        var retrievedExprs = GetSimpleMemberAccessStretch(arrayIndexExpr, out var val);
                        if (HasParams(retrievedExprs))
                        {
                            //get the params string
                            arrayIndexStr = BuildParams(retrievedExprs, resolver, serializer, out var childTypeReturned, useResolvedJsonName: null);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(arrayIndexStr))
                {
                    builder.Append($"[{arrayIndexStr}]");
                }
                else
                {
                    //restore last index less 1.
                    currentIndex = tmpIdx - 1;
                }
            }

            //go through the rest to see if the typereturned changes
            if (expressions.Count > ++currentIndex)
            {
                for (int i = currentIndex, l = expressions.Count; i < l; i++)
                {
                    var expr = expressions[i];

                    switch (expr.NodeType)
                    {
                        case ExpressionType.TypeAs:
                        case ExpressionType.Convert:
                        case ExpressionType.ConvertChecked:
                        case ExpressionType.Unbox:
                            {
                                typeReturned = (expr as UnaryExpression).Type ?? typeReturned;
                                break;
                            }
                    }
                }
            }

            return builder.ToString().Trim();
        }

        public static List<Expression> GetSimpleMemberAccessStretch(Expression expression, out Expression entityBestGuess)
        {
            var filtered = new List<Expression>();

            var currentExpression = expression;

            Expression localExpr = null, entityDisjointExpr = null, nfpExpr = null, 
                methodExpr = null, parentEntityExpr = null, paramExpr = null;

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
                    case ExpressionType.Parameter:
                        {
                            paramExpr = currentExpression;
                            currentExpression = null;
                            break;
                        }
                    default:
                        {
                            currentExpression = null;
                            break;
                        }
                }
            }

            filtered.Reverse();

            //determine where our value is by some heuristics
            entityBestGuess = nfpExpr ?? paramExpr ?? entityDisjointExpr ?? localExpr ?? methodExpr ?? parentEntityExpr ?? filtered.FirstOrDefault();

            return filtered;
        }

        /// <summary>
        /// Marker method for <see cref="Params"/> class calls in expressions.
        /// </summary>
        /// <typeparam name="TReturn">The last return type of the contiguous access stretch.</typeparam>
        /// <param name="index">The index of the actual expression called in the store.</param>
        /// <returns>A default value of the return type.</returns>
        internal static TReturn GetParams<TReturn>(int index)
        {
            return (TReturn)typeof(TReturn).GetDefaultValue();
        }

        public static bool HasWith(Expression expression)
        {
            return HasWith(expression, out var methodExpr);
        }

        public static bool HasWith(Expression expression, out MethodCallExpression methodExpr)
        {
            methodExpr = null;
            return (methodExpr = expression as MethodCallExpression) != null
                && methodExpr.Method.IsEquivalentTo("With", Defaults.ExtensionsType);
        }

        public static void CheckIfComplexTypeInstanceIsNull(object instance, string propertyName, Type declaringType)
        {
            if (instance == null)
            {
                //Complex types cannot be null. A value must be provided always.
                throw new InvalidOperationException(string.Format(Messages.NullComplexTypePropertyError, propertyName, declaringType.Name));
            }
        }
    }
}
