using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Neo4jClient.Cypher;
using System.Linq.Expressions;
using System.Reflection;
using Neo4jClient.DataAnnotations.Expressions;
using Neo4jClient.DataAnnotations.Serialization;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public class AnnotatedQuery : Annotated, IAnnotatedQuery, IOrderedAnnotatedQuery
    {
        protected static Type VbCompareReplacerType { get; private set; }

        protected static MethodInfo PrivateGenericWithInfo { get; } = typeof(CypherFluentQuery)
            .GetMethod("With", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetGenericMethodDefinition();

        protected static MethodInfo PrivateGenericReturnInfo { get; } = typeof(CypherFluentQuery)
            .GetMethod("Return", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetGenericMethodDefinition();

        protected static MethodInfo PrivateGenericReturnDistinctInfo { get; } = typeof(CypherFluentQuery)
            .GetMethod("ReturnDistinct", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetGenericMethodDefinition();


        public AnnotatedQuery(ICypherFluentQuery query) : this(query, null)
        {
        }

        protected AnnotatedQuery(ICypherFluentQuery query, AnnotatedQuery previous) : base(query)
        {
            if (VbCompareReplacerType == null)
            {
                Assembly neo4jClientAssembly = typeof(ICypherFluentQuery).GetTypeInfo().Assembly;
                VbCompareReplacerType = neo4jClientAssembly.GetType("Neo4jClient.Cypher.VbCompareReplacer");
            }

            if (previous == null)
            {
                Utilities.GetQueryUtilities(query, out var client, out var serializer,
                    out var resolver, out var converter, out var actualSerializer, out var queryWriterGetter);

                Client = client;
                QueryWriterGetter = queryWriterGetter;
                Serializer = actualSerializer;
                Converter = converter;
                Resolver = resolver;
                CamelCaseProperties = (bool)query.GetType().GetField("CamelCaseProperties", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(query);
            }
            else
            {
                Client = previous.Client;
                QueryWriterGetter = previous.QueryWriterGetter;
                Serializer = previous.Serializer;
                Converter = previous.Converter;
                Resolver = previous.Resolver;
                CamelCaseProperties = previous.CamelCaseProperties;
            }
        }


        public IGraphClient Client { get; }

        public bool CamelCaseProperties { get; }

        protected Func<ICypherFluentQuery, QueryWriter> QueryWriterGetter { get; }

        protected QueryWriter QueryWriter { get { return QueryWriterGetter.Invoke(CypherQuery); } }

        protected EntityConverter Converter { get; }

        protected EntityResolver Resolver { get; }

        protected Func<object, string> Serializer { get; }


        protected override string InternalBuild()
        {
            throw new NotImplementedException();
        }

        protected internal AnnotatedQuery Mutate(ICypherFluentQuery query)
        {
            return new AnnotatedQuery(query, this);
        }

        protected internal AnnotatedQuery<TResult> Mutate<TResult>(ICypherFluentQuery<TResult> query)
        {
            return new AnnotatedQuery<TResult>(query, this);
        }

        protected void ReplaceQueryWriterText(ICypherFluentQuery newQuery, string newText)
        {
            var queryWriter = QueryWriterGetter.Invoke(newQuery) as QueryWriter;
            var qwBuilderInfo = queryWriter.GetType().GetField("queryTextBuilder",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var qwBuilder = qwBuilderInfo.GetValue(queryWriter) as StringBuilder;
            qwBuilder.Clear();
            qwBuilder.Append(newText);
        }

        protected LambdaExpression VisitForICypherResultItem(LambdaExpression expression)
        {
            //fish out all the parameterexpression accesses
            var paramAccessStretchVisitor = new ParameterAccessStretchVisitor();
            paramAccessStretchVisitor.Visit(expression);

            Dictionary<Expression, Tuple<string, Expression>> exprToMemberAccess
                = new Dictionary<Expression, Tuple<string, Expression>>();

            //convert the param accesses to a string json access
            foreach (var paramAccess in paramAccessStretchVisitor.ParameterAccesses)
            {
                //we need the expression after "As", which should be index 2 (third element)
                //"As" expression itself should be index 1.
                var paramAccessExprs = Utilities.GetSimpleMemberAccessStretch(paramAccess.Key, out var entityExpr);

                if (paramAccessExprs.Count >= 3)
                {
                    var returnAsMethodCallExpr = paramAccessExprs[1] as MethodCallExpression;

                    if (returnAsMethodCallExpr == null
                        || returnAsMethodCallExpr.Method.Name != "As"
                        || !returnAsMethodCallExpr.Method.IsGenericMethod)
                        continue;

                    var sourceType = returnAsMethodCallExpr.Method.GetGenericArguments().Single();
                    var paramExpr = Expression.Parameter(sourceType, paramAccess.Value.Name);

                    //replace the "As" call expression with our new parameter
                    var newParamAccess = new ReplacerExpressionVisitor
                    (new Dictionary<Expression, Expression>() { { returnAsMethodCallExpr, paramExpr } })
                    .Visit(paramAccess.Key);

                    var propertyName =
                    Utilities.GetFinalPropertyNames(sourceType, Expression.Lambda(newParamAccess, paramExpr),
                    Resolver, Converter, Serializer, makeNamesMemberAccess: true)[0];

                    //we are going to replace the expression with a new one like this:
                    //Return.As<int>("person.age")  where "person" is the parameter, and int is the "age" field type.
                    //from the source code, the expression visitor simply outputs whatever we put as argument to the "As" method of "Return" class.
                    var returnAsMethod = Utilities.GetMethodInfo(() => Return.As<object>(null), paramAccess.Key.Type);
                    var returnAsExprCall = Expression.Call(returnAsMethod, Expression.Constant(propertyName, typeof(string)));

                    exprToMemberAccess[paramAccess.Key] = new Tuple<string, Expression>(propertyName, returnAsExprCall);
                }
            }

            if (exprToMemberAccess.Count > 0)
            {
                expression = new ReplacerExpressionVisitor
                    (exprToMemberAccess.ToDictionary(pair => pair.Key, pair => pair.Value.Item2))
                    .Visit(expression) as LambdaExpression;

            }

            return expression;
        }

        #region Where
        protected string BuildWhereText(LambdaExpression expression)
        {
            var vbCompareReplacerVisitor = Utilities.CreateInstance(VbCompareReplacerType, nonPublic: true) as ExpressionVisitor;
            var expr = vbCompareReplacerVisitor.Visit(expression);

            //fish out all the parameterexpression accesses
            var paramAccessStretchVisitor = new ParameterAccessStretchVisitor();
            paramAccessStretchVisitor.Visit(expr);

            Dictionary<Expression, Tuple<string, Expression>> exprToMemberAccess
                = new Dictionary<Expression, Tuple<string, Expression>>();

            //convert the param accesses to a string json access
            foreach (var paramAccess in paramAccessStretchVisitor.ParameterAccesses)
            {
                var propertyName =
                    Utilities.GetFinalPropertyNames(paramAccess.Value.Type,
                    Expression.Lambda(paramAccess.Key, paramAccess.Value),
                    Resolver, Converter, Serializer, makeNamesMemberAccess: true)[0];

                //we are going to replace the expression with a new one like this:
                //"person.age"._As<int>()  where "person" is the parameter, and int is the "age" field type.
                //from the source code, the where expression visitor will simply ignore the "As" method call.
                //we simply send the property name directly to output then
                var asMethod = Utilities.GetMethodInfo(() => ObjectExtensions._As<object>(null), paramAccess.Key.Type);
                var asExprCall = Expression.Call(asMethod, Expression.Constant(propertyName));

                exprToMemberAccess[paramAccess.Key] = new Tuple<string, Expression>(propertyName, asExprCall);
            }

            Func<object, string> createParameterCallback = QueryWriter.CreateParameter;

            if (exprToMemberAccess.Count > 0)
            {
                expr = new ReplacerExpressionVisitor(exprToMemberAccess.ToDictionary(pair => pair.Key, pair => pair.Value.Item2))
                .Visit(expr);

                //replace the createParameterCallback such that whenever there's a request for parameter,
                //and the value matches one we have in our memberAccess, return the value instead, and not a parameter.
                var actualCallback = createParameterCallback;
                createParameterCallback = val =>
                {
                    if (val is string valStr)
                    {
                        //we only check strings
                        if (exprToMemberAccess.Values.Any(v => v.Item1 == valStr))
                        {
                            //bingo
                            //found it
                            //our work is done.
                            return valStr;
                        }
                    }

                    return actualCallback(val);
                };
            }

            return CypherWhereExpressionBuilder.BuildText
                (expr as LambdaExpression, createParameterCallback, Client.CypherCapabilities, CamelCaseProperties);
        }

        protected internal AnnotatedQuery SharedWhere(LambdaExpression expression)
        {
            var newQuery = CypherQuery.Where(BuildWhereText(expression));
            return Mutate(newQuery);
        }

        protected internal AnnotatedQuery SharedAndWhere(LambdaExpression expression)
        {
            var newQuery = CypherQuery.AndWhere(BuildWhereText(expression));
            return Mutate(newQuery);
        }

        protected internal AnnotatedQuery SharedOrWhere(LambdaExpression expression)
        {
            var newQuery = CypherQuery.OrWhere(BuildWhereText(expression));
            return Mutate(newQuery);
        }
        #endregion

        #region With
        private ICypherFluentQuery<TResult> SharedWithQuery<TResult>(LambdaExpression expression)
        {
            expression = VisitForICypherResultItem(expression);

            //call private with
            var newQuery = PrivateGenericWithInfo
                .MakeGenericMethod(typeof(TResult))
                .Invoke(CypherQuery, new object[] { expression })
                as ICypherFluentQuery<TResult>;

            return newQuery;
        }

        protected internal AnnotatedQuery<TResult> SharedWith<TResult>(LambdaExpression expression, string addedWithText)
        {
            //call private with
            var newQuery = SharedWithQuery<TResult>(expression);

            if (!string.IsNullOrEmpty(addedWithText))
            {
                //prepend the addedtext in case there's a "*"
                var newQueryText = QueryWriterGetter.Invoke(newQuery).ToCypherQuery().QueryText;
                newQueryText = newQueryText.Insert(newQueryText.ToLower().LastIndexOf("with ") + 5, addedWithText.Trim(',') + ", ");
                ReplaceQueryWriterText(newQuery, newQueryText);
            }

            return Mutate(newQuery);
        }
        #endregion

        #region Return
        protected internal AnnotatedQuery<TResult> SharedReturn<TResult>(LambdaExpression expression)
        {
            expression = VisitForICypherResultItem(expression);

            //call private return
            var newQuery = PrivateGenericReturnInfo
                .MakeGenericMethod(typeof(TResult))
                .Invoke(CypherQuery, new object[] { expression })
                as ICypherFluentQuery<TResult>;

            return Mutate(newQuery);
        }

        protected internal AnnotatedQuery<TResult> SharedReturnDistinct<TResult>(LambdaExpression expression)
        {
            expression = VisitForICypherResultItem(expression);

            //call private return distinct
            var newQuery = PrivateGenericReturnDistinctInfo
                .MakeGenericMethod(typeof(TResult))
                .Invoke(CypherQuery, new object[] { expression })
                as ICypherFluentQuery<TResult>;

            return Mutate(newQuery);
        }
        #endregion

        #region OrderBy
        protected internal AnnotatedQuery SharedOrderBy<TResult>
            (LambdaExpression expression, Func<ICypherFluentQuery, string[], IOrderedCypherFluentQuery> clause)
        {
            //get the current query text before any further manipulations.
            var originalQueryText = QueryWriterGetter.Invoke(CypherQuery).ToCypherQuery().QueryText;

            //use with clause to process the icypherresultitem
            var withQuery = SharedWithQuery<TResult>(expression);

            //now get the with clause statement written to the query. we need to extract the property names from it.
            var withQueryText = QueryWriterGetter.Invoke(withQuery).ToCypherQuery().QueryText;
            withQueryText = withQueryText.Remove(0, withQueryText.ToLower().LastIndexOf("with "));
            withQueryText = withQueryText.Trim('\r', '\n');

            //extract the property names
            //to avoid using regex for this, we are going to process just the first of the property names
            var withPropertyName = withQueryText.Remove(0, 5); //removes "WITH "

            //check to see if peradventure, an " AS " made it into the text
            if (withPropertyName.ToLower().IndexOf(" as ") is var asIdx && asIdx >= 0)
            {
                withPropertyName = withPropertyName.Remove(asIdx); //removes any text after the first " AS " of the return statement.
            }

            withPropertyName = withPropertyName.Trim();

            //replace the query text with the original
            ReplaceQueryWriterText(withQuery, originalQueryText);

            //we can now write the order clause with our property name
            var newQuery = clause(withQuery, new string[] { withPropertyName });

            return Mutate(newQuery);
        }

        protected internal AnnotatedQuery SharedOrderBy<TResult>(LambdaExpression expression)
        {
            return SharedOrderBy<TResult>(expression, (q, p) => q.OrderBy(p));
        }

        protected internal AnnotatedQuery SharedOrderByDescending<TResult>(LambdaExpression expression)
        {
            return SharedOrderBy<TResult>(expression, (q, p) => q.OrderByDescending(p));
        }

        protected internal AnnotatedQuery SharedThenBy<TResult>(LambdaExpression expression)
        {
            return SharedOrderBy<TResult>(expression, (q, p) => ((IOrderedCypherFluentQuery)q).ThenBy(p));
        }

        protected internal AnnotatedQuery SharedThenByDescending<TResult>(LambdaExpression expression)
        {
            return SharedOrderBy<TResult>(expression, (q, p) => ((IOrderedCypherFluentQuery)q).ThenByDescending(p));
        }
        #endregion
    }

    public class AnnotatedQuery<TResult> : AnnotatedQuery, IAnnotatedQuery<TResult>
    {
        public AnnotatedQuery(ICypherFluentQuery<TResult> query) : this(query, null)
        {
        }

        protected internal AnnotatedQuery(ICypherFluentQuery<TResult> query, AnnotatedQuery previous) : base(query, previous)
        {
        }
    }
}
