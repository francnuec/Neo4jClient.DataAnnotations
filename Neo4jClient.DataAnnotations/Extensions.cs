using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Linq.Expressions;

namespace Neo4jClient.DataAnnotations
{
    public static class Extensions
    {
        public static T UseAnnotations<T>(this T client) where T : IGraphClient
        {
            Neo4jAnnotations.Register(client);

            return client;
        }

        public static bool IsGenericAssignableFrom(this Type genericType, Type givenType)
        {
            if (givenType == null)
                return false;

            if (genericType.IsAssignableFrom(givenType))
                return true;

            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.GetTypeInfo().IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.GetTypeInfo().IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.GetTypeInfo().BaseType;
            if (baseType == null) return false;

            return IsGenericAssignableFrom(genericType, baseType);
        }

        public static bool IsMatch(this Type propType, Type propertyType, bool includeIEnumerableArg = true)
        {
            return propType == propertyType
                      || (includeIEnumerableArg && Utilities.GetEnumerableGenericType(propType) == propertyType);
        }

        internal static IEnumerable<PropertyInfo> OrderNavs(this IEnumerable<PropertyInfo> props, EntityTypeInfo typeInfo)
        {
            //order them obeying explicit column order

            var columnFKs = typeInfo.ColumnAttributeFKs;

            return props
                .OrderBy(p => p.IsDefined(Defaults.ColumnType) ? (p.GetCustomAttribute(Defaults.ColumnType) as ColumnAttribute).Order
                : (columnFKs.Where(fk => fk.NavigationProperty == p && fk.ScalarProperty?.IsDefined(Defaults.ColumnType) == true)
                .Select(fk => (int?)(fk.ScalarProperty.GetCustomAttribute(Defaults.ColumnType) as ColumnAttribute).Order)
                .FirstOrDefault() ?? 0)) //first order by explicit column attribute
                .ThenBy(p => typeInfo.AllProperties.IndexOf(p)) //then by how they appear on the class.
                ;
        }

        internal static IEnumerable<PropertyInfo> UnionNavPropsWithFKs
            (this IEnumerable<PropertyInfo> navProps,
            EntityTypeInfo typeInfo,
            Type attributeToFilter,
            Func<ForeignKeyProperty, bool> fkFilter,
            out List<ForeignKeyProperty> FKs)
        {
            FKs = typeInfo
                .GetForeignKeysWithAttribute(attributeToFilter)
                .Where(fkFilter ?? (fk => true))
                .ToList();

            //add attributed fks that may have been set on scalar properties
            navProps = navProps
                .Union(FKs.Select(fk => fk.NavigationProperty))
                .OrderNavs(typeInfo);

            return navProps;
        }

        /// <summary>
        /// Naming and other processing escape. This instructs the expression visitors to use as specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T _<T>(this T obj)
        {
            return obj;
        }

        public static Boolean IsAnonymousType(this Type type)
        {
            Boolean hasCompilerGeneratedAttribute = type.GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
            Boolean nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            Boolean isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }

        public static object GetDefaultValue(this Type type)
        {
            if (type.GetTypeInfo().IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }

        public static void ExecuteExpression(this Expression expr)
        {
            var exprLambda = Expression.Lambda<Action>(expr);
            exprLambda.Compile().Invoke();
        }

        public static T ExecuteExpression<T>(this Expression expr)
        {
            var convertExpr = Expression.Convert(expr, typeof(T));
            var exprLambda = Expression.Lambda<Func<T>>(convertExpr);
            return (T)exprLambda.Compile().Invoke();
        }

        public static bool IsEquivalentTo(this MemberInfo @this, MemberInfo method)
        {
            return @this == method || IsEquivalentTo(@this, method?.Name, method?.DeclaringType);
        }

        public static bool IsEquivalentTo(this MemberInfo @this, string methodName, Type methodType)
        {
            return @this?.Name == methodName && @this?.DeclaringType == methodType;
        }

        public static IEnumerable<T> ExactOrEquivalent<T>(this IEnumerable<T> source, Func<T, T, bool> equivalence, T prototype)
        {
            return ExactOrEquivalent(source, (t, t2) => t.Equals(t2), equivalence, prototype);
        }

        public static IEnumerable<T> ExactOrEquivalent<T>(this IEnumerable<T> source,
            Func<T, T, bool> exactness, Func<T, T, bool> equivalence, T prototype)
        {
            //first match perfectly
            var match = source.Where(t => exactness(t, prototype)).ToList();

            if (match == null || match.Count == 0)
            {
                //then find its equivalence instead
                match = source.Where(t => equivalence(t, prototype)).ToList();
            }

            return match ?? new List<T>();
        }

        internal static IEnumerable<T> ExactOrEquivalentMember<T>
            (this IEnumerable<T> source, Func<T, MemberInfo> selector, T member)
        {
            return ExactOrEquivalent(source, (t1, t2) => selector(t1) == selector(t2), (t1, t2) => selector(t1).IsEquivalentTo(selector(t2)), member);
        }
    }
}
