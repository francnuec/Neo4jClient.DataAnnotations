using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public static class CypherFluentQueryExtensions
    {
        static MethodInfo mutateMethodInfo = null;
        static MethodInfo mutateGenericMethodInfo = null;

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
        /// See Tests for examples. Call <see cref="AnnotatedQueryExtensions.AsOrderedCypherQuery(IOrderedAnnotatedQuery)"/> to return to normal Cypher methods.
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
            var mutateMethod = mutateMethodInfo ?? (mutateMethodInfo = typeof(CypherFluentQuery)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name == "Mutate" && !m.IsGenericMethod)
                .First());

            Action<QueryWriter> callback = (w) => w.AppendClause(clauseText);

            return mutateMethod.Invoke(query, new object[] { callback }) as ICypherFluentQuery;
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
            var mutateMethod = mutateGenericMethodInfo ?? (mutateGenericMethodInfo = typeof(CypherFluentQuery)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name == "Mutate" && m.IsGenericMethod)
                .First());

            Action<QueryWriter> callback = (w) => w.AppendClause(clauseText);

            return mutateMethod.MakeGenericMethod(typeof(TResult)).Invoke(query, new object[] { callback }) as ICypherFluentQuery<TResult>;
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
        #endregion

        #region GetPattern
        [Obsolete("Use WithPattern instead.")]
        /// <summary>
        /// Builds Neo4j pattern using the <see cref="PropertiesBuildStrategy.NoParams"/> strategy. If multiple patterns are described, they are concatenated with a comma.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: path=(a), (b)--&gt;(c).
        /// </summary>
        /// <param name="query"></param>
        /// <param name="patternDescriptions"></param>
        /// <returns></returns>
        public static string GetPattern(this ICypherFluentQuery query,
            params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            WithPattern(query, out var pattern, PropertiesBuildStrategy.NoParams, patternDescriptions);
            return pattern;
        }

        [Obsolete("Use WithPattern instead.")]
        /// <summary>
        /// Builds Neo4j pattern using the specified <see cref="PropertiesBuildStrategy"/>. If multiple patterns are described, they are concatenated with a comma.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: path=(a), (b)--&gt;(c).
        /// </summary>
        /// <param name="query"></param>
        /// <param name="patternDescriptions"></param>
        /// <returns></returns>
        public static string GetPattern(this ICypherFluentQuery query,
            PropertiesBuildStrategy buildStrategy,
            params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            WithPattern(query, out var pattern, buildStrategy, patternDescriptions);
            return pattern;
        }
        #endregion

        #region WithPattern
        /// <summary>
        /// Builds Neo4j pattern using the <see cref="PropertiesBuildStrategy.WithParamsForValues"/> strategy. If multiple patterns are described, they are concatenated with a comma.
        /// E.g., Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: path=(a), (b)--&gt;(c).
        /// This method allows you to safely chain calls on an existing <see cref="ICypherFluentQuery"/> object.
        /// </summary>
        /// <param name="query"></param>
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
        /// <param name="buildStrategy"><see cref="PropertiesBuildStrategy"/> option to use.</param>
        /// <param name="patternDescriptions"></param>
        /// <returns></returns>
        public static ICypherFluentQuery WithPattern(this ICypherFluentQuery query, out string pattern,
            PropertiesBuildStrategy buildStrategy,
            params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            pattern = Utilities.BuildPaths(ref query,
                patternDescriptions ?? throw new ArgumentNullException(nameof(patternDescriptions)),
                buildStrategy);

            return query;
        }
        #endregion

        #region WithProperty
        /// <summary>
        /// Gets the specified property name as they would be stored/used in Neo4j.
        /// E.g., query.WithProperty((Person person) =&gt; person.Address.City, out var names), should get you a name like: address_city.
        /// You can also specify multiple properties.
        /// E.g., query.WithProperty((Person person) =&gt; new { person.Name, person.Address.City }, out var names).
        /// Camel casing is assumed for the examples.
        /// This method allows you to safely chain calls on an existing <see cref="ICypherFluentQuery"/> object.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public static ICypherFluentQuery WithProperty<T>(this ICypherFluentQuery query, 
            Expression<Func<T, object>> properties, out string[] names)
        {
            return WithProperty(query, properties, out names, makeMemberAccess: false);
        }

        /// <summary>
        /// Gets the specified property name as they would be stored/used in Neo4j.
        /// E.g., query.WithProperty((Person person) =&gt; person.Address.City, out var names), should get you a name like: address_city.
        /// You can also specify multiple properties.
        /// E.g., query.WithProperty((Person person) =&gt; new { person.Name, person.Address.City }, out var names).
        /// Camel casing is assumed for the examples.
        /// This method allows you to safely chain calls on an existing <see cref="ICypherFluentQuery"/> object.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="names"></param>
        /// <param name="makeMemberAccess">If true, the parameter name is included to make a member access.
        /// E.g. person.name, person.address_city.</param>
        /// <returns></returns>
        public static ICypherFluentQuery WithProperty<T>(this ICypherFluentQuery query,
            Expression<Func<T, object>> properties, out string[] names, bool makeMemberAccess)
        {
            Utilities.GetQueryUtilities(query, out var client, out var serializer,
                    out var resolver, out var converter, out var serializerFunc, out var queryWriter);

            names = Utilities.GetFinalPropertyNames(properties, resolver, converter,
                serializerFunc, makeNamesMemberAccess: makeMemberAccess);

            return query;
        }
        #endregion

        #region Match
        /// <summary>
        /// Generates a cypher MATCH statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: MATCH path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery Match(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return Match(query, PropertiesBuildStrategy.WithParamsForValues, patternDescriptions);
        }

        /// <summary>
        /// Generates a cypher MATCH statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: MATCH path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery Match(this ICypherFluentQuery query, PropertiesBuildStrategy buildStrategy,
            params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return WithPattern(query, out var pattern, buildStrategy, patternDescriptions).Match(pattern);
        }
        #endregion

        #region OptionalMatch
        /// <summary>
        /// Generates a cypher OPTIONAL MATCH statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: OPTIONAL MATCH path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery OptionalMatch(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return OptionalMatch(query, PropertiesBuildStrategy.WithParamsForValues, patternDescriptions);
        }

        /// <summary>
        /// Generates a cypher OPTIONAL MATCH statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: OPTIONAL MATCH path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery OptionalMatch(this ICypherFluentQuery query, PropertiesBuildStrategy buildStrategy,
            params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return WithPattern(query, out var pattern, buildStrategy, patternDescriptions).OptionalMatch(pattern);
        }
        #endregion

        #region Merge
        /// <summary>
        /// Generates a cypher MERGE statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: MERGE path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery Merge(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return Merge(query, PropertiesBuildStrategy.WithParamsForValues, patternDescriptions);
        }

        /// <summary>
        /// Generates a cypher MERGE statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: MERGE path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery Merge(this ICypherFluentQuery query, PropertiesBuildStrategy buildStrategy,
            params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return WithPattern(query, out var pattern, buildStrategy, patternDescriptions).Merge(pattern);
        }
        #endregion

        #region Create
        /// <summary>
        /// Generates a cypher CREATE statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: CREATE path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery Create(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return Create(query, PropertiesBuildStrategy.WithParams, patternDescriptions);
        }

        /// <summary>
        /// Generates a cypher CREATE statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: CREATE path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery Create(this ICypherFluentQuery query, PropertiesBuildStrategy buildStrategy,
            params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return WithPattern(query, out var pattern, buildStrategy, patternDescriptions).Create(pattern);
        }
        #endregion

        #region CreateUnique
        /// <summary>
        /// Generates a cypher CREATE UNIQUE statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: CREATE UNIQUE path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery CreateUnique(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return CreateUnique(query, PropertiesBuildStrategy.WithParams, patternDescriptions);
        }

        /// <summary>
        /// Generates a cypher CREATE UNIQUE statement from the pattern descriptions.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: CREATE UNIQUE path=(a), (b)--&gt;(c).
        /// </summary>
        public static ICypherFluentQuery CreateUnique(this ICypherFluentQuery query, PropertiesBuildStrategy buildStrategy,
            params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return WithPattern(query, out var pattern, buildStrategy, patternDescriptions).CreateUnique(pattern);
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
            return Set(query, variable, properties, PropertiesBuildStrategy.WithParams, out setParameter);
        }

        /// <summary>
        /// Generates a cypher SET statement from the properties.
        /// E.g. () =&gt; new Movie { title = "Grey's Anatomy", year = 2017 }, should generate: SET movie = { title: "Grey's Anatomy", year = 2017 }, where movie is the variable.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <returns></returns>
        public static ICypherFluentQuery Set(this ICypherFluentQuery query, string variable, 
            Expression<Func<object>> properties, PropertiesBuildStrategy buildStrategy)
        {
            return Set(query, variable, properties, buildStrategy, out var setParam);
        }

        /// <summary>
        /// Generates a cypher SET statement from the properties.
        /// E.g. () =&gt; new Movie { title = "Grey's Anatomy", year = 2017 }, should generate: SET movie = { title: "Grey's Anatomy", year = 2017 }, where movie is the variable.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <param name="setParameter">The parameter used in the <see cref="ICypherFluentQuery.WithParam(string, object)"/> call.</param>
        /// <returns></returns>
        public static ICypherFluentQuery Set(this ICypherFluentQuery query, string variable,
            Expression<Func<object>> properties, PropertiesBuildStrategy buildStrategy, out string setParameter)
        {
            return SharedSet(query, variable, properties, buildStrategy, out setParameter, add: false);
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
            return Set(query, predicate, PropertiesBuildStrategy.WithParamsForValues, out setParameter, variable);
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
        public static ICypherFluentQuery Set<T>(this ICypherFluentQuery query,
            Expression<Func<T, bool>> predicate, PropertiesBuildStrategy buildStrategy)
        {
            return Set(query, predicate, buildStrategy, null);
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
            Expression<Func<T, bool>> predicate, PropertiesBuildStrategy buildStrategy, string variable)
        {
            return Set(query, predicate, buildStrategy, out var setParam, variable);
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
            Expression<Func<T, bool>> predicate, PropertiesBuildStrategy buildStrategy, out string setParameter)
        {
            return Set(query, predicate, buildStrategy, out setParameter, null);
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
            Expression<Func<T, bool>> predicate, PropertiesBuildStrategy buildStrategy, out string setParameter, string variable)
        {
            return SharedSet(query, predicate, buildStrategy, out setParameter, variable);
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
            return SetAdd(query, predicate, PropertiesBuildStrategy.WithParams, out setParameter, variable);
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
        public static ICypherFluentQuery SetAdd<T>(this ICypherFluentQuery query,
            Expression<Func<T, bool>> predicate, PropertiesBuildStrategy buildStrategy)
        {
            return SetAdd(query, predicate, buildStrategy, null);
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
            Expression<Func<T, bool>> predicate, PropertiesBuildStrategy buildStrategy, string variable)
        {
            return SetAdd(query, predicate, buildStrategy, out var setParam, variable);
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
            Expression<Func<T, bool>> predicate, PropertiesBuildStrategy buildStrategy, out string setParameter)
        {
            return SetAdd(query, predicate, buildStrategy, out setParameter, null);
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
            Expression<Func<T, bool>> predicate, PropertiesBuildStrategy buildStrategy, out string setParameter, string variable)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var propertiesExpression = Utilities.GetConstraintsAsPropertiesLambda(predicate, typeof(T));

            return SharedSet(query, variable ?? predicate.Parameters[0].Name, propertiesExpression, buildStrategy, out setParameter, add: true);
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
            LambdaExpression properties, PropertiesBuildStrategy buildStrategy, out string setParameter, bool add)
        {
            Utilities.GetQueryUtilities(query, out var client, out var serializer,
                    out var resolver, out var converter, out var serializerFunc, out var queryWriter);

            var finalProperties = Utilities.GetFinalProperties(properties, resolver,
                converter, serializerFunc, out var hasVariables);

            string setParam = Utilities.GetRandomVariableFor($"{variable}_set");
            setParameter = setParam;

            buildStrategy = hasVariables && buildStrategy == PropertiesBuildStrategy.WithParams ? 
                PropertiesBuildStrategy.WithParamsForValues : buildStrategy;

            string value = null;

            switch (buildStrategy)
            {
                case PropertiesBuildStrategy.WithParams:
                case PropertiesBuildStrategy.WithParamsForValues:
                    {
                        var _finalProperties = finalProperties;

                        value = "$" + setParam;

                        if (buildStrategy == PropertiesBuildStrategy.WithParamsForValues)
                        {
                            value = Utilities.BuildWithParamsForValues(finalProperties, serializerFunc,
                                getKey: (propertyName) => propertyName, separator: ": ", 
                                getValue: (propertyName) => $"${setParam}.{propertyName}",
                                hasRaw: out var hasRaw, newFinalProperties: out var newFinalProperties);

                            _finalProperties = newFinalProperties ?? _finalProperties;
                        }

                        query = query.WithParam(setParam, _finalProperties);
                        break;
                    }
                case PropertiesBuildStrategy.NoParams:
                    {
                        value = finalProperties.Properties()
                                .Select(jp => $"{jp.Name}: {serializerFunc(jp.Value)}")
                                .Aggregate((first, second) => $"{first}, {second}");
                        break;
                    }
            }

            var serializedValue = value?.StartsWith("$") != true ? $"{{ {value} }}" : value;

            return query.Set($"{variable} {(add ? "+=" : "=")} {serializedValue}");
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
            Expression<Func<T, bool>> predicate, PropertiesBuildStrategy buildStrategy, out string setParameter, string variable)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            Utilities.GetQueryUtilities(query, out var client, out var serializer,
                    out var resolver, out var converter, out var serializerFunc, out var queryWriter);

            var finalProperties = Utilities.GetFinalProperties(Utilities.GetConstraintsAsPropertiesLambda(predicate, typeof(T)), resolver,
                converter, serializerFunc, out var hasVariables);

            variable = variable ?? predicate.Parameters[0].Name;
            var setParam = Utilities.GetRandomVariableFor($"{variable}_set");
            setParameter = setParam;

            buildStrategy = hasVariables && buildStrategy == PropertiesBuildStrategy.WithParams ? 
                PropertiesBuildStrategy.WithParamsForValues : buildStrategy;

            string value = null;

            switch (buildStrategy)
            {
                case PropertiesBuildStrategy.WithParams:
                case PropertiesBuildStrategy.WithParamsForValues:
                    {
                        //in this type of SET statement, both WithParams, and WithParamsForValues are the same
                        value = Utilities.BuildWithParamsForValues(finalProperties, serializerFunc,
                                getKey: (propertyName) => $"{variable}.{propertyName}", separator: " = ",
                                getValue: (propertyName) => $"${setParam}.{propertyName}",
                                hasRaw: out var hasRaw, newFinalProperties: out var newFinalProperties);

                        var _finalProperties = newFinalProperties ?? finalProperties;

                        query = query.WithParam(setParam, _finalProperties);
                        break;
                    }
                case PropertiesBuildStrategy.NoParams:
                    {
                        value = finalProperties.Properties()
                                .Select(jp => $"{variable}.{jp.Name} = {serializerFunc(jp.Value)}")
                                .Aggregate((first, second) => $"{first}, {second}");
                        break;
                    }
            }

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
            query = query.WithProperty(properties, out var propNames, makeMemberAccess: true);

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
            query = query.WithProperty(properties, out var propNames, makeMemberAccess: false);

            var aggregatePropNames = propNames.Aggregate((first, second) => $"{first}, {second}");

            var label = Neo4jAnnotations.GetEntityTypeInfo(typeof(T)).LabelsWithTypeNameCatch.First(); //you only set constraints on a label so the type has to be specific.

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
            query = query.WithProperty(properties, out var propNames, makeMemberAccess: true);

            var aggregatePropNames = propNames.Aggregate((first, second) => $"{first}, {second}");

            var label = Neo4jAnnotations.GetEntityTypeInfo(typeof(T)).LabelsWithTypeNameCatch.First(); //you only set constraints on a label so the type has to be specific.

            var pattern = !isRelationship ? $"({properties.Parameters[0].Name}:{label})" : $"()-[{properties.Parameters[0].Name}:{label}]-()";

            return Clause(query, $"{clause} CONSTRAINT ON {pattern} ASSERT {string.Format(assertFormat, aggregatePropNames)}");
        }
        #endregion
    }
}