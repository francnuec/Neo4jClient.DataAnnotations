using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Linq.Expressions;
using System.Linq;
using Neo4jClient.Cypher;
using System.Reflection;
using Neo4jClient.DataAnnotations.Expressions;
using Neo4jClient.DataAnnotations.Serialization;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public static partial class CypherFluentQueryExtensions
    {
        static MethodInfo mutateMethodInfo = null;
        static MethodInfo mutateGenericMethodInfo = null;

        #region AnnotationsContext
        public static IAnnotationsContext GetAnnotationsContext(this ICypherFluentQuery query)
        {
            var client = (query as IAttachedReference)?.Client;
            IAnnotationsContext context = client?.GetAnnotationsContext();

            return context ?? throw new InvalidOperationException($"Could not get an instance of {nameof(IAnnotationsContext)} from query.");
        }
        #endregion

        #region AnnotatedQuery
        /// <summary>
        /// <see cref="IAnnotatedQuery"/> abstracts methods that re-implement Neo4jClient methods using the same signatures in order to take advantage of annotations features like ComplexTypes.
        /// Separating these methods this way ensures that we avoid collisions.
        /// E.g. client.Cypher.AsAnnotatedQuery().Where((MovieNode movie) =&gt; movie.Year == 2017)
        /// See Tests for examples. Call <see cref="AnnotatedQueryExtensions.AsCypherQuery(IAnnotatedQuery)"/> to return to normal Cypher methods.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IAnnotatedQuery AsAnnotatedQuery(this ICypherFluentQuery query)
        {
            return new AnnotatedQuery(query);
        }
        /// <summary>
        /// <see cref="IOrderedAnnotatedQuery"/> abstracts methods that re-implement Neo4jClient methods using the same signatures in order to take advantage of annotations features like ComplexTypes.
        /// Separating these methods this way ensures that we avoid collisions.
        /// E.g. client.Cypher.AsAnnotatedQuery().Where((MovieNode movie) =&gt; movie.Year == 2017)
        /// See Tests for examples. Call <see cref="AnnotatedQueryExtensions.AsCypherQuery(IOrderedAnnotatedQuery)"/> to return to normal Cypher methods.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IOrderedAnnotatedQuery AsOrderedAnnotatedQuery(this IOrderedCypherFluentQuery query)
        {
            return new AnnotatedQuery(query);
        }
        #endregion

        #region Clause
        /// <summary>
        /// Adds a cypher clause to the query. 
        /// NOTE: ENSURE YOU HAVE A UNIQUE USE CASE BEFORE USING HERE.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="clauseText"></param>
        /// <returns></returns>
        public static ICypherFluentQuery Clause(this ICypherFluentQuery query, string clauseText)
        {
            return Mutate(query, w => w.AppendClause(clauseText));
        }

        /// <summary>
        /// Adds a cypher clause to the query. 
        /// NOTE: ENSURE YOU HAVE A UNIQUE USE CASE BEFORE USING HERE.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="clauseText"></param>
        /// <returns></returns>
        public static ICypherFluentQuery<TResult> Clause<TResult>(this ICypherFluentQuery query, string clauseText)
        {
            return Mutate<TResult>(query, w => w.AppendClause(clauseText));
        }

        /// <summary>
        /// Adds an ordered cypher clause to the query. 
        /// NOTE: ENSURE YOU HAVE A UNIQUE USE CASE BEFORE USING HERE.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="orderedClauseText"></param>
        /// <returns></returns>
        public static IOrderedCypherFluentQuery OrderedClause(this ICypherFluentQuery query, string orderedClauseText)
        {
            return Clause(query, orderedClauseText) as IOrderedCypherFluentQuery;
        }

        /// <summary>
        /// Adds an ordered cypher clause to the query. 
        /// NOTE: ENSURE YOU HAVE A UNIQUE USE CASE BEFORE USING HERE.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="orderedClauseText"></param>
        /// <returns></returns>
        public static IOrderedCypherFluentQuery<TResult> OrderedClause<TResult>(this ICypherFluentQuery query, string orderedClauseText)
        {
            return Clause<TResult>(query, orderedClauseText) as IOrderedCypherFluentQuery<TResult>;
        }



        /// <summary>
        /// Mutates the <see cref="ICypherFluentQuery"/> after modifying some <see cref="QueryWriter"/> parameters.
        /// NOTE: ENSURE YOU HAVE A UNIQUE USE CASE BEFORE USING HERE.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        internal static ICypherFluentQuery Mutate(this ICypherFluentQuery query, Action<QueryWriter> callback)
        {
            var mutateMethod = mutateMethodInfo ?? (mutateMethodInfo = typeof(CypherFluentQuery)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name == "Mutate" && !m.IsGenericMethod)
                .First());

            return mutateMethod.Invoke(query, new object[] { callback }) as ICypherFluentQuery;
        }

        /// <summary>
        /// Mutates the <see cref="ICypherFluentQuery"/> after modifying some <see cref="QueryWriter"/> parameters.
        /// NOTE: ENSURE YOU HAVE A UNIQUE USE CASE BEFORE USING HERE.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        internal static ICypherFluentQuery<TResult> Mutate<TResult>(this ICypherFluentQuery query, Action<QueryWriter> callback)
        {
            var mutateMethod = mutateGenericMethodInfo ?? (mutateGenericMethodInfo = typeof(CypherFluentQuery)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name == "Mutate" && m.IsGenericMethod)
                .First());

            return mutateMethod.MakeGenericMethod(typeof(TResult)).Invoke(query, new object[] { callback }) as ICypherFluentQuery<TResult>;
        }

        /// <summary>
        /// Mutates the <see cref="ICypherFluentQuery"/> into an <see cref="IOrderedCypherFluentQuery"/> after modifying some <see cref="QueryWriter"/> parameters.
        /// NOTE: ENSURE YOU HAVE A UNIQUE USE CASE BEFORE USING HERE.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IOrderedCypherFluentQuery MutateOrdered(this ICypherFluentQuery query, Action<QueryWriter> callback)
        {
            return Mutate(query, callback) as IOrderedCypherFluentQuery;
        }

        /// <summary>
        /// Mutates the <see cref="ICypherFluentQuery"/> into an <see cref="IOrderedCypherFluentQuery"/> after modifying some <see cref="QueryWriter"/> parameters.
        /// NOTE: ENSURE YOU HAVE A UNIQUE USE CASE BEFORE USING HERE.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IOrderedCypherFluentQuery<TResult> MutateOrdered<TResult>(this ICypherFluentQuery query, Action<QueryWriter> callback)
        {
            return MutateOrdered<TResult>(query, callback) as IOrderedCypherFluentQuery<TResult>;
        }
        #endregion

        #region WithPattern
        /// <summary>
        /// Builds Neo4j pattern with <see cref="PropertiesBuildStrategy.WithParamsForValues"/> as default strategy. 
        /// If multiple patterns are described, they are concatenated with a comma.
        /// E.g., Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: path=(a), (b)--&gt;(c).
        /// This method allows you to safely chain calls on an existing <see cref="ICypherFluentQuery"/> object.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pattern">Pattern generated using the specified strategy and existing query object.</param>
        /// <param name="patternDescriptions"></param>
        /// <returns></returns>
        public static ICypherFluentQuery WithPattern(this ICypherFluentQuery query, out string pattern,
            params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return WithPattern(query, out pattern, PropertiesBuildStrategy.WithParamsForValues, patternDescriptions);
        }

        /// <summary>
        /// Builds Neo4j pattern using the specified <see cref="PropertiesBuildStrategy"/>. If multiple patterns are described, they are concatenated with a comma.
        /// E.g., Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: path=(a), (b)--&gt;(c).
        /// This method allows you to safely chain calls on an existing <see cref="ICypherFluentQuery"/> object.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pattern">Pattern generated using the specified strategy and existing query object.</param>
        /// <param name="defaultBuildStrategy"><see cref="PropertiesBuildStrategy"/> option to use.</param>
        /// <param name="patternDescriptions"></param>
        /// <returns></returns>
        internal static ICypherFluentQuery WithPattern(this ICypherFluentQuery query, out string pattern,
            PropertiesBuildStrategy defaultBuildStrategy,
            params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            pattern = CypherUtilities.BuildPaths(ref query,
                patternDescriptions ?? throw new ArgumentNullException(nameof(patternDescriptions)),
                query.GetBuildStrategy(defaultBuildStrategy));

            return query;
        }
        #endregion

        #region WithExpression
        /// <summary>
        /// Gets the specified variable expression as they would be used in Neo4j.
        /// E.g., query.WithExpression((Person person) =&gt; person.Address.City, out var values), should get you an expression like: person.address_city.
        /// A complex expression like query.WithExpression((Person person) =&gt; person.Address.City.ToString().Length, out var values), gets you: size(toString(person.addres_city)).
        /// For multiple expressions of same type, query.WithExpression((Person person) =&gt; new { person.Name.ToLower(), person.Address.City }, out var values).
        /// Camel casing is assumed for the examples. The complex expressions are mostly limited to Neo4j string functions for now.
        /// This method allows you to safely chain calls on an existing <see cref="ICypherFluentQuery"/> object.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="expressions"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static ICypherFluentQuery WithExpression<T>(this ICypherFluentQuery query, 
            Expression<Func<T, object>> expressions, out string[] values)
        {
            return WithExpression(query, expressions, out values, null);
        }

        /// <summary>
        /// Gets the specified variable expression as they would be used in Neo4j.
        /// E.g., query.WithExpression((Person person) =&gt; person.Address.City, out var values), should get you an expression like: person.address_city.
        /// A complex expression like query.WithExpression((Person person) =&gt; person.Address.City.ToString().Length, out var values), gets you: size(toString(person.addres_city)).
        /// For multiple expressions of same type, query.WithExpression((Person person) =&gt; new { person.Name.ToLower(), person.Address.City }, out var values).
        /// Camel casing is assumed for the examples. The complex expressions are mostly limited to Neo4j string functions for now.
        /// This method allows you to safely chain calls on an existing <see cref="ICypherFluentQuery"/> object.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="expressions"></param>
        /// <param name="values"></param>
        /// <param name="variable">Overrides the parameter used in the lambda. This is useful if the actual variable is only known at runtime (dynamic).</param>
        /// <returns></returns>
        public static ICypherFluentQuery WithExpression<T>(this ICypherFluentQuery query,
            Expression<Func<T, object>> expressions, out string[] values, string variable)
        {
            return WithExpression(query, expressions, out values, isMemberAccess: true, variable: variable);
        }

        /// <summary>
        /// Gets the specified variable expression as they would be used in Neo4j.
        /// E.g., query.WithExpression((Person person) =&gt; person.Address.City, out var values), should get you an expression like: person.address_city.
        /// A complex expression like query.WithExpression((Person person) =&gt; person.Address.City.ToString().Length, out var values), gets you: size(toString(person.addres_city)).
        /// For multiple expressions of same type, query.WithExpression((Person person) =&gt; new { person.Name.ToLower(), person.Address.City }, out var values).
        /// Camel casing is assumed for the examples. The complex expressions are mostly limited to Neo4j string functions for now.
        /// This method allows you to safely chain calls on an existing <see cref="ICypherFluentQuery"/> object.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="expressions"></param>
        /// <param name="values"></param>
        /// <param name="isMemberAccess">If false, the variable is omitted in the returned names.
        /// E.g., name, address_city. Default is true.</param>
        /// <returns></returns>
        public static ICypherFluentQuery WithExpression<T>(this ICypherFluentQuery query,
            Expression<Func<T, object>> expressions, out string[] values, bool isMemberAccess)
        {
            return WithExpression(query, expressions, out values, isMemberAccess, null);
        }

        /// <summary>
        /// Gets the specified variable expression as they would be used in Neo4j.
        /// E.g., query.WithExpression((Person person) =&gt; person.Address.City, out var values), should get you an expression like: person.address_city.
        /// A complex expression like query.WithExpression((Person person) =&gt; person.Address.City.ToString().Length, out var values), gets you: size(toString(person.addres_city)).
        /// For multiple expressions of same type, query.WithExpression((Person person) =&gt; new { person.Name.ToLower(), person.Address.City }, out var values).
        /// Camel casing is assumed for the examples. The complex expressions are mostly limited to Neo4j string functions for now.
        /// This method allows you to safely chain calls on an existing <see cref="ICypherFluentQuery"/> object.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="expressions"></param>
        /// <param name="values"></param>
        /// <param name="isMemberAccess">If false, the parameter is omitted in the returned expressions.
        /// That is, instead of "person.name", you get just "name". Default is true.</param>
        /// <param name="variable">Overrides the parameter used in the lambda. This is useful if the actual variable is only known at runtime (dynamic).</param>
        /// <returns></returns>
        public static ICypherFluentQuery WithExpression<T>(this ICypherFluentQuery query,
            Expression<Func<T, object>> expressions, out string[] values, bool isMemberAccess, string variable)
        {
            return WithExpression(query, expressions, out values, isMemberAccess, variable, PropertiesBuildStrategy.WithParams);
        }

        /// <summary>
        /// Gets the specified variable expression as they would be used in Neo4j.
        /// E.g., query.WithExpression((Person person) =&gt; person.Address.City, out var values), should get you an expression like: person.address_city.
        /// A complex expression like query.WithExpression((Person person) =&gt; person.Address.City.ToString().Length, out var values), gets you: size(toString(person.addres_city)).
        /// For multiple expressions of same type, query.WithExpression((Person person) =&gt; new { person.Name.ToLower(), person.Address.City }, out var values).
        /// Camel casing is assumed for the examples. The complex expressions are mostly limited to Neo4j string functions for now.
        /// This method allows you to safely chain calls on an existing <see cref="ICypherFluentQuery"/> object.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="expressions"></param>
        /// <param name="values"></param>
        /// <param name="isMemberAccess">If false, the parameter is omitted in the returned expressions.
        /// That is, instead of "person.name", you get just "name". Default is true.</param>
        /// <param name="variable">Overrides the parameter used in the lambda. This is useful if the actual variable is only known at runtime (dynamic).</param>
        /// <param name="defaultBuildStrategy">Set a build strategy in the event that it unexpectedly encounters a constant expression.</param>
        /// <returns></returns>
        internal static ICypherFluentQuery WithExpression<T>(this ICypherFluentQuery query,
            Expression<Func<T, object>> expressions, out string[] values, bool isMemberAccess, 
            string variable, PropertiesBuildStrategy defaultBuildStrategy)
        {
            var queryContext = CypherUtilities.GetQueryContext(query);

            queryContext.CurrentBuildStrategy = 
                queryContext.CurrentBuildStrategy ?? defaultBuildStrategy;

            values = ExpressionUtilities.GetVariableExpressions(expressions, queryContext, 
                isMemberAccess: isMemberAccess, variable: variable);

            return query;
        }
        #endregion

        #region UsingBuildStrategy
        /// <summary>
        /// Assigns a specific <see cref="PropertiesBuildStrategy"/> value to be used with all clause/statement calls.
        /// This value is set until you change it again. 
        /// Pass null to this method to reset it and use the default build strategy for each clause.
        /// E.g., query.UsingBuildStrategy(PropertiesBuildStrategy.WithParams).Match(...).
        /// The build strategy used after this call is now <see cref="PropertiesBuildStrategy.WithParams"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="buildStrategy"></param>
        /// <returns></returns>
        public static ICypherFluentQuery UsingBuildStrategy(this ICypherFluentQuery query, PropertiesBuildStrategy? buildStrategy)
        {
            return query.WithParam(Defaults.QueryBuildStrategyKey, buildStrategy);
        }
        
        internal static PropertiesBuildStrategy? GetBuildStrategy(this ICypherFluentQuery query)
        {
            query.Query.QueryParameters.TryGetValue(Defaults.QueryBuildStrategyKey, out var strategy);
            return strategy as PropertiesBuildStrategy?;
        }

        internal static PropertiesBuildStrategy GetBuildStrategy(this ICypherFluentQuery query, PropertiesBuildStrategy defaultBuildStrategy)
        {
            return GetBuildStrategy(query) ?? defaultBuildStrategy;
        }
        #endregion

        #region Match
        /// <summary>
        /// Generates a cypher MATCH statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: MATCH path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery Match(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return WithPattern(query, out var pattern, PropertiesBuildStrategy.WithParamsForValues, patternDescriptions).Match(pattern);
        }
        #endregion

        #region OptionalMatch
        /// <summary>
        /// Generates a cypher OPTIONAL MATCH statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: OPTIONAL MATCH path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery OptionalMatch(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return WithPattern(query, out var pattern, PropertiesBuildStrategy.WithParamsForValues, patternDescriptions).OptionalMatch(pattern);
        }
        #endregion

        #region Merge
        /// <summary>
        /// Generates a cypher MERGE statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: MERGE path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery Merge(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return WithPattern(query, out var pattern, PropertiesBuildStrategy.WithParamsForValues, patternDescriptions).Merge(pattern);
        }
        #endregion

        #region Create
        /// <summary>
        /// Generates a cypher CREATE statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: CREATE path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery Create(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return WithPattern(query, out var pattern, PropertiesBuildStrategy.WithParams, patternDescriptions).Create(pattern);
        }
        #endregion

        #region CreateUnique
        /// <summary>
        /// Generates a cypher CREATE UNIQUE statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: CREATE UNIQUE path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery CreateUnique(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return WithPattern(query, out var pattern, PropertiesBuildStrategy.WithParams, patternDescriptions).CreateUnique(pattern);
        }
        #endregion

        #region Set
        /// <summary>
        /// Generates a cypher SET statement from the properties.
        /// E.g. () =&gt; new Movie { title = "Grey's Anatomy", year = 2017 }, should generate: SET movie = { title: "Grey's Anatomy", year = 2017 }, where movie is the variable.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <returns></returns>
        public static ICypherFluentQuery Set(this ICypherFluentQuery query, string variable, Expression<Func<object>> properties)
        {
            return Set(query, variable, properties, out var setParam);
        }

        /// <summary>
        /// Generates a cypher SET statement from the properties.
        /// E.g. () =&gt; new Movie { title = "Grey's Anatomy", year = 2017 }, should generate: SET movie = { title: "Grey's Anatomy", year = 2017 }, where movie is the variable.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <param name="setParameter">The parameter used in the <see cref="ICypherFluentQuery.WithParam(string, object)"/> call.</param>
        /// <returns></returns>
        public static ICypherFluentQuery Set(this ICypherFluentQuery query, string variable, Expression<Func<object>> properties, out string setParameter)
        {
            return SharedSet(query, variable, properties, PropertiesBuildStrategy.WithParams, out setParameter, add: false);
        }


        /// <summary>
        /// Generates a cypher SET statement from the predicate.
        /// E.g. movie =&gt; movie.title = "Grey's Anatomy" &amp;&amp; movie.year = 2017, should generate: SET movie.title = "Grey's Anatomy", movie.year = 2017.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static ICypherFluentQuery Set<T>(this ICypherFluentQuery query, Expression<Func<T, bool>> predicate)
        {
            return Set(query, predicate, null);
        }

        /// <summary>
        /// Generates a cypher SET statement from the predicate.
        /// E.g. movie =&gt; movie.title = "Grey's Anatomy" &amp;&amp; movie.year = 2017, should generate: SET movie.title = "Grey's Anatomy", movie.year = 2017.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <param name="variable">Overrides the parameter used in the predicate lambda. This is useful if the actual variable is only known at runtime (dynamic).</param>
        /// <returns></returns>
        public static ICypherFluentQuery Set<T>(this ICypherFluentQuery query,
            Expression<Func<T, bool>> predicate, string variable)
        {
            return Set(query, predicate, out var setParam, variable);
        }

        /// <summary>
        /// Generates a cypher SET statement from the predicate.
        /// E.g. movie =&gt; movie.title = "Grey's Anatomy" &amp;&amp; movie.year = 2017, should generate: SET movie.title = "Grey's Anatomy", movie.year = 2017.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <param name="setParameter">The parameter used in the <see cref="ICypherFluentQuery.WithParam(string, object)"/> call.</param>
        /// <returns></returns>
        public static ICypherFluentQuery Set<T>(this ICypherFluentQuery query,
            Expression<Func<T, bool>> predicate, out string setParameter)
        {
            return Set(query, predicate, out setParameter, null);
        }

        /// <summary>
        /// Generates a cypher SET statement from the predicate.
        /// E.g. movie =&gt; movie.title = "Grey's Anatomy" &amp;&amp; movie.year = 2017, should generate: SET movie.title = "Grey's Anatomy", movie.year = 2017.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <param name="setParameter">The parameter used in the <see cref="ICypherFluentQuery.WithParam(string, object)"/> call.</param>
        /// <param name="variable">Overrides the parameter used in the predicate lambda. This is useful if the actual variable is only known at runtime (dynamic).</param>
        /// <returns></returns>
        public static ICypherFluentQuery Set<T>(this ICypherFluentQuery query,
            Expression<Func<T, bool>> predicate, out string setParameter, string variable)
        {
            return SharedSet(query, predicate, PropertiesBuildStrategy.WithParamsForValues, out setParameter, variable);
        }


        /// <summary>
        /// Generates a cypher SET statement from the predicate using the "+=" operator.
        /// E.g. movie =&gt; movie.title = "Grey's Anatomy" &amp;&amp; movie.year = 2017, should generate: SET movie += { title: "Grey's Anatomy", year = 2017 }.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static ICypherFluentQuery SetAdd<T>(this ICypherFluentQuery query, Expression<Func<T, bool>> predicate)
        {
            return SetAdd(query, predicate, null);
        }

        /// <summary>
        /// Generates a cypher SET statement from the predicate using the "+=" operator.
        /// E.g. movie =&gt; movie.title = "Grey's Anatomy" &amp;&amp; movie.year = 2017, should generate: SET movie += { title: "Grey's Anatomy", year = 2017 }.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <param name="variable">Overrides the parameter used in the predicate lambda. This is useful if the actual variable is only known at runtime (dynamic).</param>
        /// <returns></returns>
        public static ICypherFluentQuery SetAdd<T>(this ICypherFluentQuery query,
            Expression<Func<T, bool>> predicate, string variable)
        {
            return SetAdd(query, predicate, out var setParam, variable);
        }

        /// <summary>
        /// Generates a cypher SET statement from the predicate using the "+=" operator.
        /// E.g. movie =&gt; movie.title = "Grey's Anatomy" &amp;&amp; movie.year = 2017, should generate: SET movie += { title: "Grey's Anatomy", year = 2017 }.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <param name="setParameter">The parameter used in the <see cref="ICypherFluentQuery.WithParam(string, object)"/> call.</param>
        /// <returns></returns>
        public static ICypherFluentQuery SetAdd<T>(this ICypherFluentQuery query,
            Expression<Func<T, bool>> predicate, out string setParameter)
        {
            return SetAdd(query, predicate, out setParameter, null);
        }

        /// <summary>
        /// Generates a cypher SET statement from the predicate using the "+=" operator.
        /// E.g. movie =&gt; movie.title = "Grey's Anatomy" &amp;&amp; movie.year = 2017, should generate: SET movie += { title: "Grey's Anatomy", year = 2017 }.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <param name="setParameter">The parameter used in the <see cref="ICypherFluentQuery.WithParam(string, object)"/> call.</param>
        /// <param name="variable">Overrides the parameter used in the predicate lambda. This is useful if the actual variable is only known at runtime (dynamic).</param>
        /// <returns></returns>
        public static ICypherFluentQuery SetAdd<T>(this ICypherFluentQuery query,
            Expression<Func<T, bool>> predicate, out string setParameter, string variable)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var propertiesExpression = CypherUtilities.GetConstraintsAsPropertiesLambda(predicate, typeof(T));

            return SharedSet(query, variable ?? predicate.Parameters[0].Name, propertiesExpression, PropertiesBuildStrategy.WithParams, out setParameter, add: true);
        }



        /// <summary>
        /// Generates a cypher SET statement from the properties.
        /// E.g. () =&gt; new Movie { title = "Grey's Anatomy", year = 2017 }, should generate: SET movie = { title: "Grey's Anatomy", year = 2017 }, where movie is the variable.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <param name="add">If this is set to true, the SET statement would use the '+=' operator and not the '=' operator.</param>
        /// <param name="setParameter">The parameter used in the <see cref="ICypherFluentQuery.WithParam(string, object)"/> call.</param>
        /// <returns></returns>
        internal static ICypherFluentQuery SharedSet(this ICypherFluentQuery query, string variable,
            LambdaExpression properties, PropertiesBuildStrategy defaultBuildStrategy, out string setParameter, bool add)
        {
            var queryContext = CypherUtilities.GetQueryContext(query);

            var buildStrategy = queryContext.CurrentBuildStrategy ?? defaultBuildStrategy;

            var serializedValue = CypherUtilities.BuildFinalProperties
                (queryContext, $"{variable}_set", properties,
                ref buildStrategy, out setParameter, out var newProperties,
                out var finalPropsHasFunctions, separator: ": ",
                useVariableAsParameter: false,
                wrapValueInJsonObjectNotation: true);

            return query.Set($"{variable} {(add ? "+=" : "=")} {serializedValue ?? ""}");
        }

        /// <summary>
        /// Generates a cypher SET statement from the predicate.
        /// E.g. movie =&gt; movie.title = "Grey's Anatomy" &amp;&amp; movie.year = 2017, should generate: SET movie.title = "Grey's Anatomy", movie.year = 2017.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <param name="setParameter">The parameter used in the <see cref="ICypherFluentQuery.WithParam(string, object)"/> call.</param>
        /// <param name="variable">Overrides the parameter used in the predicate lambda. This is useful if the actual variable is only known at runtime (dynamic).</param>
        /// <returns></returns>
        internal static ICypherFluentQuery SharedSet<T>(this ICypherFluentQuery query,
            Expression<Func<T, bool>> predicate, PropertiesBuildStrategy defaultBuildStrategy,
            out string setParameter, string variable)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var queryContext = CypherUtilities.GetQueryContext(query);

            var buildStrategy = queryContext.CurrentBuildStrategy ?? defaultBuildStrategy;

            if (!string.IsNullOrWhiteSpace(variable))
            {
                //replace the variable
                var newParamExpr = Expression.Parameter(typeof(T), variable);

                var replacer = new ReplacerExpressionVisitor(new System.Collections.Generic.Dictionary<Expression, Expression>()
                {
                    { predicate.Parameters.First(),  newParamExpr }
                });

                predicate = replacer.Visit(predicate) as Expression<Func<T, bool>>;
            }
            else
            {
                variable = predicate.Parameters[0].Name;
            }

            var properties = CypherUtilities.GetConstraintsAsPropertiesLambda(predicate, typeof(T));

            var value = CypherUtilities.BuildFinalProperties
                (queryContext, $"{variable}_set", properties,
                ref buildStrategy, out setParameter, out var newProperties,
                out var finalPropsHasFunctions, separator: " = ",
                useVariableAsParameter: false,
                wrapValueInJsonObjectNotation: false);

            //var finalProperties = CypherUtilities.GetFinalProperties(CypherUtilities.GetConstraintsAsPropertiesLambda(predicate, typeof(T)),
            //    queryContext, out var hasFunctions);

            //var setParam = Utils.Utilities.GetRandomVariableFor($"{variable}_set");
            //setParameter = setParam;

            //buildStrategy = hasFunctions && buildStrategy == PropertiesBuildStrategy.WithParams ? 
            //    PropertiesBuildStrategy.WithParamsForValues : buildStrategy;

            //string value = null;

            //switch (buildStrategy)
            //{
            //    case PropertiesBuildStrategy.WithParams:
            //    case PropertiesBuildStrategy.WithParamsForValues:
            //        {
            //            //in this type of SET statement, both WithParams, and WithParamsForValues are the same
            //            value = CypherUtilities.BuildWithParamsForValues(finalProperties, queryContext.SerializeCallback,
            //                    getKey: (propertyName) => $"{variable}.{propertyName}", separator: " = ",
            //                    getValue: (propertyName) => $"${setParam}.{propertyName}",
            //                    hasRaw: out var hasRaw, newFinalProperties: out var newFinalProperties);

            //            var _finalProperties = newFinalProperties ?? finalProperties;

            //            query = query.WithParam(setParam, _finalProperties);
            //            break;
            //        }
            //    case PropertiesBuildStrategy.NoParams:
            //        {
            //            value = finalProperties.Properties()
            //                    .Select(jp => $"{variable}.{jp.Name} = {queryContext.SerializeCallback(jp.Value)}")
            //                    .Aggregate((first, second) => $"{first}, {second}");
            //            break;
            //        }
            //}

            return query.Set(value);
        }
        #endregion

        #region OrderBy
        /// <summary>
        /// Orders the query by a property in ascending order.
        /// E.g., query.OrderBy((Actor actor) => actor.Name) generates the cypher: ORDER BY actor.name.
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedCypherFluentQuery OrderBy<T>(this ICypherFluentQuery query, Expression<Func<T, object>> property)
        {
            return SharedOrderBy(query, (q, p) => q.OrderBy(p), property);
        }

        /// <summary>
        /// Orders the query by a property in descending order.
        /// E.g., query.OrderByDescending((Actor actor) => actor.Name) generates the cypher: ORDER BY actor.name DESC.
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedCypherFluentQuery OrderByDescending<T>(this ICypherFluentQuery query, Expression<Func<T, object>> property)
        {
            return SharedOrderBy(query, (q, p) => q.OrderByDescending(p), property);
        }

        /// <summary>
        /// Orders the query by an additional property in ascending order.
        /// E.g., query.OrderByDescending((Actor actor) => actor.Name).ThenBy((Actor actor) => actor.Age) generates the cypher: ORDER BY actor.name DESC, actor.age.
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedCypherFluentQuery ThenBy<T>(this IOrderedCypherFluentQuery query, Expression<Func<T, object>> property)
        {
            return SharedOrderBy(query, (q, p) => ((IOrderedCypherFluentQuery)q).ThenBy(p), property);
        }

        /// <summary>
        /// Orders the query by an additional property in descending order.
        /// E.g., query.OrderBy((Actor actor) => actor.Name).ThenByDescending((Actor actor) => actor.Age) generates the cypher: ORDER BY actor.name, actor.age DESC.
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedCypherFluentQuery ThenByDescending<T>(this IOrderedCypherFluentQuery query,
            Expression<Func<T, object>> property)
        {
            return SharedOrderBy(query, (q, p) => ((IOrderedCypherFluentQuery)q).ThenByDescending(p), property);
        }


        internal static IOrderedCypherFluentQuery SharedOrderBy<T>
            (this ICypherFluentQuery query, Func<ICypherFluentQuery, string[], IOrderedCypherFluentQuery> clause, Expression<Func<T, object>> properties)
        {
            query = query.WithExpression(properties, out var propNames, isMemberAccess: true);

            return clause(query, propNames);
        }
        #endregion

        #region Index
        /// <summary>
        /// Creates a single-property or composite index on properties of a label.
        /// E.g., query.CreateIndex((Actor actor) => actor.Name) should generate the cypher statement: CREATE INDEX ON :Actor(name). This is for a single-property index.
        /// Likewise, for a composite index, query.CreateIndex((Actor actor) => new { actor.Name, actor.Born }) should generate the cypher statement: CREATE INDEX ON :Actor(name, born).
        /// The examples assume camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static ICypherFluentQuery CreateIndex<T>(this ICypherFluentQuery query, Expression<Func<T, object>> properties)
        {
            return SharedIndex(query, "CREATE", properties);
        }

        /// <summary>
        /// Drops a single-property or composite index on properties of a label.
        /// E.g., query.DropIndex((Actor actor) => actor.Name) should generate the cypher statement: DROP INDEX ON :Actor(name). This is for a single-property index.
        /// Likewise, for a composite index, query.DropIndex((Actor actor) => new { actor.Name, actor.Born }) should generate the cypher statement: DROP INDEX ON :Actor(name, born).
        /// The examples assume camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static ICypherFluentQuery DropIndex<T>(this ICypherFluentQuery query, Expression<Func<T, object>> properties)
        {
            return SharedIndex(query, "DROP", properties);
        }



        internal static ICypherFluentQuery SharedIndex<T>(this ICypherFluentQuery query, string clause, Expression<Func<T, object>> properties)
        {
            query = query.WithExpression(properties, out var propNames, isMemberAccess: false);

            var aggregatePropNames = propNames.Aggregate((first, second) => $"{first}, {second}");

            var label = query.GetAnnotationsContext().EntityService.GetEntityTypeInfo(typeof(T)).LabelsWithTypeNameCatch.First(); //you only set constraints on a label so the type has to be specific.

            return Clause(query, $"{clause} INDEX ON :{label}({aggregatePropNames})");
        }
        #endregion

        #region Constraint
        /// <summary>
        /// Creates a uniqueness constraint on a property.
        /// E.g., query.CreateUniqueConstraint((Actor actor) => actor.Name) generates the cypher: CREATE CONSTRAINT ON (actor:Actor) ASSERT actor.name IS UNIQUE.
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static ICypherFluentQuery CreateUniqueConstraint<T>(this ICypherFluentQuery query, Expression<Func<T, object>> property)
        {
            return SharedConstraint(query, "CREATE", "{0} IS UNIQUE", property, isRelationship: false);
        }

        /// <summary>
        /// Drops a uniqueness constraint on a property.
        /// E.g., query.DropUniqueConstraint((Actor actor) => actor.Name) generates the cypher: DROP CONSTRAINT ON (actor:Actor) ASSERT actor.name IS UNIQUE.
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static ICypherFluentQuery DropUniqueConstraint<T>(this ICypherFluentQuery query, Expression<Func<T, object>> property)
        {
            return SharedConstraint(query, "DROP", "{0} IS UNIQUE", property, isRelationship: false);
        }

        /// <summary>
        /// Creates an existence constraint on a property.
        /// E.g., query.CreateExistsConstraint((Actor actor) => actor.Name) generates the cypher: CREATE CONSTRAINT ON (actor:Actor) ASSERT exists(actor.name).
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static ICypherFluentQuery CreateExistsConstraint<T>(this ICypherFluentQuery query, Expression<Func<T, object>> property)
        {
            return CreateExistsConstraint(query, property, isRelationship: false);
        }

        /// <summary>
        /// Creates an existence constraint on a relationship property.
        /// E.g., query.CreateExistsConstraint((MovieActorRelationship rel) => rel.Roles, true) generates the cypher: CREATE CONSTRAINT ON ()-[rel:ACTED_IN]-() ASSERT exists(rel.roles).
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="property"></param>
        /// <param name="isRelationship">Set to true if <typeparamref name="T"/> is a relationship model class, else false for nodes.</param>
        /// <returns></returns>
        public static ICypherFluentQuery CreateExistsConstraint<T>(this ICypherFluentQuery query, Expression<Func<T, object>> property, bool isRelationship)
        {
            return SharedConstraint(query, "CREATE", "exists({0})", property, isRelationship: isRelationship);
        }

        /// <summary>
        /// Drops an existence constraint on a property.
        /// E.g., query.DropExistsConstraint((Actor actor) => actor.Name) generates the cypher: DROP CONSTRAINT ON (actor:Actor) ASSERT exists(actor.name).
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static ICypherFluentQuery DropExistsConstraint<T>(this ICypherFluentQuery query, Expression<Func<T, object>> property)
        {
            return DropExistsConstraint(query, property, isRelationship: false);
        }

        /// <summary>
        /// Drops an existence constraint on a relationship property.
        /// E.g., query.DropExistsConstraint((MovieActorRelationship rel) => rel.Roles, true) generates the cypher: DROP CONSTRAINT ON ()-[rel:ACTED_IN]-() ASSERT exists(rel.roles).
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="property"></param>
        /// <param name="isRelationship">Set to true if <typeparamref name="T"/> is a relationship model class, else false for nodes.</param>
        /// <returns></returns>
        public static ICypherFluentQuery DropExistsConstraint<T>(this ICypherFluentQuery query, Expression<Func<T, object>> property, bool isRelationship)
        {
            return SharedConstraint(query, "DROP", "exists({0})", property, isRelationship: isRelationship);
        }

        /// <summary>
        /// Creates a node key constraint on properties of a node.
        /// E.g., query.CreateKeyConstraint((Actor actor) => new { actor.FirstName, actor.LastName }) generates the cypher: CREATE CONSTRAINT ON (actor:Actor) ASSERT (actor.firstName, actor.lastName) IS NODE KEY.
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static ICypherFluentQuery CreateKeyConstraint<T>(this ICypherFluentQuery query, Expression<Func<T, object>> properties)
        {
            return SharedConstraint(query, "CREATE", "({0}) IS NODE KEY", properties, isRelationship: false);
        }

        /// <summary>
        /// Drops a node key constraint on properties of a node.
        /// E.g., query.DropKeyConstraint((Actor actor) => new { actor.FirstName, actor.LastName }) generates the cypher: DROP CONSTRAINT ON (actor:Actor) ASSERT (actor.firstName, actor.lastName) IS NODE KEY.
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static ICypherFluentQuery DropKeyConstraint<T>(this ICypherFluentQuery query, Expression<Func<T, object>> properties)
        {
            return SharedConstraint(query, "DROP", "({0}) IS NODE KEY", properties, isRelationship: false);
        }



        internal static ICypherFluentQuery SharedConstraint<T>(this ICypherFluentQuery query, string clause, 
            string assertFormat, Expression<Func<T, object>> properties, bool isRelationship)
        {
            query = query.WithExpression(properties, out var propNames, isMemberAccess: true);

            var aggregatePropNames = propNames.Aggregate((first, second) => $"{first}, {second}");

            var label = query.GetAnnotationsContext().EntityService.GetEntityTypeInfo(typeof(T)).LabelsWithTypeNameCatch.First(); //you only set constraints on a label so the type has to be specific.

            var pattern = !isRelationship ? $"({properties.Parameters[0].Name}:{label})" : $"()-[{properties.Parameters[0].Name}:{label}]-()";

            return Clause(query, $"{clause} CONSTRAINT ON {pattern} ASSERT {string.Format(assertFormat, aggregatePropNames)}");
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes a property/properties from a node.
        /// E.g., query.RemoveProperty((Actor actor) => actor.Name) generates the cypher: REMOVE actor.name.
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="property">The property to remove from node.</param>
        /// <returns></returns>
        public static ICypherFluentQuery RemoveProperty<T>(this ICypherFluentQuery query, Expression<Func<T, object>> property)
        {
            return RemoveProperty(query, property, null);
        }

        /// <summary>
        /// Removes a property/properties from a node.
        /// E.g., query.RemoveProperty((Actor actor) => actor.Name, "a") generates the cypher: REMOVE a.name.
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="property">The property to remove from node.</param>
        /// <param name="variable">>Overrides the parameter used in the predicate lambda. This is useful if the actual variable is only known at runtime (dynamic).</param>
        /// <returns></returns>
        public static ICypherFluentQuery RemoveProperty<T>(this ICypherFluentQuery query, Expression<Func<T, object>> property, string variable)
        {
            return query.WithExpression(property, out var expressions, isMemberAccess: true, variable: variable,
                defaultBuildStrategy: PropertiesBuildStrategy.WithParamsForValues).Remove(expressions.Aggregate((first, second) => $"{first}, {second}"));
        }


        /// <summary>
        /// Removes the label defined on <typeparamref name="T"/> from the node.
        /// E.g., query.RemoveLabel&lt;Actor&gt;("actor") generates the cypher: REMOVE actor:Actor.
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        public static ICypherFluentQuery RemoveLabel<T>(this ICypherFluentQuery query, string variable)
        {
            var labels = query.GetAnnotationsContext().EntityService.GetEntityTypeInfo(typeof(T)).LabelsWithTypeNameCatch; //the type has to be there one way or another.
            return RemoveMultipleLabels(query, variable, labels.First());
        }

        /// <summary>
        /// Removes all the labels defined on the <typeparamref name="T"/> type heirarchy from the node.
        /// E.g., query.RemoveAllLabels&lt;Actor&gt;("actor") generates the cypher: REMOVE actor:Actor:User.
        /// Example assumes camel casing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        public static ICypherFluentQuery RemoveAllLabels<T>(this ICypherFluentQuery query, string variable)
        {
            var labels = query.GetAnnotationsContext().EntityService.GetEntityTypeInfo(typeof(T)).LabelsWithTypeNameCatch; //the type has to be there one way or another.
            return RemoveMultipleLabels(query, variable, labels.ToArray());
        }

        /// <summary>
        /// Removes the label defined on each specified type in <paramref name="labelTypes"/> from the node.
        /// E.g., query.RemoveMultipleLabels("actor", typeof(Actor), typeof(MovieExtra)) generates the cypher: REMOVE actor:Actor:MovieExtra.
        /// Example assumes camel casing.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="variable"></param>
        /// <param name="labelTypes">All label types to be removed.</param>
        /// <returns></returns>
        public static ICypherFluentQuery RemoveMultipleLabels(this ICypherFluentQuery query, string variable, params Type[] labelTypes)
        {
            var labels = labelTypes?.Select(lt => Utils.Utilities.GetLabel(lt, useTypeNameIfEmpty: true)).ToArray();

            return RemoveMultipleLabels(query, variable, labels);
        }

        /// <summary>
        /// Removes each label specified in <paramref name="labels"/> from the node.
        /// E.g., query.RemoveMultipleLabels("actor", "Actor", "MovieExtra") generates the cypher: REMOVE actor:Actor:MovieExtra.
        /// Example assumes camel casing.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="variable"></param>
        /// <param name="labels">All labels to be removed.</param>
        /// <returns></returns>
        public static ICypherFluentQuery RemoveMultipleLabels(this ICypherFluentQuery query, string variable, params string[] labels)
        {
            string labelText = labels?.Aggregate((first, second) => $"{first}:{second}");

            if (string.IsNullOrWhiteSpace(labelText))
                throw new ArgumentNullException(nameof(labels));

            return query.Remove($"{variable}:{labelText}");
        }
        #endregion
    }
}