using System;
using Neo4jClient.DataAnnotations.Utils;
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
                QueryContext = CypherUtilities.GetQueryContext(query);

                CamelCaseProperties = (bool)query.GetType().GetField("CamelCaseProperties", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(query);
                FunctionVisitor = new FunctionExpressionVisitor(QueryContext);
            }
            else
            {
                QueryContext = previous.QueryContext;
                QueryContext.CurrentQueryWriter = QueryWriterGetter(query);
                QueryContext.CurrentBuildStrategy = QueryContext.BuildStrategyGetter(query);

                CamelCaseProperties = previous.CamelCaseProperties;
                var funcsVisitor = previous.FunctionVisitor;
                funcsVisitor.QueryContext = QueryContext;
                FunctionVisitor = funcsVisitor;
            }

            var context = QueryContext?.AnnotationsContext ?? previous?.AnnotationsContext;
            if (context != null)
                AnnotationsContext = context;
        }

        public QueryContext QueryContext { get; }

        public IGraphClient Client => QueryContext?.Client;

        public bool CamelCaseProperties { get; }

        protected Func<ICypherFluentQuery, QueryWriter> QueryWriterGetter => QueryContext.QueryWriterGetter;

        protected QueryWriter QueryWriter { get { return QueryWriterGetter.Invoke(CypherQuery); } }

        protected EntityConverter Converter => QueryContext?.Converter;

        protected EntityResolver Resolver => QueryContext?.Resolver;

        protected Func<object, string> Serializer => QueryContext?.SerializeCallback;

        private FunctionExpressionVisitor _funcVisitor;
        protected FunctionExpressionVisitor FunctionVisitor
        {
            get
            {
                if (_funcVisitor != null)
                {
                    _funcVisitor.QueryContext.CurrentQueryWriter = QueryWriter;
                    _funcVisitor.QueryContext.CurrentBuildStrategy = QueryContext.BuildStrategyGetter(CypherQuery);
                }

                return _funcVisitor;
            }
            private set { _funcVisitor = value; }
        }


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
            var paramAccessStretchVisitor = new ParameterAccessStretchVisitor(EntityService);
            paramAccessStretchVisitor.Visit(expression);

            var replacements = new Dictionary<Expression, Expression>();

            //remove all param.As calls. change them to vars.get calls
            foreach (var paramAccess in paramAccessStretchVisitor.ParameterAccesses)
            {
                //"As" expression itself should be index 1.
                var paramAccessExprs = ExpressionUtilities.GetSimpleMemberAccessStretch(EntityService, paramAccess.Key, out var entityExpr);
                if (paramAccessExprs.Count >= 2)
                {
                    var returnAsMethodCallExpr = paramAccessExprs[1] as MethodCallExpression;

                    if (returnAsMethodCallExpr == null
                        || returnAsMethodCallExpr.Method.Name != "As"
                        || !returnAsMethodCallExpr.Method.IsGenericMethod)
                        continue;

                    var sourceType = returnAsMethodCallExpr.Method.GetGenericArguments().Single();

                    if (replacements.FirstOrDefault(r => r.Value.Type == sourceType
                        && ((r.Value as MethodCallExpression)?.Arguments[0] as ConstantExpression)
                        .Value as string == paramAccess.Value.Name).Value is var varsGetExpr
                        && varsGetExpr == null) //try get existing vars.get call
                    {
                        varsGetExpr = ExpressionUtilities.GetVarsGetExpressionFor(paramAccess.Value.Name, sourceType);
                    }

                    //replace the "As" call expression with our new parameter
                    replacements.Add(returnAsMethodCallExpr, varsGetExpr);
                }
            }

            if (replacements.Count > 0)
            {
                //make replacements
                expression = new ReplacerExpressionVisitor(replacements).Visit(expression) as LambdaExpression;
            }

            return expression;
        }

        protected string BuildICypherResultItemExpression
            (LambdaExpression expression, bool isOutputQuery,
            out CypherResultMode resultMode, out CypherResultFormat resultFormat)
        {
            //expecting:
            //u => u //parameter
            //u => u.Id() //methodcall
            //u => u.As<User>().Name //memberaccess
            //(u,v) => new { u, v } //new anonymous expression
            //(u, v) => new User(){ Name = u.Id() } //member init

            expression = VisitForICypherResultItem(expression);
            QueryContext.CurrentBuildStrategy = QueryContext.CurrentBuildStrategy ?? PropertiesBuildStrategy.WithParams;
            var result = ExpressionUtilities.BuildProjectionQueryExpression(expression, QueryContext, FunctionVisitor,
                isOutputQuery, out resultMode, out resultFormat);

            return result;
        }

        #region Where
        protected string BuildWhereText(LambdaExpression expression)
        {
            var vbCompareReplacerVisitor = Utils.Utilities.CreateInstance(VbCompareReplacerType, nonPublic: true) as ExpressionVisitor;
            var expr = vbCompareReplacerVisitor.Visit(expression) as LambdaExpression;

            //visit the expression
            var visitor = FunctionVisitor;
            visitor.Clear();
            var newBodyExpr = visitor.Visit(expr.Body);
            return "(" + visitor.Builder.ToString() + ")";
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
            var text = "WITH " + BuildICypherResultItemExpression(expression, false, out var mode, out var format);

            return CypherQuery.Mutate<TResult>(w =>
            {
                w.ResultMode = mode;
                w.AppendClause(text);
            });
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
            var text = "RETURN " + BuildICypherResultItemExpression(expression, true, out var mode, out var format);

            var newQuery = CypherQuery.Mutate<TResult>(w =>
            {
                w.ResultMode = mode;
                w.ResultFormat = format;
                w.AppendClause(text);
            });

            return Mutate(newQuery);
        }

        protected internal AnnotatedQuery<TResult> SharedReturnDistinct<TResult>(LambdaExpression expression)
        {
            var text = "RETURN DISTINCT " + BuildICypherResultItemExpression(expression, true, out var mode, out var format);

            var newQuery = CypherQuery.Mutate<TResult>(w =>
            {
                w.ResultMode = mode;
                w.ResultFormat = format;
                w.AppendClause(text);
            });

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
