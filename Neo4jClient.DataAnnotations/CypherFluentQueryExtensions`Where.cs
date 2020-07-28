using System;
using System.Linq.Expressions;
using Neo4jClient.Cypher;

namespace Neo4jClient.DataAnnotations //.Cypher
{
    public static partial class CypherFluentQueryExtensions
    {
        #region AndWhere

        internal static ICypherFluentQuery SharedAndWhere(this ICypherFluentQuery query, LambdaExpression expression)
        {
            return SharedProjectionQuery<object>(query, expression, "AND", false, applyResultFormat: false);
        }

        public static ICypherFluentQuery AnnotatedAndWhere(this ICypherFluentQuery query,
            Expression<Func<object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1>(this ICypherFluentQuery query,
            Expression<Func<T1, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1, T2>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1, T2, T3>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1, T2, T3, T4>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1, T2, T3, T4, T5>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1, T2, T3, T4, T5, T6>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1, T2, T3, T4, T5, T6, T7>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1, T2, T3, T4, T5, T6, T7, T8>(
            this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedAndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery
            AnnotatedAndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
                this ICypherFluentQuery query,
                Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, object>> expression)
        {
            return SharedAndWhere(query, expression);
        }

        public static ICypherFluentQuery
            AnnotatedAndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
                this ICypherFluentQuery query,
                Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, object>>
                    expression)
        {
            return SharedAndWhere(query, expression);
        }

        #endregion

        #region OrWhere

        internal static ICypherFluentQuery SharedOrWhere(this ICypherFluentQuery query, LambdaExpression expression)
        {
            return SharedProjectionQuery<object>(query, expression, "OR", false, applyResultFormat: false);
        }

        public static ICypherFluentQuery AnnotatedOrWhere(this ICypherFluentQuery query,
            Expression<Func<object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1>(this ICypherFluentQuery query,
            Expression<Func<T1, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1, T2>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1, T2, T3>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1, T2, T3, T4>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1, T2, T3, T4, T5>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1, T2, T3, T4, T5, T6>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1, T2, T3, T4, T5, T6, T7>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1, T2, T3, T4, T5, T6, T7, T8>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedOrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery
            AnnotatedOrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
                this ICypherFluentQuery query,
                Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, object>> expression)
        {
            return SharedOrWhere(query, expression);
        }

        public static ICypherFluentQuery
            AnnotatedOrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
                this ICypherFluentQuery query,
                Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, object>>
                    expression)
        {
            return SharedOrWhere(query, expression);
        }

        #endregion

        #region Where

        internal static ICypherFluentQuery SharedWhere(this ICypherFluentQuery query, LambdaExpression expression)
        {
            return SharedProjectionQuery<object>(query, expression, "WHERE", false, applyResultFormat: false);
        }

        public static ICypherFluentQuery AnnotatedWhere(this ICypherFluentQuery query,
            Expression<Func<object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1>(this ICypherFluentQuery query,
            Expression<Func<T1, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1, T2>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1, T2, T3>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1, T2, T3, T4>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1, T2, T3, T4, T5>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1, T2, T3, T4, T5, T6>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1, T2, T3, T4, T5, T6, T7>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1, T2, T3, T4, T5, T6, T7, T8>(this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery AnnotatedWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            this ICypherFluentQuery query,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery
            AnnotatedWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
                this ICypherFluentQuery query,
                Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, object>> expression)
        {
            return SharedWhere(query, expression);
        }

        public static ICypherFluentQuery
            AnnotatedWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
                this ICypherFluentQuery query,
                Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, object>>
                    expression)
        {
            return SharedWhere(query, expression);
        }

        #endregion
    }
}