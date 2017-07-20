using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Linq.Expressions;
using Neo4jClient.DataAnnotations.Expressions;
using Neo4jClient.DataAnnotations.Cypher;

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
        /// No Further Processing. Naming and other processing escape. This instructs the expression visitors to use as specified.
        /// NOTE: This method does not affect serialization. So inner complex typed properties would still serialize to exploded properties as expected.
        /// This method is mainly used by the expression visitors, and mostly at the top surface level (rarely deep into the object).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T _<T>(this T obj)
        {
            return obj;
        }

        /// <summary>
        /// Casts an object to a certain type so as to use its properties directly.
        /// Use only in expressions, especially with <see cref="Params"/> method calls (in which case it Pseudo-casts).
        /// NOTE: THIS METHOD IS NOT SAFE TO EXECUTE. If the cast fails, it merely generates a default value for the return type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TReturn As<TReturn>(this object obj)
        {
            //do something, just in case this method was executed
            TReturn ret = default(TReturn);

            try
            {
                ret = (TReturn)obj;
            }
            catch
            {

            }

            return ret;
        }

        public static bool IsAnonymousType(this Type type)
        {
            Boolean hasCompilerGeneratedAttribute = type.GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
            Boolean nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            Boolean isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }

        public static bool IsDictionaryType(this Type type)
        {
            return Defaults.DictionaryType.IsGenericAssignableFrom(type);
        }

        public static object GetDefaultValue(this Type type)
        {
            if (type.GetTypeInfo().IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }

        public static void ExecuteExpression(this Expression expr)
        {
            if (expr.NodeType == ExpressionType.Lambda)
            {
                try
                {
                    (expr as Expression<Action>).Compile().Invoke();
                    return;
                }
                catch
                {
                    //try our way then
                }
            }

            var exprLambda = Expression.Lambda<Action>(expr);
            exprLambda.Compile().Invoke();
        }

        public static T ExecuteExpression<T>(this Expression expr)
        {
            if (expr.NodeType == ExpressionType.Lambda)
            {
                try
                {
                    return (expr as Expression<Func<T>>).Compile().Invoke();
                }
                catch
                {
                    //try our way then
                }
            }

            if (expr?.Type != typeof(T))
            {
                expr = Expression.Convert(expr, typeof(T));
            }
            
            var exprLambda = Expression.Lambda<Func<T>>(expr);
            return exprLambda.Compile().Invoke();
        }

        public static bool IsEquivalentTo(this MemberInfo @this, MemberInfo member)
        {
            Type memberType = (member as PropertyInfo)?.PropertyType 
                ?? (member as FieldInfo)?.FieldType ?? (member as MethodInfo)?.ReturnType;

            return @this == member || (memberType != null ? IsEquivalentTo(@this, member?.Name, member?.DeclaringType, memberType) //use the longer version 
                : IsEquivalentTo(@this, member?.Name, member?.DeclaringType));
        }

        public static bool IsEquivalentTo(this MemberInfo @this, string memberName, Type memberDeclaringType)
        {
            return @this?.Name == memberName && @this?.DeclaringType == memberDeclaringType;
        }

        public static bool IsEquivalentTo(this MemberInfo @this, string memberName, Type memberDeclaringType, Type memberType)
        {
            Type thisType = (@this as PropertyInfo)?.PropertyType
                ?? (@this as FieldInfo)?.FieldType ?? (@this as MethodInfo)?.ReturnType;

            return IsEquivalentTo(@this, memberName, memberDeclaringType) && thisType == memberType;
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

        /// <summary>
        /// Allows you to expressively assign new values (especially <see cref="Params"/>) to select properties of an existing instance without cloning or modifying the original instance.
        /// These new values would be used in place of the old ones on the instance.
        /// E.g. () =&gt; ellenPompeo.Set(actor =&gt; actor.Born == Params.Get&lt;Actor&gt;("shondaRhimes").Born);
        /// NOTE: The member selection must always be on the left, and new values on the right of a logical equal-to ('==') operation. Use '&&' for more properties.
        /// Also note that this method does not modify this instance, but overrides its final properties written to cypher.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T Set<T>(this T instance, Expression<Func<T, bool>> predicate)
        {
            return Set(instance, predicate, usePredicateOnly: false);
        }
         
        internal static T Set<T>(this T instance, Expression<Func<T, bool>> predicate, bool usePredicateOnly)
        {
            return instance;
        }

        public static Expression Uncast(this Expression expression, out Type castRemoved, Type castToRemove = null)
        {
            castRemoved = null;

            switch (expression.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.TypeAs:
                case ExpressionType.Unbox:
                    {
                        var unary = (expression as UnaryExpression);
                        if (castToRemove == null || castToRemove == unary.Type)
                        {
                            castRemoved = unary.Type;
                            return unary.Operand;
                        }

                        break;
                    }
            }

            return expression;
        }

        public static Expression Cast(this Expression expression, out Type castAdded, Type castToAdd = null)
        {
            castAdded = null;

            if (castToAdd == null)
            {
                //try executing this to get the actual type of the object returned and use that type
                try
                {
                    var obj = expression.ExecuteExpression<object>();
                    castToAdd = obj?.GetType();
                }
                catch
                {

                }
            }

            if (castToAdd != null && expression.Type != castToAdd)
            {
                //cast it to its appropriate type
                try
                {
                    //try as first
                    expression = Expression.TypeAs(expression, castToAdd);
                }
                catch
                {
                    //rejected, so use normal convert
                    expression = Expression.Convert(expression, castToAdd);
                }

                castAdded = castToAdd;
            }

            return expression;
        }

        public static Expression SafeCast(this Expression expression, out Type castAdded, Type castToAdd = null)
        {
            castAdded = null;

            try
            {
                return Cast(expression, out castAdded, castToAdd);
            }
            catch
            {

            }

            return expression;
        }

        public static bool IsExtensionMethod(this MethodInfo method)
        {
            return method.IsDefined(typeof(ExtensionAttribute));
        }

        public static bool IsLocalMember(this MemberExpression expression)
        {
            var constantExpression = expression.Expression as ConstantExpression;

            if (constantExpression != null)
            {
                var type = constantExpression.Type;
                var typeInfo = type.GetTypeInfo();

                //compilergeneratedattribute
                //isnested
                //isnestedprivate
                //issealed
                //has reflectedtype
                //membertype is nestedtype
                //<>
                //DisplayClass

                return typeInfo.IsDefined(typeof(CompilerGeneratedAttribute))
                    && typeInfo.IsNested
                    && typeInfo.IsNestedPrivate
                    && typeInfo.IsSealed
                    && typeInfo.Name.StartsWith("<>")
                    && typeInfo.Name.Contains("DisplayClass");
            }

            return false;
        }

        public static bool IsEntity(this Expression expression)
        {
            return Neo4jAnnotations.ContainsEntityType(expression.Type);
        }

        public static bool IsScalar(this Type type)
        {
            return Utilities.IsTypeScalar(type);
        }

        /// <summary>
        /// Placeholder method for predicate member values in expressions.
        /// </summary>
        /// <typeparam name="TReturn">The last return type of the contiguous access stretch.</typeparam>
        /// <param name="index">The index of the value in the store.</param>
        /// <returns>The executed expression value, or a default value of the return type.</returns>
        internal static TReturn GetValue<TReturn>(this EntityExpressionVisitor visitor, int index)
        {
            var specialNodeExpr = visitor.SpecialNodes[index];

            return (TReturn)(specialNodeExpr.ConcreteValue ?? typeof(TReturn).GetDefaultValue());
        }

        public static bool IsComplex(this Type type)
        {
            return type.GetTypeInfo().IsDefined(Defaults.ComplexType);
        }

        public static Type GetMemberType(this MemberInfo member)
        {
            return (member as PropertyInfo)?.PropertyType ?? (member as FieldInfo)?.FieldType ?? (member as MethodInfo)?.ReturnType;
        }
    }
}
