using System;
using Neo4jClient.DataAnnotations.Utils;
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
using System.Runtime.CompilerServices;

namespace Neo4jClient.DataAnnotations.Utils
{
    public static class Utilities
    {
        private static readonly Random variableRandom = new Random();

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

        public static bool IsScalarType(Type type, IEntityService entityService)
        {
            bool isScalar = false;
            if ((entityService as EntityService)?.processedScalarTypes.TryGetValue(type, out isScalar) == true)
                return isScalar;

            Func<Type, bool> isNav = (_type) =>
            {
                var _typeInfo = _type?.GetTypeInfo();
                var ret = (_typeInfo.IsClass
                || _typeInfo.IsInterface
                || _typeInfo.IsDefined(Defaults.NeoNonScalarType)
                || entityService.KnownNonScalarTypes.Contains(_type))
                && !entityService.KnownScalarTypes.Contains(_type)
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
                        (entityService as EntityService).processedScalarTypes[type] = false;
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
                    (entityService as EntityService).processedScalarTypes[type] = true;
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

        internal static void InitializeComplexTypedProperties(object entity, IEntityService entityService)
        {
            var entityInfo = entityService.GetEntityTypeInfo(entity.GetType());

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
                    instance = CreateInstance(complexProp.PropertyType);
                    complexProp.SetValue(entity, instance);
                }

                //recursively do this for its own properties
                if (instance != null)
                    InitializeComplexTypedProperties(instance, entityService);
            }
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

        /// <summary>
        /// Placeholder method for <see cref="Vars"/> class calls in expressions.
        /// </summary>
        /// <typeparam name="TReturn">The last return type of the contiguous access stretch.</typeparam>
        /// <param name="index">The index of the actual expression in the store.</param>
        /// <returns>A default value of the return type.</returns>
        internal static TReturn GetValue<TReturn>(int index)
        {
            return GetValue<TReturn>(index, createNewInstance: false);
        }

        internal static TReturn GetValue<TReturn>(int index, bool createNewInstance = false)
        {
            var ret = (TReturn)typeof(TReturn).GetDefaultValue();

            if (createNewInstance && ret == null)
            {
                //this shouldn't be null
                try
                {
                    ret = (TReturn)CreateInstance(typeof(TReturn), nonPublic: true);
                }
                catch
                {

                }
            }

            return ret;
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

        internal static string GetRandomVariableFor(string entity)
        {
            //var guid = Guid.NewGuid().ToString("N");
            //var guidLength = guid.Length;

            //Random random = new Random(DateTime.UtcNow.Millisecond);
            //var first = random.Next(0, guidLength);
            //var second = random.Next(1, 100);

            lock (variableRandom)
            {
                return "___" + $"{entity}"
                    + variableRandom.Next(1, 500)
                    + variableRandom.Next(500, 999);
            }
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

        public static FieldInfo GetBackingField(PropertyInfo pi)
        {
            if (!pi.CanRead || !pi.GetGetMethod(nonPublic: true).IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
                return null;
            var backingField = pi.DeclaringType.GetField($"<{pi.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (backingField == null)
                return null;
            if (!backingField.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
                return null;
            return backingField;
        }
    }
}
