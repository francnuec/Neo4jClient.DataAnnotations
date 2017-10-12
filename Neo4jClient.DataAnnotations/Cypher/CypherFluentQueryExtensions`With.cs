using System;
using System.Linq.Expressions;
using System.Linq;
using Neo4jClient.Cypher;
using System.Reflection;
using Neo4jClient.DataAnnotations.Expressions;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public static partial class CypherFluentQueryExtensions
    {
        internal static ICypherFluentQuery<TResult> SharedProjectionQuery<TResult>
            (this ICypherFluentQuery query, LambdaExpression expression, string clause, bool applyResultMode = true, bool applyResultFormat = true)
        {
            var queryUtilities = Utilities.GetQueryUtilities(query);
            queryUtilities.CurrentBuildStrategy = queryUtilities.CurrentBuildStrategy ?? PropertiesBuildStrategy.WithParams;
            var funcVisitor = new FunctionExpressionVisitor(queryUtilities);
            var result = Utilities.BuildProjectionQueryExpression(expression, queryUtilities, funcVisitor, out var mode, out var format);

            return Mutate<TResult>(query, w =>
            {
                if (applyResultMode)
                    w.ResultMode = mode;

                if (applyResultFormat)
                    w.ResultFormat = format;

                w.AppendClause(clause + " " + result);
            });
        }

        #region With
        internal static ICypherFluentQuery SharedWith(this ICypherFluentQuery query, LambdaExpression expression)
        {
            return SharedProjectionQuery<object>(query, expression, "WITH", applyResultFormat: false);
        }

        public static ICypherFluentQuery With(this ICypherFluentQuery query, Expression<Func<object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1>(this ICypherFluentQuery query, Expression<Func<T1, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2>(this ICypherFluentQuery query, Expression<Func<T1, T2, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3, T4>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3, T4, T5>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3, T4, T5, T6>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3, T4, T5, T6, T7>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3, T4, T5, T6, T7, T8>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, object>> expression)
        {
            return SharedWith(query, expression);
        }

        public static ICypherFluentQuery With<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this ICypherFluentQuery query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, object>> expression)
        {
            return SharedWith(query, expression);
        }
        #endregion
    }
}