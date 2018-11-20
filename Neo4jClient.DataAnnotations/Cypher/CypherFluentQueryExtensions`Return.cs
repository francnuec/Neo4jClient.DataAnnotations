using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Linq.Expressions;
using System.Linq;
using Neo4jClient.Cypher;
using System.Reflection;
using Neo4jClient.DataAnnotations.Expressions;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public static partial class CypherFluentQueryExtensions
    {
        #region Return
        internal static ICypherFluentQuery<TResult> SharedReturn<TResult>(this ICypherFluentQuery query, LambdaExpression expression)
        {
            return SharedProjectionQuery<TResult>(query, expression, "RETURN", isOutputQuery: true);
        }

        public static ICypherFluentQuery<TResult> Return<TResult>(this ICypherFluentQuery query, Expression<Func<TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, TResult>(this ICypherFluentQuery query, Expression<Func<T1, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, T4, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, T4, T5, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, T4, T5, T6, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, T4, T5, T6, T7, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> Return<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> expression)
        {
            return SharedReturn<TResult>(query, expression);
        }
        #endregion

        #region ReturnDistinct
        internal static ICypherFluentQuery<TResult> SharedReturnDistinct<TResult>(this ICypherFluentQuery query, LambdaExpression expression)
        {
            return SharedProjectionQuery<TResult>(query, expression, "RETURN DISTINCT", isOutputQuery: true);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<TResult>(this ICypherFluentQuery query, Expression<Func<TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, TResult>(this ICypherFluentQuery query, Expression<Func<T1, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, T4, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, T4, T5, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, T4, T5, T6, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, T4, T5, T6, T7, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }

        public static ICypherFluentQuery<TResult> ReturnDistinct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> expression)
        {
            return SharedReturnDistinct<TResult>(query, expression);
        }
        #endregion
    }
}