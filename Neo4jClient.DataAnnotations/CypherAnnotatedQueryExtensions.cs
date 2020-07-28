using System;
using System.Linq.Expressions;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Cypher;

namespace Neo4jClient.DataAnnotations //.Cypher
{
    public static class CypherAnnotatedQueryExtensions
    {
        #region CypherQuery

        public static ICypherFluentQuery AsCypherQuery(this IAnnotatedQuery annotatedQuery)
        {
            return annotatedQuery.CypherQuery;
        }

        public static ICypherFluentQuery<TResult> AsCypherQuery<TResult>(this IAnnotatedQuery<TResult> annotatedQuery)
        {
            return (ICypherFluentQuery<TResult>)annotatedQuery.CypherQuery;
        }

        public static IOrderedCypherFluentQuery AsCypherQuery(this IOrderedAnnotatedQuery annotatedQuery)
        {
            return (IOrderedCypherFluentQuery)annotatedQuery.CypherQuery;
        }

        #endregion

        #region Where

        internal static IAnnotatedQuery Where(this IAnnotatedQuery annotatedQuery, LambdaExpression expression)
        {
            return ((AnnotatedQuery)annotatedQuery).SharedWhere(expression);
        }

        internal static IAnnotatedQuery AndWhere(this IAnnotatedQuery annotatedQuery, LambdaExpression expression)
        {
            return ((AnnotatedQuery)annotatedQuery).SharedAndWhere(expression);
        }

        internal static IAnnotatedQuery OrWhere(this IAnnotatedQuery annotatedQuery, LambdaExpression expression)
        {
            return ((AnnotatedQuery)annotatedQuery).SharedOrWhere(expression);
        }


        public static IAnnotatedQuery Where<T1>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3, T4>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3, T4, T5>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3, T4, T5, T6>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3, T4, T5, T6, T7>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3, T4, T5, T6, T7, T8>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression)
        {
            return Where(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3, T4>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3, T4, T5>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3, T4, T5, T6>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3, T4, T5, T6, T7>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression)
        {
            return AndWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3, T4>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3, T4, T5>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3, T4, T5, T6>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3, T4, T5, T6, T7>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            this IAnnotatedQuery annotatedQuery,
            Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression)
        {
            return OrWhere(annotatedQuery, (LambdaExpression)expression);
        }

        #endregion

        #region With

        internal static IAnnotatedQuery<TResult> With<TResult>(this IAnnotatedQuery annotatedQuery,
            LambdaExpression expression, string addedWithText = null)
        {
            return ((AnnotatedQuery)annotatedQuery).SharedWith<TResult>(expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>>
                expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    TResult>>
                expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    TResult>>
                expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression);
        }


        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<TResult>> expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, TResult>> expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression,
            string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>>
                expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, TResult>> expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, TResult>> expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression,
            string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    TResult>>
                expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, TResult>> expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, TResult>> expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression,
            string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    TResult>>
                expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, TResult>> expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        public static IAnnotatedQuery With<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, TResult>> expression, string addedWithText)
        {
            return With<TResult>(annotatedQuery, (LambdaExpression)expression, addedWithText);
        }

        #endregion

        #region Return

        internal static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            LambdaExpression expression)
        {
            return ((AnnotatedQuery)annotatedQuery).SharedReturn<TResult>(expression);
        }

        internal static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            LambdaExpression expression)
        {
            return ((AnnotatedQuery)annotatedQuery).SharedReturnDistinct<TResult>(expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>>
                expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    TResult>>
                expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    TResult>>
                expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> Return<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>>
                expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    TResult>>
                expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                    TResult>>
                expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        public static IAnnotatedQuery<TResult> ReturnDistinct<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem,
                ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        #endregion

        #region OrderBy

        internal static IOrderedAnnotatedQuery OrderBy<TResult>(this IAnnotatedQuery annotatedQuery,
            LambdaExpression expression)
        {
            return ((AnnotatedQuery)annotatedQuery).SharedOrderBy<TResult>(expression);
        }

        internal static IOrderedAnnotatedQuery OrderByDescending<TResult>(this IAnnotatedQuery annotatedQuery,
            LambdaExpression expression)
        {
            return ((AnnotatedQuery)annotatedQuery).SharedOrderByDescending<TResult>(expression);
        }

        internal static IOrderedAnnotatedQuery ThenBy<TResult>(this IAnnotatedQuery annotatedQuery,
            LambdaExpression expression)
        {
            return ((AnnotatedQuery)annotatedQuery).SharedThenBy<TResult>(expression);
        }

        internal static IOrderedAnnotatedQuery ThenByDescending<TResult>(this IAnnotatedQuery annotatedQuery,
            LambdaExpression expression)
        {
            return ((AnnotatedQuery)annotatedQuery).SharedThenByDescending<TResult>(expression);
        }


        /// <summary>
        ///     Orders the query by a single property in ascending other.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="annotatedQuery"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IOrderedAnnotatedQuery OrderBy<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, TResult>> expression)
        {
            return OrderBy<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        /// <summary>
        ///     Orders the query by a single property in decending other.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="annotatedQuery"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IOrderedAnnotatedQuery OrderByDescending<TResult>(this IAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, TResult>> expression)
        {
            return OrderByDescending<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        /// <summary>
        ///     Orders the query by an additional property in ascending other.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="annotatedQuery"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IOrderedAnnotatedQuery ThenBy<TResult>(this IOrderedAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, TResult>> expression)
        {
            return ThenBy<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        /// <summary>
        ///     Orders the query by an additional property in descending other.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="annotatedQuery"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IOrderedAnnotatedQuery ThenByDescending<TResult>(this IOrderedAnnotatedQuery annotatedQuery,
            Expression<Func<ICypherResultItem, TResult>> expression)
        {
            return ThenByDescending<TResult>(annotatedQuery, (LambdaExpression)expression);
        }

        #endregion
    }
}