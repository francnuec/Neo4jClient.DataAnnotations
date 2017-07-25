using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public static partial class CypherFluentQueryExtensions
    {
        /// <summary>
        /// Builds Neo4j pattern using the <see cref="PropertiesBuildStrategy.NoParams"/> strategy, separating them with a comma.
        /// E.g Given: (path) => path.Pattern("a").Assign(), (path2) => path2.Pattern("b", "c"), you should get the Neo4j pattern: path=(a), (b)-->(c).
        /// </summary>
        /// <param name="query"></param>
        /// <param name="patternDescriptions"></param>
        /// <returns></returns>
        public static string GetPattern(this ICypherFluentQuery query,
            params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return GetPattern(query, PropertiesBuildStrategy.NoParams, patternDescriptions);
        }

        /// <summary>
        /// Builds Neo4j Patterns using the specified <see cref="PropertiesBuildStrategy"/>, separating them with a comma.
        /// E.g Given: (path) => path.Pattern("a").Assign(), (path2) => path2.Pattern("b", "c"), you should get the Neo4j pattern: path=(a), (b)-->(c).
        /// </summary>
        /// <param name="query"></param>
        /// <param name="patternDescriptions"></param>
        /// <returns></returns>
        public static string GetPattern(this ICypherFluentQuery query,
            PropertiesBuildStrategy patternBuildStrategy,
            params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return Utilities.BuildPaths(query,
                patternDescriptions ?? throw new ArgumentNullException(nameof(patternDescriptions)),
                patternBuildStrategy);
        }

        /// <summary>
        /// Generates a cypher MATCH statement from the pattern descriptions.
        /// E.g Given: (path) => path.Pattern("a").Assign(), (path2) => path2.Pattern("b", "c"), you should get the Neo4j pattern: MATCH path=(a), (b)-->(c).
        /// </summary>
        public static ICypherFluentQuery Match(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return query.Match(GetPattern(query, PropertiesBuildStrategy.WithParamsForValues, patternDescriptions));
        }

        /// <summary>
        /// Generates a cypher OPTIONAL MATCH statement from the pattern descriptions.
        /// E.g Given: (path) => path.Pattern("a").Assign(), (path2) => path2.Pattern("b", "c"), you should get the Neo4j pattern: OPTIONAL MATCH path=(a), (b)-->(c).
        /// </summary>
        public static ICypherFluentQuery OptionalMatch(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return query.OptionalMatch(GetPattern(query, PropertiesBuildStrategy.WithParamsForValues, patternDescriptions));
        }

        /// <summary>
        /// Generates a cypher MERGE statement from the pattern descriptions.
        /// E.g Given: (path) => path.Pattern("a").Assign(), (path2) => path2.Pattern("b", "c"), you should get the Neo4j pattern: MERGE path=(a), (b)-->(c).
        /// </summary>
        public static ICypherFluentQuery Merge(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return query.Merge(GetPattern(query, PropertiesBuildStrategy.WithParamsForValues, patternDescriptions));
        }

        /// <summary>
        /// Generates a cypher CREATE statement from the pattern descriptions.
        /// E.g Given: (path) => path.Pattern("a").Assign(), (path2) => path2.Pattern("b", "c"), you should get the Neo4j pattern: CREATE path=(a), (b)-->(c).
        /// </summary>
        public static ICypherFluentQuery Create(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return query.Create(GetPattern(query, PropertiesBuildStrategy.WithParams, patternDescriptions));
        }

        /// <summary>
        /// Generates a cypher CREATE UNIQUE statement from the pattern descriptions.
        /// E.g Given: (path) => path.Pattern("a").Assign(), (path2) => path2.Pattern("b", "c"), you should get the Neo4j pattern: CREATE UNIQUE path=(a), (b)-->(c).
        /// </summary>
        public static ICypherFluentQuery CreateUnique(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return query.CreateUnique(GetPattern(query, PropertiesBuildStrategy.WithParams, patternDescriptions));
        }

        #region Set
        /// <summary>
        /// Generates a cypher SET statement from the predicate.
        /// E.g. movie => movie.title = "Grey's Anatomy" &amp;&amp; movie.year = 2017, should generate: SET movie.title = "Grey's Anatomy", movie.year = 2017.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static ICypherFluentQuery Set<T>(this ICypherFluentQuery query, Expression<Func<T, bool>> predicate)
        {
            return Set(query, predicate, out var setParam);
        }

        /// <summary>
        /// Generates a cypher SET statement from the predicate.
        /// E.g. movie => movie.title = "Grey's Anatomy" &amp;&amp; movie.year = 2017, should generate: SET movie.title = "Grey's Anatomy", movie.year = 2017.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <param name="setParameter">The parameter used in the <see cref="ICypherFluentQuery.WithParam(string, object)"/> call.</param>
        /// <returns></returns>
        public static ICypherFluentQuery Set<T>(this ICypherFluentQuery query, Expression<Func<T, bool>> predicate, out string setParameter)
        {
            return Set(query, predicate, PropertiesBuildStrategy.WithParamsForValues, out setParameter);
        }

        /// <summary>
        /// Generates a cypher SET statement from the predicate.
        /// E.g. movie => movie.title = "Grey's Anatomy" &amp;&amp; movie.year = 2017, should generate: SET movie.title = "Grey's Anatomy", movie.year = 2017.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static ICypherFluentQuery Set<T>(this ICypherFluentQuery query,
            Expression<Func<T, bool>> predicate, PropertiesBuildStrategy buildStrategy)
        {
            return Set(query, predicate, buildStrategy, out var setParam);
        }

        /// <summary>
        /// Generates a cypher SET statement from the predicate.
        /// E.g. movie => movie.title = "Grey's Anatomy" &amp;&amp; movie.year = 2017, should generate: SET movie.title = "Grey's Anatomy", movie.year = 2017.
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
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            Utilities.GetQueryUtilities(query, out var client, out var serializer,
                    out var resolver, out var converter, out var serializerFunc, out var queryWriter);

            var finalProperties = Utilities.GetFinalProperties(Utilities.GetConstraintsAsPropertiesLambda(predicate, typeof(T)), resolver,
                converter, serializerFunc, out var hasVariables);

            string variable = predicate.Parameters[0].Name;
            var setParam = Utilities.GetRandomVariableFor($"{variable}_set");
            setParameter = setParam;

            buildStrategy = hasVariables ? PropertiesBuildStrategy.NoParams : buildStrategy;

            string value = null;

            switch (buildStrategy)
            {
                case PropertiesBuildStrategy.WithParams:
                case PropertiesBuildStrategy.WithParamsForValues:
                    {
                        //in this type of SET statement, both WithParams, and WithParamsForValues are the same
                        query.WithParam(setParam, finalProperties);

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

        /// <summary>
        /// Generates a cypher SET statement from the properties.
        /// E.g. () => new Movie { title = "Grey's Anatomy", year = 2017 }, should generate: SET movie = { title: "Grey's Anatomy", year = 2017 }, where movie is the variable.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <returns></returns>
        public static ICypherFluentQuery Set(this ICypherFluentQuery query, string variable, Expression<Func<object>> properties)
        {
            return Set(query, variable, properties, out var setParam);
        }

        /// <summary>
        /// Generates a cypher SET statement from the properties.
        /// E.g. () => new Movie { title = "Grey's Anatomy", year = 2017 }, should generate: SET movie = { title: "Grey's Anatomy", year = 2017 }, where movie is the variable.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <param name="setParameter">The parameter used in the <see cref="ICypherFluentQuery.WithParam(string, object)"/> call.</param>
        /// <returns></returns>
        public static ICypherFluentQuery Set(this ICypherFluentQuery query, string variable, Expression<Func<object>> properties, out string setParameter)
        {
            return Set(query, variable, properties, false, out setParameter);
        }

        /// <summary>
        /// Generates a cypher SET statement from the properties.
        /// E.g. () => new Movie { title = "Grey's Anatomy", year = 2017 }, should generate: SET movie = { title: "Grey's Anatomy", year = 2017 }, where movie is the variable.
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
        /// E.g. () => new Movie { title = "Grey's Anatomy", year = 2017 }, should generate: SET movie = { title: "Grey's Anatomy", year = 2017 }, where movie is the variable.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <param name="setParameter">The parameter used in the <see cref="ICypherFluentQuery.WithParam(string, object)"/> call.</param>
        /// <returns></returns>
        public static ICypherFluentQuery Set(this ICypherFluentQuery query, string variable,
            Expression<Func<object>> properties, PropertiesBuildStrategy buildStrategy, out string setParameter)
        {
            return Set(query, variable, properties, false, buildStrategy, out setParameter);
        }

        /// <summary>
        /// Generates a cypher SET statement from the properties.
        /// E.g. () => new Movie { title = "Grey's Anatomy", year = 2017 }, should generate: SET movie = { title: "Grey's Anatomy", year = 2017 }, where movie is the variable.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <param name="add">If this is set to true, the SET statement would use the '+=' operator and not the '=' operator.</param>
        /// <returns></returns>
        public static ICypherFluentQuery Set(this ICypherFluentQuery query, string variable, Expression<Func<object>> properties, bool add)
        {
            return Set(query, variable, properties, add, out var setParam);
        }

        /// <summary>
        /// Generates a cypher SET statement from the properties.
        /// E.g. () => new Movie { title = "Grey's Anatomy", year = 2017 }, should generate: SET movie = { title: "Grey's Anatomy", year = 2017 }, where movie is the variable.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <param name="add">If this is set to true, the SET statement would use the '+=' operator and not the '=' operator.</param>
        /// <param name="setParameter">The parameter used in the <see cref="ICypherFluentQuery.WithParam(string, object)"/> call.</param>
        /// <returns></returns>
        public static ICypherFluentQuery Set(this ICypherFluentQuery query, string variable,
            Expression<Func<object>> properties, bool add, out string setParameter)
        {
            return Set(query, variable, properties, add, PropertiesBuildStrategy.WithParams, out setParameter);
        }

        /// <summary>
        /// Generates a cypher SET statement from the properties.
        /// E.g. () => new Movie { title = "Grey's Anatomy", year = 2017 }, should generate: SET movie = { title: "Grey's Anatomy", year = 2017 }, where movie is the variable.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <param name="add">If this is set to true, the SET statement would use the '+=' operator and not the '=' operator.</param>
        /// <returns></returns>
        public static ICypherFluentQuery Set(this ICypherFluentQuery query, string variable,
            Expression<Func<object>> properties, bool add, PropertiesBuildStrategy buildStrategy)
        {
            return Set(query, variable, properties, add, buildStrategy, out var setParam);
        }

        /// <summary>
        /// Generates a cypher SET statement from the properties.
        /// E.g. () => new Movie { title = "Grey's Anatomy", year = 2017 }, should generate: SET movie = { title: "Grey's Anatomy", year = 2017 }, where movie is the variable.
        /// The <see cref="PropertiesBuildStrategy"/> used could slightly modify the statement generated.
        /// </summary>
        /// <param name="add">If this is set to true, the SET statement would use the '+=' operator and not the '=' operator.</param>
        /// <param name="setParameter">The parameter used in the <see cref="ICypherFluentQuery.WithParam(string, object)"/> call.</param>
        /// <returns></returns>
        public static ICypherFluentQuery Set(this ICypherFluentQuery query, string variable, 
            Expression<Func<object>> properties, bool add, PropertiesBuildStrategy buildStrategy, out string setParameter)
        {
            Utilities.GetQueryUtilities(query, out var client, out var serializer,
                    out var resolver, out var converter, out var serializerFunc, out var queryWriter);

            var finalProperties = Utilities.GetFinalProperties(properties, resolver,
                converter, serializerFunc, out var hasVariables);

            string setParam = Utilities.GetRandomVariableFor($"{variable}_set");
            setParameter = setParam;

            buildStrategy = hasVariables ? PropertiesBuildStrategy.NoParams : buildStrategy;

            string value = null;

            switch (buildStrategy)
            {
                case PropertiesBuildStrategy.WithParams:
                case PropertiesBuildStrategy.WithParamsForValues:
                    {
                        query.WithParam(setParam, finalProperties);

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

            return query.Set($"{variable} {(add? "+=" : "=")} {serializedValue}");
        }
        #endregion
    }
}
