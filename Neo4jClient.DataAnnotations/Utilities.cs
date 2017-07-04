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

        public static bool IsEntityPropertyTypeScalar(Type propType)
        {
            repeat:
            if (propType != null && !(Defaults.ScalarTypes.Contains(propType)
                || propType.GetTypeInfo().IsPrimitive
                || propType.GetTypeInfo().IsDefined(Defaults.NeoScalarType)))
            {
                //check if it's an array/iEnumerable before concluding
                Type genericType = null;

                if ((genericType = GetEnumerableGenericType(propType)) != null
                    || (genericType = GetNullableUnderlyingType(propType)) != null)
                {
                    propType = genericType;
                    goto repeat;
                }

                return false;
            }

            return propType != null;
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

        internal static void BuildEntityPath
            (object entity, List<Expression> pathExpressions,
            ref int index, out Type lastType,
            out List<MemberInfo> pathTraversed,
            bool getTypeReturnedOnly = false)
        {
            lastType = null;

            pathTraversed = new List<MemberInfo>();

            object currentInstance = null;
            object nextInstance = entity;

            PropertyInfo propInfo = null;
            FieldInfo fieldInfo = null;

            bool breakLoop = false;

            for (int i = index, l = pathExpressions.Count; i < l; i++)
            {
                var expr = pathExpressions[i];

                switch (expr.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        {
                            var memberAccessExpr = expr as MemberExpression;
                            propInfo = memberAccessExpr.Member as PropertyInfo;
                            fieldInfo = memberAccessExpr.Member as FieldInfo;

                            lastType = propInfo?.PropertyType ?? fieldInfo?.FieldType ?? lastType;

                            if (propInfo != null)
                            {
                                pathTraversed.Add(propInfo);
                            }
                            else
                            {
                                pathTraversed.Add(fieldInfo);
                            }

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

                index = i; //update the current index always

                //get the existing member instance
                nextInstance = propInfo != null ? propInfo.GetValue(currentInstance) : fieldInfo.GetValue(currentInstance);

                if (!getTypeReturnedOnly)
                {
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

        //internal static string GetEntityPathName(object entity, List<Expression> expressions, 
        //    Func<object, string> serializer, ref int currentIndex, 
        //    out Type typeReturned, bool useResolvedJsonName = true)
        //{
        //    string memberJsonName = "";
        //    Type entityType = entity.GetType();

        //    BuildEntityPath(entity, expressions, ref currentIndex, out var lastType,
        //        out var members, getTypeReturnedOnly: !useResolvedJsonName);

        //    typeReturned = lastType;

        //    //get the last member
        //    var memberInfo = members.LastOrDefault();

        //    if (memberInfo != null)
        //    {
        //        if (useResolvedJsonName)
        //        {
        //            //take care of the entity's complex properties and those of its children
        //            Utilities.InitializeComplexTypedProperties(entity);

        //            bool entityTypeAutoAdded = false;

        //            if (entityTypeAutoAdded = !Neo4jAnnotations.EntityTypes.Contains(entityType))
        //            {
        //                Neo4jAnnotations.AddEntityType(entityType); //just in case it wasn't already added.
        //            }

        //            //serialize the entity so the jsonnames would be set
        //            var serializedEntity = serializer(entity);

        //            ////get the jsonName of the last member from entityInfo
        //            //var entityInfo = Neo4jAnnotations.GetEntityTypeInfo(entityType);
        //            //var entry = entityInfo.JsonNamePropertyMap.FirstOrDefault(pm => pm.Value.IsEquivalentTo(memberInfo));

        //            //memberJsonName = entry.Key;
        //        }

        //        if (string.IsNullOrWhiteSpace(memberJsonName))
        //        {
        //            //operation probably not successful
        //            //build alternate name
        //            if (members.Count > 0)
        //            {
        //                string alternateName = members.Select((m, idx) =>
        //                {
        //                    string name = m.Name;
        //                    if (useResolvedJsonName)
        //                    {
        //                        //try to get name from entity info
        //                        Type parentType = m.DeclaringType;

        //                        //we do this because MemberInfo.ReflectedType is not public yet in the .NET Core API.
        //                        var infos = Neo4jAnnotations.GetDerivedEntityTypeInfos(parentType);

        //                        var result = infos.SelectMany(info => info.JsonNamePropertyMap)
        //                        .ExactOrEquivalentMember((pair) => pair.Value, new KeyValuePair<string, MemberInfo>("", m))
        //                        .FirstOrDefault();

        //                        name = result.Key ?? name;
        //                    }

        //                    return name;
        //                }).Aggregate((first, second) => $"{first}.{second}");

        //                memberJsonName = alternateName;
        //            }
        //        }
        //    }

        //    return memberJsonName;
        //}

        internal static string[] GetEntityPathNames(object entity, List<Expression> expressions,
            ref int currentIndex, Func<object, string> serializer,
            out List<MemberInfo> members, out Type lastType, bool useResolvedJsonName = true)
        {
            string[] memberNames = new string[0];

            Type entityType = entity.GetType();

            BuildEntityPath(entity, expressions, ref currentIndex, out lastType,
                out members, getTypeReturnedOnly: !useResolvedJsonName);

            if (members.Count > 0)
            {
                if (useResolvedJsonName)
                {
                    //take care of the entity's complex properties and those of its children
                    Utilities.InitializeComplexTypedProperties(entity);

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
                        Neo4jAnnotations.EntityTypes.Remove(entityType);
                    }
                }

                var entityInfo = Neo4jAnnotations.GetEntityTypeInfo(entityType);

                //first get the members that are not complex typed.
                //this is because their child members would be the actual member of interest
                //if none found, or name returns empty, try all members then.

                var membersToUse = members.Where(m => !((m as PropertyInfo)?.PropertyType ?? (m as FieldInfo)?.FieldType)?
                    .GetTypeInfo().IsDefined(Defaults.ComplexType) == true).ToList();

                bool repeated = false;

                repeat:
                memberNames = membersToUse.Select((m, idx) =>
                {
                    string name = null;
                    if (useResolvedJsonName)
                    {
                        //try the entityInfo first
                        name = entityInfo.JsonNamePropertyMap.FirstOrDefault(pm => pm.Value.IsEquivalentTo(m)).Key;

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            //try to get name from entity info
                            Type parentType = m.DeclaringType;

                            //we do this because MemberInfo.ReflectedType is not public yet in the .NET Core API.
                            var infos = Neo4jAnnotations.GetDerivedEntityTypeInfos(parentType);

                            var result = infos.SelectMany(info => info.JsonNamePropertyMap)
                            .ExactOrEquivalentMember((pair) => pair.Value, new KeyValuePair<string, MemberInfo>("", m))
                            .FirstOrDefault();

                            name = result.Key;
                        }
                    }
                    else
                    {
                        name = m.Name;
                    }

                    return name;
                }).Where(n => !string.IsNullOrWhiteSpace(n)).ToArray();

                if (!repeated && memberNames.Length == 0)
                {
                    //repeat with all members
                    membersToUse = members;
                    repeated = true;
                    goto repeat;
                }
            }

            return memberNames;
        }

        internal static List<Expression> ExpandComplexTypeAccess(Expression expression, out List<List<MemberInfo>> paths)
        {
            Type type = expression.Type;

            paths = new List<List<MemberInfo>>();

            if (type == null || !type.GetTypeInfo().IsDefined(Defaults.ComplexType))
                return new List<Expression>();

            var result = new List<Expression>();

            var info = Neo4jAnnotations.GetEntityTypeInfo(type);

            foreach (var prop in info.AllProperties)
            {
                var newExpr = Expression.MakeMemberAccess(expression, prop);

                if (IsEntityPropertyTypeScalar(prop.PropertyType))
                {
                    result.Add(newExpr);
                    paths.Add(new List<MemberInfo>() { prop });
                }
                else
                {
                    //recursively check till we hit the last scalar property
                    result.AddRange(ExpandComplexTypeAccess(newExpr, out var members));
                    paths.AddRange(members.Select(ml => { ml.Add(prop); return ml; }));
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

        public static string BuildParams(List<Expression> expressions,
            Func<object, string> serializer, out Type typeReturned, bool useResolvedJsonName = true)
        {
            typeReturned = null;
            if (!HasParams(expressions, out var methodExpr))
                return null;

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
                    var entity = Activator.CreateInstance(entityType);

                    var memberNames = GetEntityPathNames(entity, expressions, ref currentIndex, serializer,
                        out var members, out var lastType, useResolvedJsonName: useResolvedJsonName);

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

                int? arrayIndex = null;

                //expecting either "ElementAt" methodCall, or ArrayIndex expression
                for (int i = currentIndex, l = expressions.Count; i < l; i++)
                {
                    var expr = expressions[i];

                    switch (expr.NodeType)
                    {
                        case ExpressionType.ArrayIndex:
                            {
                                var binExpr = expr as BinaryExpression;
                                arrayIndex = binExpr.Right.ExecuteExpression<int?>();

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
                                    var argExpr = callExpr.Arguments[1]; //because it is extension method, the first argument would be the instance (this) argument. so we take the second one.
                                    arrayIndex = argExpr.ExecuteExpression<int?>();

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

                    if (arrayIndex != null)
                        break;
                }

                if (arrayIndex != null)
                {
                    builder.Append($"[{arrayIndex}]");
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

        public static List<Expression> GetValidSimpleAccessStretch(Expression expression)
        {
            var filtered = new List<Expression>();

            var currentExpression = expression;

            while (currentExpression != null)
            {
                filtered.Add(currentExpression);

                switch (currentExpression.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        {
                            currentExpression = (currentExpression as MemberExpression).Expression;
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
                                && methodCallExpr.Method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute)))
                            {
                                //pick first argument
                                currentExpression = methodCallExpr.Arguments[0];
                            }

                            break;
                        }
                    case ExpressionType.ArrayIndex:
                        {
                            currentExpression = (currentExpression as BinaryExpression).Left;
                            break;
                        }
                    default:
                        {
                            filtered.Remove(currentExpression);
                            currentExpression = null;
                            break;
                        }
                }
            }

            filtered.Reverse();

            return filtered;
        }

        /// <summary>
        /// Marker method for <see cref="Params"/> class calls in expressions.
        /// </summary>
        /// <typeparam name="TReturn">The last return type of the contiguous access stretch.</typeparam>
        /// <param name="index">The index of the actual expression called in the store</param>
        /// <returns>A default value of the return type.</returns>
        internal static TReturn GetParams<TReturn>(int index)
        {
            return (TReturn)typeof(TReturn).GetDefaultValue();
        }
    }
}
