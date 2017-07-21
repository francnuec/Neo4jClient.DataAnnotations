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
        /// Builds Neo4j pattern using the <see cref="PatternBuildStrategy.NoParams"/> strategy, separating them with a comma.
        /// E.g Given: (path) => path.Pattern("a").Assign(), (path2) => path2.Pattern("b", "c"), you should get the Neo4j pattern: path=(a), (b)-->(c).
        /// </summary>
        /// <param name="query"></param>
        /// <param name="patternDescriptions"></param>
        /// <returns></returns>
        public static string GetPattern(this ICypherFluentQuery query,
            params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return GetPattern(query, PatternBuildStrategy.NoParams, patternDescriptions);
        }

        /// <summary>
        /// Builds Neo4j Patterns using the specified <see cref="PatternBuildStrategy"/>, separating them with a comma.
        /// E.g Given: (path) => path.Pattern("a").Assign(), (path2) => path2.Pattern("b", "c"), you should get the Neo4j pattern: path=(a), (b)-->(c).
        /// </summary>
        /// <param name="query"></param>
        /// <param name="patternDescriptions"></param>
        /// <returns></returns>
        public static string GetPattern(this ICypherFluentQuery query,
            PatternBuildStrategy patternBuildStrategy,
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
            return query.Match(GetPattern(query, PatternBuildStrategy.WithParamsForValues, patternDescriptions));
        }

        /// <summary>
        /// Generates a cypher OPTIONAL MATCH statement from the pattern descriptions.
        /// E.g Given: (path) => path.Pattern("a").Assign(), (path2) => path2.Pattern("b", "c"), you should get the Neo4j pattern: OPTIONAL MATCH path=(a), (b)-->(c).
        /// </summary>
        public static ICypherFluentQuery OptionalMatch(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return query.OptionalMatch(GetPattern(query, PatternBuildStrategy.WithParamsForValues, patternDescriptions));
        }

        /// <summary>
        /// Generates a cypher MERGE statement from the pattern descriptions.
        /// E.g Given: (path) => path.Pattern("a").Assign(), (path2) => path2.Pattern("b", "c"), you should get the Neo4j pattern: MERGE path=(a), (b)-->(c).
        /// </summary>
        public static ICypherFluentQuery Merge(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return query.Merge(GetPattern(query, PatternBuildStrategy.WithParamsForValues, patternDescriptions));
        }

        /// <summary>
        /// Generates a cypher CREATE statement from the pattern descriptions.
        /// E.g Given: (path) => path.Pattern("a").Assign(), (path2) => path2.Pattern("b", "c"), you should get the Neo4j pattern: CREATE path=(a), (b)-->(c).
        /// </summary>
        public static ICypherFluentQuery Create(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return query.Create(GetPattern(query, PatternBuildStrategy.WithParams, patternDescriptions));
        }

        /// <summary>
        /// Generates a cypher CREATE UNIQUE statement from the pattern descriptions.
        /// E.g Given: (path) => path.Pattern("a").Assign(), (path2) => path2.Pattern("b", "c"), you should get the Neo4j pattern: CREATE UNIQUE path=(a), (b)-->(c).
        /// </summary>
        public static ICypherFluentQuery CreateUnique(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathExtent>>[] patternDescriptions)
        {
            return query.CreateUnique(GetPattern(query, PatternBuildStrategy.WithParams, patternDescriptions));
        }
    }
}
