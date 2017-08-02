using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using Neo4jClient.Cypher;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public static class CypherFluentQueryExtensions
    {
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

        /// <summary>
        /// Builds Neo4j pattern using the <see cref="PropertiesBuildStrategy.WithParamsForValues"/> strategy. If multiple patterns are described, they are concatenated with a comma.
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: path=(a), (b)--&gt;(c).
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
        /// E.g Given: (path) =&gt; path.Pattern("a").Assign(), (path2) =&gt; path2.Pattern("b", "c"), you should get the Neo4j pattern: path=(a), (b)--&gt;(c).
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
            DataAnnotations.Utilities.GetQueryUtilities(query, out var client, out var serializer,
                    out var resolver, out var converter, out var serializerFunc, out var queryWriter);

            var finalProperties = DataAnnotations.Utilities.GetFinalProperties(properties, resolver,
                converter, serializerFunc, out var hasVariables);

            string setParam = DataAnnotations.Utilities.GetRandomVariableFor($"{variable}_set");
            setParameter = setParam;

            buildStrategy = hasVariables ? PropertiesBuildStrategy.NoParams : buildStrategy;

            string value = null;

            switch (buildStrategy)
            {
                case PropertiesBuildStrategy.WithParams:
                case PropertiesBuildStrategy.WithParamsForValues:
                    {
                        query = query.WithParam(setParam, finalProperties);

                        value = "$" + setParam;

                        if (buildStrategy == PropertiesBuildStrategy.WithParamsForValues)
                        {
                            value = finalProperties.Properties()
                                .Select(jp => $"{jp.Name}: ${setParam}.{jp.Name}")
                                .Aggregate((first, second) => $"{first}, {second}");
                        }
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

            var finalProperties = DataAnnotations.Utilities.GetFinalProperties(Utilities.GetConstraintsAsPropertiesLambda(predicate, typeof(T)), resolver,
                converter, serializerFunc, out var hasVariables);

            variable = variable ?? predicate.Parameters[0].Name;
            var setParam = DataAnnotations.Utilities.GetRandomVariableFor($"{variable}_set");
            setParameter = setParam;

            buildStrategy = hasVariables ? PropertiesBuildStrategy.NoParams : buildStrategy;

            string value = null;

            switch (buildStrategy)
            {
                case PropertiesBuildStrategy.WithParams:
                case PropertiesBuildStrategy.WithParamsForValues:
                    {
                        //in this type of SET statement, both WithParams, and WithParamsForValues are the same
                        query = query.WithParam(setParam, finalProperties);

                        value = finalProperties.Properties()
                                .Select(jp => $"{variable}.{jp.Name} = ${setParam}.{jp.Name}")
                                .Aggregate((first, second) => $"{first}, {second}");
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
    }
}
