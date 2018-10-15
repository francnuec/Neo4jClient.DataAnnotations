using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher.Short
{
    public static class PathShortExtensions
    {
        //P = (A)-[R]-(B), where A = First Node in pattern, R = relationship, B = Last Node in pattern, P = entire path, dir = direction of relationship
        //Names are so chosen to allow ease in naming paramters explicitly.
        
        #region P
        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath P(this IPathBuilder source, string A)
        {
            return PathExtensions.SharedPattern(source, A, null, null, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath P(this IPathBuilder source, string A, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern(source, A, null, null, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath P(this IPathBuilder source, string A, string B)
        {
            return PathExtensions.SharedPattern(source, A, null, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath P(this IPathBuilder source, string A, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern(source, A, null, B, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath P(this IPathBuilder source, string A, string R, string B)
        {
            return PathExtensions.SharedPattern(source, A, R, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath P(this IPathBuilder source, string A, string R, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern(source, A, R, B, dir);
        }



        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode> P<TANode>(this IPathBuilder source, string A)
        {
            return PathExtensions.SharedPattern<TANode>(source, A, null, null, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode> P<TANode>(this IPathBuilder source, string A, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern<TANode>(source, A, null, null, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode> P<TANode>(this IPathBuilder source, string A, string B)
        {
            return PathExtensions.SharedPattern<TANode>(source, A, null, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode> P<TANode>(this IPathBuilder source, string A, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern<TANode>(source, A, null, B, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode> P<TANode>(this IPathBuilder source, string A, string R, string B)
        {
            return PathExtensions.SharedPattern<TANode>(source, A, R, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode> P<TANode>(this IPathBuilder source, string A, string R, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern<TANode>(source, A, R, B, dir);
        }



        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, string A)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, A, null, null, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, string A, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, A, null, null, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, string A, string B)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, A, null, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, string A, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, A, null, B, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, string A, string R, string B)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, A, R, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, string A, string R, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, A, R, B, dir);
        }



        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="relationship">Selects the property that represents the one-to-one relationship in this pattern. The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TBNode>> relationship)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, relationship, null, null, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="relationship">Selects the property that represents the one-to-one relationship in this pattern. The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TBNode>> relationship, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, relationship, null, null, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="relationship">Selects the property that represents the one-to-one relationship in this pattern. The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TBNode>> relationship, string B)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, relationship, null, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="relationship">Selects the property that represents the one-to-one relationship in this pattern. The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TBNode>> relationship, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, relationship, null, B, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="relationship">Selects the property that represents the one-to-one relationship in this pattern. The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TBNode>> relationship, string R, string B)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, relationship, R, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="relationship">Selects the property that represents the one-to-one relationship in this pattern. The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TBNode>> relationship, string R, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, relationship, R, B, dir);
        }



        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="relationship">Selects the property that represents the to-many relationship in this pattern. The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, relationship, null, null, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="relationship">Selects the property that represents the to-many relationship in this pattern. The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, relationship, null, null, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="relationship">Selects the property that represents the to-many relationship in this pattern. The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string B)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, relationship, null, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="relationship">Selects the property that represents the to-many relationship in this pattern. The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, relationship, null, B, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="relationship">Selects the property that represents the to-many relationship in this pattern. The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string R, string B)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, relationship, R, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="relationship">Selects the property that represents the to-many relationship in this pattern. The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TBNode> P<TANode, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string R, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedPattern<TANode, TBNode>(source, relationship, R, B, dir);
        }



        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, string A)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, A, null, null, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, string A, RelationshipDirection dir)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, A, null, null, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, string A, string B)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, A, null, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, string A, string B, RelationshipDirection dir)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, A, null, B, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, string A, string R, string B)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, A, R, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Variable for first node in pattern</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, string A, string R, string B, RelationshipDirection dir)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, A, R, B, dir);
        }



        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TRel>> beginRelationship)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, null, null, null, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TRel>> beginRelationship, RelationshipDirection dir)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, null, null, null, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TRel>> beginRelationship, string R, string B)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, null, R, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TRel>> beginRelationship, string R, string B, RelationshipDirection dir)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, null, R, B, dir);
        }



        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the to-many relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, null, null, null, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the to-many relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship, RelationshipDirection dir)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, null, null, null, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the to-many relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship, string R, string B)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, null, R, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the to-many relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="R">Variable for relationship</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship, string R, string B, RelationshipDirection dir)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, null, R, B, dir);
        }



        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="endRelationship">Selects the property that connects the other node (Node B) of this one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Relationship R.</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TRel>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, null, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="endRelationship">Selects the property that connects the other node (Node B) of this one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Relationship R.</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TRel>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, RelationshipDirection dir)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, null, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="endRelationship">Selects the property that connects the other node (Node B) of this one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Relationship R.</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TRel>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, string B)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="endRelationship">Selects the property that connects the other node (Node B) of this one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Relationship R.</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, TRel>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, string B, RelationshipDirection dir)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, B, dir);
        }



        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the to-many relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="endRelationship">Selects the property that connects the other node (Node B) of this one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Relationship R.</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, null, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the to-many relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="endRelationship">Selects the property that connects the other node (Node B) of this one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Relationship R.</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, RelationshipDirection dir)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, null, dir);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the to-many relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="endRelationship">Selects the property that connects the other node (Node B) of this one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Relationship R.</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, string B)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, B, null);
        }

        /// <summary>
        /// P = (A)-[R]-(B).
        /// To omit any variable, pass empty string or null to it.
        /// E.g. To omit the first node A in an incoming relationship, pass null to A, and you have ()&lt;-[R]-(B)
        /// </summary>
        /// <typeparam name="TANode">The class of Node A. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <typeparam name="TRel">The class of Relationship R. If annotated, information like its type could be gotten from this class.</typeparam>
        /// <typeparam name="TBNode">The class of Node B. If annotated, information like labels could be gotten from this class.</typeparam>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="beginRelationship">Selects the property that represent the to-many relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Node A.</param>
        /// <param name="endRelationship">Selects the property that connects the other node (Node B) of this one-to-one relationship in this pattern.
        /// The selected property returns, instead of a node class, a dedicated class that is used to represent this relationship.
        /// The variable used in this expression automatically represents Relationship R.</param>
        /// <param name="B">Variable for last node in pattern</param>
        /// <param name="dir">The direction of the relationship, i.e. how the arrow is placed.
        /// Null value means annotations determine the direction.
        /// If no annotations, it would default to <see cref="RelationshipDirection.Outgoing"/></param>
        /// <returns>A source that can allow you match more nodes.</returns>
        /// <exception cref="InvalidOperationException">Usually happens when all variables are null. In most cases, at least one variable should be named.</exception>
        public static IPatternedPath<TANode, TRel, TBNode> P<TANode, TRel, TBNode>
            (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, string B, RelationshipDirection dir)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedPattern<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, B, dir);
        }
        #endregion

        #region L
        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath L
            (this IPatternedPath source, IEnumerable<string> A)
        {
            return L(source, A, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath L
            (this IPatternedPath source, IEnumerable<string> A, bool replaceA)
        {
            return L(source, A, null, null, replaceA, false, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath L
            (this IPatternedPath source, IEnumerable<string> A, IEnumerable<string> B)
        {
            return L(source, A, B, false, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath L
            (this IPatternedPath source, IEnumerable<string> A, IEnumerable<string> B,
            bool replaceA, bool replaceB)
        {
            return L(source, A, null, B, replaceA, false, replaceB);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath L
            (this IPatternedPath source, IEnumerable<string> A, IEnumerable<string> R, IEnumerable<string> B)
        {
            return L(source, A, R, B, false, false, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath L
            (this IPatternedPath source, IEnumerable<string> A, IEnumerable<string> R, IEnumerable<string> B,
            bool replaceA, bool replaceR, bool replaceB)
        {
            return PathExtensions.SharedLabel(source, A, R, B, replaceA, replaceR, replaceB);
        }



        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode> L<TANode>
            (this IPatternedPath<TANode> source, IEnumerable<string> A)
        {
            return L(source, A, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode> L<TANode>
            (this IPatternedPath<TANode> source, IEnumerable<string> A, bool replaceA)
        {
            return L(source, A, null, null, replaceA, false, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode> L<TANode>
            (this IPatternedPath<TANode> source, IEnumerable<string> A, IEnumerable<string> B)
        {
            return L(source, A, B, false, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode> L<TANode>
            (this IPatternedPath<TANode> source, IEnumerable<string> A, IEnumerable<string> B,
            bool replaceA, bool replaceB)
        {
            return L(source, A, null, B, replaceA, false, replaceB);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode> L<TANode>
            (this IPatternedPath<TANode> source, IEnumerable<string> A, IEnumerable<string> R, IEnumerable<string> B)
        {
            return L(source, A, R, B, false, false, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode> L<TANode>
            (this IPatternedPath<TANode> source, IEnumerable<string> A, IEnumerable<string> R, IEnumerable<string> B,
            bool replaceA, bool replaceR, bool replaceB)
        {
            return (IPatternedPath<TANode>)PathExtensions.SharedLabel(source, A, R, B, replaceA, replaceR, replaceB);
        }



        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> L<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source, IEnumerable<string> A)
        {
            return L(source, A, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> L<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source, IEnumerable<string> A, bool replaceA)
        {
            return L(source, A, null, null, replaceA, false, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> L<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source, IEnumerable<string> A, IEnumerable<string> B)
        {
            return L(source, A, B, false, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> L<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source, IEnumerable<string> A, IEnumerable<string> B,
            bool replaceA, bool replaceB)
        {
            return L(source, A, null, B, replaceA, false, replaceB);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> L<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source, IEnumerable<string> A, IEnumerable<string> R, IEnumerable<string> B)
        {
            return L(source, A, R, B, false, false, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> L<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source, IEnumerable<string> A, IEnumerable<string> R, IEnumerable<string> B,
            bool replaceA, bool replaceR, bool replaceB)
        {
            return (IPatternedPath<TANode, TBNode>)PathExtensions.SharedLabel(source, A, R, B, replaceA, replaceR, replaceB);
        }



        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> L<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source, IEnumerable<string> A)
        {
            return L(source, A, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> L<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source, IEnumerable<string> A, bool replaceA)
        {
            return L(source, A, null, null, replaceA, false, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> L<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source, IEnumerable<string> A, IEnumerable<string> B)
        {
            return L(source, A, B, false, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> L<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source, IEnumerable<string> A, IEnumerable<string> B,
            bool replaceA, bool replaceB)
        {
            return L(source, A, null, B, replaceA, false, replaceB);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> L<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source, IEnumerable<string> A, IEnumerable<string> R, IEnumerable<string> B)
        {
            return L(source, A, R, B, false, false, false);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> L<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source, IEnumerable<string> A, IEnumerable<string> R, IEnumerable<string> B,
            bool replaceA, bool replaceR, bool replaceB)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedLabel(source, A, R, B, replaceA, replaceR, replaceB);
        }
        #endregion

        #region H
        /// <summary>
        /// (A)-[*1..6]-(B)
        /// Adds variable length relationships where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPath H
            (this IPatternedPath source)
        {
            return H(source, null, null);
        }

        /// <summary>
        /// (A)-[*1..6]-(B)
        /// Adds variable length relationships where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPath H
            (this IPatternedPath source, int exact)
        {
            return H(source, exact, exact);
        }

        /// <summary>
        /// (A)-[*1..6]-(B)
        /// Adds variable length relationships where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPath H
            (this IPatternedPath source, int? from, int? to)
        {
            return (IPatternedPath)PathExtensions.SharedHop<CypherObject, CypherObject, CypherObject>(source, from, to);
        }


        /// <summary>
        /// (A)-[*1..6]-(B)
        /// Adds variable length relationships where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode> H<TANode>
            (this IPatternedPath<TANode> source)
        {
            return H(source, null, null);
        }

        /// <summary>
        /// (A)-[*1..6]-(B)
        /// Adds variable length relationships where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode> H<TANode>
            (this IPatternedPath<TANode> source, int exact)
        {
            return H(source, exact, exact);
        }

        /// <summary>
        /// (A)-[*1..6]-(B)
        /// Adds variable length relationships where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode> H<TANode>
            (this IPatternedPath<TANode> source, int? from, int? to)
        {
            return (IPatternedPath<TANode>)PathExtensions.SharedHop<TANode, CypherObject, CypherObject>(source, from, to);
        }



        /// <summary>
        /// (A)-[*1..6]-(B)
        /// Adds variable length relationships where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> H<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source)
        {
            return H(source, null, null);
        }

        /// <summary>
        /// (A)-[*1..6]-(B)
        /// Adds variable length relationships where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> H<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source, int exact)
        {
            return H(source, exact, exact);
        }

        /// <summary>
        /// (A)-[*1..6]-(B)
        /// Adds variable length relationships where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> H<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source, int? from, int? to)
        {
            return (IPatternedPath<TANode, TBNode>)PathExtensions.SharedHop<TANode, CypherObject, TBNode>(source, from, to);
        }



        /// <summary>
        /// (A)-[*1..6]-(B)
        /// Adds variable length relationships where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> H<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source)
        {
            return H(source, null, null);
        }

        /// <summary>
        /// (A)-[*1..6]-(B)
        /// Adds variable length relationships where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> H<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source, int exact)
        {
            return H(source, exact, exact);
        }

        /// <summary>
        /// (A)-[*1..6]-(B)
        /// Adds variable length relationships where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> H<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source, int? from, int? to)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedHop<TANode, TRel, TBNode>(source, from, to);
        }
        #endregion

        #region C
        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds constraints to the entities.
        /// Each expression expects simple property comparisons. E.g. A: (actor) =&gt; actor.Name == "Ellen Pompeo" &amp;&amp; actor.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, A: (actor) =&gt; actor["Name"] == "Ellen Pompeo" &amp;&amp; actor["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: (actor) =&gt; actor.movie == Vars.get("movie")["name"].
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Constraints for Node A.</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath C
            (this IPatternedPath source, 
            Expression<Func<CypherObject, bool>> A)
        {
            return C(source, A, null, null);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds constraints to the entities.
        /// Each expression expects simple property comparisons. E.g. A: (actor) =&gt; actor.Name == "Ellen Pompeo" &amp;&amp; actor.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, A: (actor) =&gt; actor["Name"] == "Ellen Pompeo" &amp;&amp; actor["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously. 
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: (actor) =&gt; actor.movie == Vars.get("movie")["name"].
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Constraints for Node A.</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath C
            (this IPatternedPath source,
            Expression<Func<CypherObject, bool>> A,
            Expression<Func<CypherObject, bool>> B)
        {
            return C(source, A, null, B);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds constraints to the entities.
        /// Each expression expects simple property comparisons. E.g. A: (actor) =&gt; actor.Name == "Ellen Pompeo" &amp;&amp; actor.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, A: (actor) =&gt; actor["Name"] == "Ellen Pompeo" &amp;&amp; actor["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: (actor) =&gt; actor.movie == Vars.get("movie")["name"].
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Constraints for Node A.</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath C
            (this IPatternedPath source,
            Expression<Func<CypherObject, bool>> A,
            Expression<Func<CypherObject, bool>> R,
            Expression<Func<CypherObject, bool>> B)
        {
            return PathExtensions.SharedConstrain(source, A, R, B);
        }



        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds constraints to the entities.
        /// Each expression expects simple property comparisons. E.g. A: (actor) =&gt; actor.Name == "Ellen Pompeo" &amp;&amp; actor.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, A: (actor) =&gt; actor["Name"] == "Ellen Pompeo" &amp;&amp; actor["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: (actor) =&gt; actor.movie == Vars.get("movie")["name"].
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Constraints for Node A.</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode> C<TANode>
            (this IPatternedPath<TANode> source, 
            Expression<Func<TANode, bool>> A)
        {
            return C(source, A, null, null);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds constraints to the entities.
        /// Each expression expects simple property comparisons. E.g. A: (actor) =&gt; actor.Name == "Ellen Pompeo" &amp;&amp; actor.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, A: (actor) =&gt; actor["Name"] == "Ellen Pompeo" &amp;&amp; actor["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: (actor) =&gt; actor.movie == Vars.get("movie")["name"].
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Constraints for Node A.</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode> C<TANode>
            (this IPatternedPath<TANode> source,
            Expression<Func<TANode, bool>> A,
            Expression<Func<CypherObject, bool>> B)
        {
            return C(source, A, null, B);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds constraints to the entities.
        /// Each expression expects simple property comparisons. E.g. A: (actor) =&gt; actor.Name == "Ellen Pompeo" &amp;&amp; actor.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, A: (actor) =&gt; actor["Name"] == "Ellen Pompeo" &amp;&amp; actor["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: (actor) =&gt; actor.movie == Vars.get("movie")["name"].
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Constraints for Node A.</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode> C<TANode>
            (this IPatternedPath<TANode> source,
            Expression<Func<TANode, bool>> A,
            Expression<Func<CypherObject, bool>> R,
            Expression<Func<CypherObject, bool>> B)
        {
            return (IPatternedPath<TANode>)PathExtensions.SharedConstrain(source, A, R, B);
        }



        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds constraints to the entities.
        /// Each expression expects simple property comparisons. E.g. A: (actor) =&gt; actor.Name == "Ellen Pompeo" &amp;&amp; actor.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, A: (actor) =&gt; actor["Name"] == "Ellen Pompeo" &amp;&amp; actor["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: (actor) =&gt; actor.movie == Vars.get("movie")["name"].
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Constraints for Node A.</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> C<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source,
            Expression<Func<TANode, bool>> A)
        {
            return C(source, A, null, null);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds constraints to the entities.
        /// Each expression expects simple property comparisons. E.g. A: (actor) =&gt; actor.Name == "Ellen Pompeo" &amp;&amp; actor.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, A: (actor) =&gt; actor["Name"] == "Ellen Pompeo" &amp;&amp; actor["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: (actor) =&gt; actor.movie == Vars.get("movie")["name"].
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Constraints for Node A.</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> C<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source,
            Expression<Func<TANode, bool>> A,
            Expression<Func<TBNode, bool>> B)
        {
            return C(source, A, null, B);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds constraints to the entities.
        /// Each expression expects simple property comparisons. E.g. A: (actor) =&gt; actor.Name == "Ellen Pompeo" &amp;&amp; actor.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, A: (actor) =&gt; actor["Name"] == "Ellen Pompeo" &amp;&amp; actor["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: (actor) =&gt; actor.movie == Vars.get("movie")["name"].
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Constraints for Node A.</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> C<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source,
            Expression<Func<TANode, bool>> A,
            Expression<Func<CypherObject, bool>> R,
            Expression<Func<TBNode, bool>> B)
        {
            return (IPatternedPath<TANode, TBNode>)PathExtensions.SharedConstrain(source, A, R, B);
        }



        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds constraints to the entities.
        /// Each expression expects simple property comparisons. E.g. A: (actor) =&gt; actor.Name == "Ellen Pompeo" &amp;&amp; actor.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, A: (actor) =&gt; actor["Name"] == "Ellen Pompeo" &amp;&amp; actor["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: (actor) =&gt; actor.movie == Vars.get("movie")["name"].
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Constraints for Node A.</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> C<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source,
            Expression<Func<TANode, bool>> A)
        {
            return C(source, A, null, null);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds constraints to the entities.
        /// Each expression expects simple property comparisons. E.g. A: (actor) =&gt; actor.Name == "Ellen Pompeo" &amp;&amp; actor.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, A: (actor) =&gt; actor["Name"] == "Ellen Pompeo" &amp;&amp; actor["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: (actor) =&gt; actor.movie == Vars.get("movie")["name"].
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Constraints for Node A.</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> C<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source,
            Expression<Func<TANode, bool>> A,
            Expression<Func<TBNode, bool>> B)
        {
            return C(source, A, null, B);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds constraints to the entities.
        /// Each expression expects simple property comparisons. E.g. A: (actor) =&gt; actor.Name == "Ellen Pompeo" &amp;&amp; actor.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, A: (actor) =&gt; actor["Name"] == "Ellen Pompeo" &amp;&amp; actor["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: (actor) =&gt; actor.movie == Vars.get("movie")["name"].
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Constraints for Node A.</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> C<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source,
            Expression<Func<TANode, bool>> A,
            Expression<Func<TRel, bool>> R,
            Expression<Func<TBNode, bool>> B)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedConstrain(source, A, R, B);
        }
        #endregion

        #region PP
        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds properties to the entities. This method should be primarily used with the CREATE clause.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. A: () =&gt; actor, where actor is an instance of the class Actor which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, A: () =&gt; new { actor.Name, actor.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: () =&gt; new { Name = "Ellen Pompeo", Age = Vars.get("shondaRhimes").As&lt;Writer&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Properties for Node A.</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath PP
            (this IPatternedPath source,
            Expression<Func<object>> A)
        {
            return PP(source, A, null, null);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds properties to the entities. This method should be primarily used with the CREATE clause.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. A: () =&gt; actor, where actor is an instance of the class Actor which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, A: () =&gt; new { actor.Name, actor.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: () =&gt; new { Name = "Ellen Pompeo", Age = Vars.get("shondaRhimes").As&lt;Writer&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Properties for Node A.</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath PP
            (this IPatternedPath source,
            Expression<Func<object>> A,
            Expression<Func<object>> B)
        {
            return PP(source, A, null, B);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds properties to the entities. This method should be primarily used with the CREATE clause.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. A: () =&gt; actor, where actor is an instance of the class Actor which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, A: () =&gt; new { actor.Name, actor.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: () =&gt; new { Name = "Ellen Pompeo", Age = Vars.get("shondaRhimes").As&lt;Writer&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Properties for Node A.</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath PP
            (this IPatternedPath source,
            Expression<Func<object>> A,
            Expression<Func<object>> R,
            Expression<Func<object>> B)
        {
            return PathExtensions.SharedProp<CypherObject, CypherObject, CypherObject>(source, A, R, B);
        }



        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds properties to the entities. This method should be primarily used with the CREATE clause.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. A: () =&gt; actor, where actor is an instance of the class Actor which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, A: () =&gt; new { actor.Name, actor.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: () =&gt; new { Name = "Ellen Pompeo", Age = Vars.get("shondaRhimes").As&lt;Writer&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Properties for Node A.</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode> PP<TANode>
            (this IPatternedPath<TANode> source,
            Expression<Func<object>> A)
        {
            return PP(source, A, null, null);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds properties to the entities. This method should be primarily used with the CREATE clause.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. A: () =&gt; actor, where actor is an instance of the class Actor which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, A: () =&gt; new { actor.Name, actor.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: () =&gt; new { Name = "Ellen Pompeo", Age = Vars.get("shondaRhimes").As&lt;Writer&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Properties for Node A.</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode> PP<TANode>
            (this IPatternedPath<TANode> source,
            Expression<Func<object>> A,
            Expression<Func<object>> B)
        {
            return PP(source, A, null, B);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds properties to the entities. This method should be primarily used with the CREATE clause.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. A: () =&gt; actor, where actor is an instance of the class Actor which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, A: () =&gt; new { actor.Name, actor.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: () =&gt; new { Name = "Ellen Pompeo", Age = Vars.get("shondaRhimes").As&lt;Writer&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Properties for Node A.</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode> PP<TANode>
            (this IPatternedPath<TANode> source,
            Expression<Func<object>> A,
            Expression<Func<object>> R,
            Expression<Func<object>> B)
        {
            return (IPatternedPath<TANode>)PathExtensions.SharedProp<TANode, CypherObject, CypherObject>(source, A, R, B);
        }



        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds properties to the entities. This method should be primarily used with the CREATE clause.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. A: () =&gt; actor, where actor is an instance of the class Actor which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, A: () =&gt; new { actor.Name, actor.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: () =&gt; new { Name = "Ellen Pompeo", Age = Vars.get("shondaRhimes").As&lt;Writer&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Properties for Node A.</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> PP<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source,
            Expression<Func<object>> A)
        {
            return PP(source, A, null, null);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds properties to the entities. This method should be primarily used with the CREATE clause.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. A: () =&gt; actor, where actor is an instance of the class Actor which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, A: () =&gt; new { actor.Name, actor.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: () =&gt; new { Name = "Ellen Pompeo", Age = Vars.get("shondaRhimes").As&lt;Writer&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Properties for Node A.</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> PP<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source,
            Expression<Func<object>> A,
            Expression<Func<object>> B)
        {
            return PP(source, A, null, B);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds properties to the entities. This method should be primarily used with the CREATE clause.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. A: () =&gt; actor, where actor is an instance of the class Actor which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, A: () =&gt; new { actor.Name, actor.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: () =&gt; new { Name = "Ellen Pompeo", Age = Vars.get("shondaRhimes").As&lt;Writer&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Properties for Node A.</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> PP<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source,
            Expression<Func<object>> A,
            Expression<Func<object>> R,
            Expression<Func<object>> B)
        {
            return (IPatternedPath<TANode, TBNode>)PathExtensions.SharedProp<TANode, CypherObject, TBNode>(source, A, R, B);
        }



        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds properties to the entities. This method should be primarily used with the CREATE clause.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. A: () =&gt; actor, where actor is an instance of the class Actor which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, A: () =&gt; new { actor.Name, actor.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: () =&gt; new { Name = "Ellen Pompeo", Age = Vars.get("shondaRhimes").As&lt;Writer&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Properties for Node A.</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> PP<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source,
            Expression<Func<object>> A)
        {
            return PP(source, A, null, null);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds properties to the entities. This method should be primarily used with the CREATE clause.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. A: () =&gt; actor, where actor is an instance of the class Actor which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, A: () =&gt; new { actor.Name, actor.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: () =&gt; new { Name = "Ellen Pompeo", Age = Vars.get("shondaRhimes").As&lt;Writer&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Properties for Node A.</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> PP<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source,
            Expression<Func<object>> A,
            Expression<Func<object>> B)
        {
            return PP(source, A, null, B);
        }

        /// <summary>
        /// (A:Actor {name: "Ellen Pompeo", age: 47})-[R:ACTED_IN {isActive: true}]-(B:Movie {title: "Grey's Anatomy"}).
        /// Adds properties to the entities. This method should be primarily used with the CREATE clause.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. A: () =&gt; actor, where actor is an instance of the class Actor which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, A: () =&gt; new { actor.Name, actor.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. A: () =&gt; new { Name = "Ellen Pompeo", Age = Vars.get("shondaRhimes").As&lt;Writer&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="A">Properties for Node A.</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> PP<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source,
            Expression<Func<object>> A,
            Expression<Func<object>> R,
            Expression<Func<object>> B)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedProp<TANode, TRel, TBNode>(source, A, R, B);
        }
        #endregion

        #region T
        /// <summary>
        /// Changes the node's c# type at runtime. However, ensure that this new type can be assigned to the type specified at compile time.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode> T<TANode>
            (this IPatternedPath<TANode> source, Type A)
        {
            return (IPatternedPath<TANode>)PathExtensions.SharedType(source, A, null, null);
        }



        /// <summary>
        /// Changes the node's c# type at runtime. However, ensure that this new type can be assigned to the type specified at compile time.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> T<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source, Type A)
        {
            return T(source, A, null);
        }

        /// <summary>
        /// Changes the node's c# type at runtime. However, ensure that this new type can be assigned to the type specified at compile time.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TBNode> T<TANode, TBNode>
            (this IPatternedPath<TANode, TBNode> source, Type A, Type B)
        {
            return (IPatternedPath<TANode, TBNode>)PathExtensions.SharedType(source, A, null, B);
        }



        /// <summary>
        /// Changes the node's c# type at runtime. However, ensure that this new type can be assigned to the type specified at compile time.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> T<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source, Type A)
        {
            return T(source, A, null, null);
        }

        /// <summary>
        /// (A:Label1:Label2)-[R:TYPE1|TYPE2]-(B:Label1:Label2).
        /// Adds labels in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> T<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source, Type A, Type B)
        {
            return T(source, A, null, B);
        }

        /// <summary>
        /// Changes the node's c# type at runtime. However, ensure that this new type can be assigned to the type specified at compile time.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPath<TANode, TRel, TBNode> T<TANode, TRel, TBNode>
            (this IPatternedPath<TANode, TRel, TBNode> source, Type A, Type R, Type B)
        {
            return (IPatternedPath<TANode, TRel, TBNode>)PathExtensions.SharedType(source, A, R, B);
        }
        #endregion

        #region E
        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension E(this IPath source, RelationshipDirection dir)
        {
            return E(source, null, null, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension E(this IPath source, string B, RelationshipDirection dir)
        {
            return E(source, null, B, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension E(this IPath source,
            string R, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedExtend(source, R, B, dir);
        }



        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> E<TBNode>
            (this IPath source, RelationshipDirection dir)
        {
            return E<TBNode>(source, null, null, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> E<TBNode>
            (this IPath source, string B, RelationshipDirection dir)
        {
            return E<TBNode>(source, null, B, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> E<TBNode>
            (this IPath source, string R, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedExtend<TBNode>(source, R, B, dir);
        }



        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> E<TANode, TBNode>
            (this IPath source, Expression<Func<TANode, TBNode>> relationship)
        {
            return PathExtensions.SharedExtend(source, relationship, null, null, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> E<TANode, TBNode>
            (this IPath source, Expression<Func<TANode, TBNode>> relationship, RelationshipDirection dir)
        {
            return PathExtensions.SharedExtend(source, relationship, null, null, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> E<TANode, TBNode>
            (this IPath source, Expression<Func<TANode, TBNode>> relationship, string B)
        {
            return PathExtensions.SharedExtend(source, relationship, null, B, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> E<TANode, TBNode>
            (this IPath source, Expression<Func<TANode, TBNode>> relationship, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedExtend(source, relationship, null, B, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> E<TANode, TBNode>
            (this IPath source, Expression<Func<TANode, TBNode>> relationship, string R, string B)
        {
            return PathExtensions.SharedExtend(source, relationship, R, B, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> E<TANode, TBNode>
            (this IPath source, Expression<Func<TANode, TBNode>> relationship, string R, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedExtend(source, relationship, R, B, dir);
        }


        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> E<TANode, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship)
        {
            return PathExtensions.SharedExtend(source, relationship, null, null, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> E<TANode, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, RelationshipDirection dir)
        {
            return PathExtensions.SharedExtend(source, relationship, null, null, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> E<TANode, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string B)
        {
            return PathExtensions.SharedExtend(source, relationship, null, B, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> E<TANode, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedExtend(source, relationship, null, B, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> E<TANode, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string R, string B)
        {
            return PathExtensions.SharedExtend(source, relationship, R, B, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> E<TANode, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string R, string B, RelationshipDirection dir)
        {
            return PathExtensions.SharedExtend(source, relationship, R, B, dir);
        }


        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> E<TRel, TBNode>
            (this IPath source, RelationshipDirection dir)
        {
            return (IPatternedPathExtension<CypherObject, TRel, TBNode>)PathExtensions.SharedExtend<CypherObject, TRel, TBNode>(source, null, null, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> E<TRel, TBNode>
            (this IPath source, string B, RelationshipDirection dir)
        {
            return (IPatternedPathExtension<CypherObject, TRel, TBNode>)PathExtensions.SharedExtend<CypherObject, TRel, TBNode>(source, null, B, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> E<TRel, TBNode>
            (this IPath source, string R, string B, RelationshipDirection dir)
        {
            return (IPatternedPathExtension<CypherObject, TRel, TBNode>)PathExtensions.SharedExtend<CypherObject, TRel, TBNode>(source, R, B, dir);
        }



        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, string B)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, null, B, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, string B, RelationshipDirection dir)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, null, B, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, string R, string B)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, R, B, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, string R, string B, RelationshipDirection dir)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, R, B, dir);
        }



        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, TRel>> beginRelationship)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, null, null, null, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, TRel>> beginRelationship, RelationshipDirection dir)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, null, null, null, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, TRel>> beginRelationship, string R, string B)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, null, R, B, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, TRel>> beginRelationship, string R, string B, RelationshipDirection dir)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, null, R, B, dir);
        }



        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, null, null, null, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship, RelationshipDirection dir)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, null, null, null, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship, string R, string B)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, null, R, B, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship, string R, string B, RelationshipDirection dir)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, null, R, B, dir);
        }



        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, TRel>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)
                PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, null, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, TRel>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, RelationshipDirection dir)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)
                PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, null, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, TRel>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, string B)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)
                PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, B, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, TRel>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, string B, RelationshipDirection dir)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)
                PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, B, dir);
        }



        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)
                PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, null, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, RelationshipDirection dir)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)
                PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, null, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, string B)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)
                PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, B, null);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C).
        /// Extends an existing path.
        /// From the sample, B is the new A for this method, R2 is the new R, and C is the new B.
        /// Because this new A here (which is also the old B) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> E<TANode, TRel, TBNode>
            (this IPath source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, string B, RelationshipDirection dir)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)
                PathExtensions.SharedExtend<TANode, TRel, TBNode>(source, beginRelationship, endRelationship, null, B, dir);
        }
        #endregion

        #region ExtensionL
        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension L
            (this IPatternedPathExtension source, IEnumerable<string> B)
        {
            return L(source, B, false);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension L
            (this IPatternedPathExtension source, IEnumerable<string> B, bool replaceB)
        {
            return L(source, null, B, false, replaceB);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension L
            (this IPatternedPathExtension source, IEnumerable<string> R, IEnumerable<string> B)
        {
            return L(source, R, B, false, false);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension L
            (this IPatternedPathExtension source, IEnumerable<string> R, IEnumerable<string> B,
            bool replaceR, bool replaceB)
        {
            return PathExtensions.SharedLabel(source, R, B, replaceR, replaceB);
        }



        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> L<TBNode>
            (this IPatternedPathExtension<TBNode> source, IEnumerable<string> B)
        {
            return L(source, B, false);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> L<TBNode>
            (this IPatternedPathExtension<TBNode> source, IEnumerable<string> B, bool replaceB)
        {
            return L(source, null, B, false, replaceB);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> L<TBNode>
            (this IPatternedPathExtension<TBNode> source, IEnumerable<string> R, IEnumerable<string> B)
        {
            return L(source, R, B, false, false);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> L<TBNode>
            (this IPatternedPathExtension<TBNode> source, IEnumerable<string> R, IEnumerable<string> B,
            bool replaceR, bool replaceB)
        {
            return (IPatternedPathExtension<TBNode>)PathExtensions.SharedLabel(source, R, B, replaceR, replaceB);
        }



        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> L<TANode, TBNode>
            (this IPatternedPathExtension<TANode, TBNode> source, IEnumerable<string> B)
        {
            return L(source, B, false);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> L<TANode, TBNode>
            (this IPatternedPathExtension<TANode, TBNode> source, IEnumerable<string> B, bool replaceB)
        {
            return L(source, null, B, false, replaceB);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> L<TANode, TBNode>
            (this IPatternedPathExtension<TANode, TBNode> source, IEnumerable<string> R, IEnumerable<string> B)
        {
            return L(source, R, B, false, false);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> L<TANode, TBNode>
            (this IPatternedPathExtension<TANode, TBNode> source, IEnumerable<string> R, IEnumerable<string> B,
            bool replaceR, bool replaceB)
        {
            return (IPatternedPathExtension<TANode, TBNode>)PathExtensions.SharedLabel(source, R, B, replaceR, replaceB);
        }



        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> L<TRel, TBNode>
            (this IPatternedPathExtension<CypherObject, TRel, TBNode> source, IEnumerable<string> B)
        {
            return L(source, B, false);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> L<TRel, TBNode>
            (this IPatternedPathExtension<CypherObject, TRel, TBNode> source, IEnumerable<string> B, bool replaceB)
        {
            return L(source, null, B, false, replaceB);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> L<TRel, TBNode>
            (this IPatternedPathExtension<CypherObject, TRel, TBNode> source, IEnumerable<string> R, IEnumerable<string> B)
        {
            return L(source, R, B, false, false);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> L<TRel, TBNode>
            (this IPatternedPathExtension<CypherObject, TRel, TBNode> source, IEnumerable<string> R, IEnumerable<string> B,
            bool replaceR, bool replaceB)
        {
            return (IPatternedPathExtension<CypherObject, TRel, TBNode>)PathExtensions.SharedLabel(source, R, B, replaceR, replaceB);
        }



        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> L<TANode, TRel, TBNode>
            (this IPatternedPathExtension<TANode, TRel, TBNode> source, IEnumerable<string> B)
        {
            return L(source, B, false);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> L<TANode, TRel, TBNode>
            (this IPatternedPathExtension<TANode, TRel, TBNode> source, IEnumerable<string> B, bool replaceB)
        {
            return L(source, null, B, false, replaceB);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> L<TANode, TRel, TBNode>
            (this IPatternedPathExtension<TANode, TRel, TBNode> source, IEnumerable<string> R, IEnumerable<string> B)
        {
            return L(source, R, B, false, false);
        }

        /// <summary>
        /// (B)-[R2:TYPE1|TYPE2]-(C:Label1:Label2).
        /// Adds labels to the pattern extension, in addition to those already marked by attributes. For the relationship, you're adding types, not labels.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> L<TANode, TRel, TBNode>
            (this IPatternedPathExtension<TANode, TRel, TBNode> source, IEnumerable<string> R, IEnumerable<string> B,
            bool replaceR, bool replaceB)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedLabel(source, R, B, replaceR, replaceB);
        }
        #endregion

        #region ExtensionH
        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension H
            (this IPatternedPathExtension source)
        {
            return H(source, null, null);
        }

        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension H
            (this IPatternedPathExtension source, int exact)
        {
            return H(source, exact, exact);
        }

        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension H
            (this IPatternedPathExtension source, int? from, int? to)
        {
            return PathExtensions.SharedHop<CypherObject, CypherObject, CypherObject>(source, from, to);
        }



        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> H<TBNode>
            (this IPatternedPathExtension<TBNode> source)
        {
            return H(source, null, null);
        }

        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> H<TBNode>
            (this IPatternedPathExtension<TBNode> source, int exact)
        {
            return H(source, exact, exact);
        }

        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> H<TBNode>
            (this IPatternedPathExtension<TBNode> source, int? from, int? to)
        {
            return (IPatternedPathExtension<TBNode>)PathExtensions.SharedHop<CypherObject, CypherObject, TBNode>(source, from, to);
        }



        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> H<TANode, TBNode>
            (this IPatternedPathExtension<TANode, TBNode> source)
        {
            return H(source, null, null);
        }

        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> H<TANode, TBNode>
            (this IPatternedPathExtension<TANode, TBNode> source, int exact)
        {
            return H(source, exact, exact);
        }

        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> H<TANode, TBNode>
            (this IPatternedPathExtension<TANode, TBNode> source, int? from, int? to)
        {
            return (IPatternedPathExtension<TANode, TBNode>)PathExtensions.SharedHop<TANode, CypherObject, TBNode>(source, from, to);
        }



        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> H<TRel, TBNode>
            (this IPatternedPathExtension<CypherObject, TRel, TBNode> source)
        {
            return H(source, null, null);
        }

        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> H<TRel, TBNode>
            (this IPatternedPathExtension<CypherObject, TRel, TBNode> source, int exact)
        {
            return H(source, exact, exact);
        }

        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> H<TRel, TBNode>
            (this IPatternedPathExtension<CypherObject, TRel, TBNode> source, int? from, int? to)
        {
            return (IPatternedPathExtension<CypherObject, TRel, TBNode>)PathExtensions.SharedHop<CypherObject, TRel, TBNode>(source, from, to);
        }



        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> H<TANode, TRel, TBNode>
            (this IPatternedPathExtension<TANode, TRel, TBNode> source)
        {
            return H(source, null, null);
        }

        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> H<TANode, TRel, TBNode>
            (this IPatternedPathExtension<TANode, TRel, TBNode> source, int exact)
        {
            return H(source, exact, exact);
        }

        /// <summary>
        /// (B)-[:*1..6]-(C)
        /// Adds variable length relationships to the pattern extension where, from the sample, the number of hops would be from 1 (min), to 6 (max).
        /// To remove either hop variable, pass <c>null</c> to the variable. E.g. Omitting 1 means [*..6] which also means no lower bound.
        /// For fixed number of hops, pass the same value to the <paramref name="from"/> and <paramref name="to"/> variables, or use method provided. E.g. passing 6 and 6 means [*6]
        /// For unbounded hops, that is, Cypher considers any number of hops possible (usually not advised), pass <c>null</c> to both variables, or use the no variable method. E.g passing null means [*]
        /// </summary>
        /// <param name="exact">Fixed number of hops to consider.</param>
        /// <param name="from">Minimum number of hops to consider.</param>
        /// <param name="to">Maximum number of hops to consider.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> H<TANode, TRel, TBNode>
            (this IPatternedPathExtension<TANode, TRel, TBNode> source, int? from, int? to)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedHop<TANode, TRel, TBNode>(source, from, to);
        }
        #endregion

        #region ExtensionC
        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds constraints to the entities of the pattern extension.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects simple property comparisons. E.g. B: (writer) =&gt; writer.Name == "Shonda Rhimes" &amp;&amp; writer.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, B: (writer) =&gt; writer["Name"] == "Shonda Rhimes" &amp;&amp; writer["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension C
            (this IPatternedPathExtension source, Expression<Func<CypherObject, bool>> B)
        {
            return C(source, null, B);
        }

        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds constraints to the entities of the pattern extension.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects simple property comparisons. E.g. B: (writer) =&gt; writer.Name == "Shonda Rhimes" &amp;&amp; writer.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, B: (writer) =&gt; writer["Name"] == "Shonda Rhimes" &amp;&amp; writer["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension C
            (this IPatternedPathExtension source,
            Expression<Func<CypherObject, bool>> R,
            Expression<Func<CypherObject, bool>> B)
        {
            return PathExtensions.SharedConstrain<CypherObject, CypherObject, CypherObject>(source, R, B);
        }



        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds constraints to the entities of the pattern extension.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects simple property comparisons. E.g. B: (writer) =&gt; writer.Name == "Shonda Rhimes" &amp;&amp; writer.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, B: (writer) =&gt; writer["Name"] == "Shonda Rhimes" &amp;&amp; writer["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> C<TBNode>
            (this IPatternedPathExtension<TBNode> source, Expression<Func<TBNode, bool>> B)
        {
            return C(source, null, B);
        }

        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds constraints to the entities of the pattern extension.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects simple property comparisons. E.g. B: (writer) =&gt; writer.Name == "Shonda Rhimes" &amp;&amp; writer.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, B: (writer) =&gt; writer["Name"] == "Shonda Rhimes" &amp;&amp; writer["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> C<TBNode>
            (this IPatternedPathExtension<TBNode> source,
            Expression<Func<CypherObject, bool>> R,
            Expression<Func<TBNode, bool>> B)
        {
            return (IPatternedPathExtension<TBNode>)PathExtensions.SharedConstrain<CypherObject, CypherObject, TBNode>(source, R, B);
        }



        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds constraints to the entities of the pattern extension.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects simple property comparisons. E.g. B: (writer) =&gt; writer.Name == "Shonda Rhimes" &amp;&amp; writer.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, B: (writer) =&gt; writer["Name"] == "Shonda Rhimes" &amp;&amp; writer["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> C<TANode, TBNode>
            (this IPatternedPathExtension<TANode, TBNode> source, Expression<Func<TBNode, bool>> B)
        {
            return C(source, null, B);
        }

        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds constraints to the entities of the pattern extension.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects simple property comparisons. E.g. B: (writer) =&gt; writer.Name == "Shonda Rhimes" &amp;&amp; writer.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, B: (writer) =&gt; writer["Name"] == "Shonda Rhimes" &amp;&amp; writer["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> C<TANode, TBNode>
            (this IPatternedPathExtension<TANode, TBNode> source,
            Expression<Func<CypherObject, bool>> R,
            Expression<Func<TBNode, bool>> B)
        {
            return (IPatternedPathExtension<TANode, TBNode>)PathExtensions.SharedConstrain<TANode, CypherObject, TBNode>(source, R, B);
        }



        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds constraints to the entities of the pattern extension.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects simple property comparisons. E.g. B: (writer) =&gt; writer.Name == "Shonda Rhimes" &amp;&amp; writer.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, B: (writer) =&gt; writer["Name"] == "Shonda Rhimes" &amp;&amp; writer["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> C<TRel, TBNode>
            (this IPatternedPathExtension<CypherObject, TRel, TBNode> source,
            Expression<Func<TRel, bool>> R,
            Expression<Func<TBNode, bool>> B)
        {
            return (IPatternedPathExtension<CypherObject, TRel, TBNode>)PathExtensions.SharedConstrain<CypherObject, TRel, TBNode>(source, R, B);
        }



        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds constraints to the entities of the pattern extension.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects simple property comparisons. E.g. B: (writer) =&gt; writer.Name == "Shonda Rhimes" &amp;&amp; writer.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, B: (writer) =&gt; writer["Name"] == "Shonda Rhimes" &amp;&amp; writer["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> C<TANode, TRel, TBNode>
            (this IPatternedPathExtension<TANode, TRel, TBNode> source, Expression<Func<TBNode, bool>> B)
        {
            return C(source, null, B);
        }

        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds constraints to the entities of the pattern extension.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects simple property comparisons. E.g. B: (writer) =&gt; writer.Name == "Shonda Rhimes" &amp;&amp; writer.Age == 47.
        /// When the <see cref="CypherObject"/> is used, it becomes, B: (writer) =&gt; writer["Name"] == "Shonda Rhimes" &amp;&amp; writer["Age"] == 47.
        /// This example would generate the sample Cypher predicate shown previously.
        /// NOTE that only the LOGICAL AND operator (&amp;&amp;) is expected in the expressions. Any other operators used would either be ignored, or result in an exception being thrown.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Constraints for Relationship R.</param>
        /// <param name="B">Constraints for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> C<TANode, TRel, TBNode>
            (this IPatternedPathExtension<TANode, TRel, TBNode> source,
            Expression<Func<TRel, bool>> R,
            Expression<Func<TBNode, bool>> B)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedConstrain<TANode, TRel, TBNode>(source, R, B);
        }
        #endregion

        #region ExtensionPP
        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds properties to the entities of the pattern extension. This method should be primarily used with the CREATE clause.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. B: () =&gt; writer, where writer is an instance of the class Writer which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, B: () =&gt; new { writer.Name, writer.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. B: () =&gt; new { Name = "Shonda Rhimes", Age = Vars.get("ellenPompeo").As&lt;Actor&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension PP
            (this IPatternedPathExtension source, Expression<Func<object>> B)
        {
            return PP(source, null, B);
        }

        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds properties to the entities of the pattern extension. This method should be primarily used with the CREATE clause.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. B: () =&gt; writer, where writer is an instance of the class Writer which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, B: () =&gt; new { writer.Name, writer.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. B: () =&gt; new { Name = "Shonda Rhimes", Age = Vars.get("ellenPompeo").As&lt;Actor&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension PP
            (this IPatternedPathExtension source,
            Expression<Func<object>> R,
            Expression<Func<object>> B)
        {
            return PathExtensions.SharedProp<CypherObject, CypherObject, CypherObject>(source, R, B);
        }



        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds properties to the entities of the pattern extension. This method should be primarily used with the CREATE clause.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. B: () =&gt; writer, where writer is an instance of the class Writer which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, B: () =&gt; new { writer.Name, writer.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. B: () =&gt; new { Name = "Shonda Rhimes", Age = Vars.get("ellenPompeo").As&lt;Actor&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> PP<TBNode>
            (this IPatternedPathExtension<TBNode> source, Expression<Func<object>> B)
        {
            return PP(source, null, B);
        }

        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds properties to the entities of the pattern extension. This method should be primarily used with the CREATE clause.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. B: () =&gt; writer, where writer is an instance of the class Writer which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, B: () =&gt; new { writer.Name, writer.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. B: () =&gt; new { Name = "Shonda Rhimes", Age = Vars.get("ellenPompeo").As&lt;Actor&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> PP<TBNode>
            (this IPatternedPathExtension<TBNode> source,
            Expression<Func<object>> R,
            Expression<Func<object>> B)
        {
            return (IPatternedPathExtension<TBNode>)PathExtensions.SharedProp<CypherObject, CypherObject, TBNode>(source, R, B);
        }



        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds properties to the entities of the pattern extension. This method should be primarily used with the CREATE clause.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. B: () =&gt; writer, where writer is an instance of the class Writer which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, B: () =&gt; new { writer.Name, writer.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. B: () =&gt; new { Name = "Shonda Rhimes", Age = Vars.get("ellenPompeo").As&lt;Actor&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> PP<TANode, TBNode>
            (this IPatternedPathExtension<TANode, TBNode> source, Expression<Func<object>> B)
        {
            return PP(source, null, B);
        }

        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds properties to the entities of the pattern extension. This method should be primarily used with the CREATE clause.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. B: () =&gt; writer, where writer is an instance of the class Writer which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, B: () =&gt; new { writer.Name, writer.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. B: () =&gt; new { Name = "Shonda Rhimes", Age = Vars.get("ellenPompeo").As&lt;Actor&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> PP<TANode, TBNode>
            (this IPatternedPathExtension<TANode, TBNode> source,
            Expression<Func<object>> R,
            Expression<Func<object>> B)
        {
            return (IPatternedPathExtension<TANode, TBNode>)PathExtensions.SharedProp<TANode, CypherObject, TBNode>(source, R, B);
        }



        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds properties to the entities of the pattern extension. This method should be primarily used with the CREATE clause.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. B: () =&gt; writer, where writer is an instance of the class Writer which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, B: () =&gt; new { writer.Name, writer.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. B: () =&gt; new { Name = "Shonda Rhimes", Age = Vars.get("ellenPompeo").As&lt;Actor&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> PP<TRel, TBNode>
            (this IPatternedPathExtension<CypherObject, TRel, TBNode> source,
            Expression<Func<object>> R,
            Expression<Func<object>> B)
        {
            return (IPatternedPathExtension<CypherObject, TRel, TBNode>)PathExtensions.SharedProp<CypherObject, TRel, TBNode>(source, R, B);
        }



        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds properties to the entities of the pattern extension. This method should be primarily used with the CREATE clause.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. B: () =&gt; writer, where writer is an instance of the class Writer which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, B: () =&gt; new { writer.Name, writer.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. B: () =&gt; new { Name = "Shonda Rhimes", Age = Vars.get("ellenPompeo").As&lt;Actor&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> PP<TANode, TRel, TBNode>
            (this IPatternedPathExtension<TANode, TRel, TBNode> source, Expression<Func<object>> B)
        {
            return PP(source, null, B);
        }

        /// <summary>
        /// (B)-[R2:WROTE {isActive: true}]-(C:Writer {name: "Shonda Rhimes"}).
        /// Adds properties to the entities of the pattern extension. This method should be primarily used with the CREATE clause.
        /// Remember that, from the sample, B is our new A for this method (and hence omitted), R2 is new R, and C is new B.
        /// Each expression expects a return instance of the entity type, or an anonymous object declaring specific properties to serialize. E.g. B: () =&gt; writer, where writer is an instance of the class Writer which has only the properties Name and Age.
        /// If <see cref="object"/> type is used, it becomes, B: () =&gt; new { writer.Name, writer.Age }.
        /// This example would generate the sample Cypher predicate shown previously.
        /// You can also introduce previously declared variables with the <see cref="Vars"/> class. E.g. B: () =&gt; new { Name = "Shonda Rhimes", Age = Vars.get("ellenPompeo").As&lt;Actor&gt;().Age }, where variable shondaRhimes was declared possibly in a previous Cypher clause.
        /// NOTE that all properties on any instance supplied will be serialized. If you need only a select few properties, declare an anonymous type instead.
        /// </summary>
        /// <param name="source">Path object from the calling cypher clause</param>
        /// <param name="R">Properties for Relationship R.</param>
        /// <param name="B">Properties for Node B.</param>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> PP<TANode, TRel, TBNode>
            (this IPatternedPathExtension<TANode, TRel, TBNode> source,
            Expression<Func<object>> R,
            Expression<Func<object>> B)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedProp<TANode, TRel, TBNode>(source, R, B);
        }
        #endregion

        #region ExtensionT
        /// <summary>
        /// Changes the node's c# type at runtime. However, ensure that this new type can be assigned to the type specified at compile time.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> T<TBNode>
            (this IPatternedPathExtension<TBNode> source, Type B)
        {
            return (IPatternedPathExtension<TBNode>)PathExtensions.SharedType(source, null, B);
        }



        /// <summary>
        /// Changes the node's c# type at runtime. However, ensure that this new type can be assigned to the type specified at compile time.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> T<TANode, TBNode>
            (this IPatternedPathExtension<TANode, TBNode> source, Type B)
        {
            return (IPatternedPathExtension<TANode, TBNode>)PathExtensions.SharedType(source, null, B);
        }



        /// <summary>
        /// Changes the node's c# type at runtime. However, ensure that this new type can be assigned to the type specified at compile time.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> T<TRel, TBNode>
            (this IPatternedPathExtension<CypherObject, TRel, TBNode> source, Type B)
        {
            return T(source, null, B);
        }

        /// <summary>
        /// Changes the node's c# type at runtime. However, ensure that this new type can be assigned to the type specified at compile time.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> T<TRel, TBNode>
            (this IPatternedPathExtension<CypherObject, TRel, TBNode> source, Type R, Type B)
        {
            return (IPatternedPathExtension<CypherObject, TRel, TBNode>)PathExtensions.SharedType(source, R, B);
        }



        /// <summary>
        /// Changes the node's c# type at runtime. However, ensure that this new type can be assigned to the type specified at compile time.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> T<TANode, TRel, TBNode>
            (this IPatternedPathExtension<TANode, TRel, TBNode> source, Type B)
        {
            return T(source, null, B);
        }

        /// <summary>
        /// Changes the node's c# type at runtime. However, ensure that this new type can be assigned to the type specified at compile time.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> T<TANode, TRel, TBNode>
            (this IPatternedPathExtension<TANode, TRel, TBNode> source, Type R, Type B)
        {
            return (IPatternedPathExtension<TANode, TRel, TBNode>)PathExtensions.SharedType(source, R, B);
        }
        #endregion

        #region TE
        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension TE(this IPathExtension source, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension TE(this IPathExtension source, string B, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, B, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension TE(this IPathExtension source,
            string R, string B, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, R, B, dir);
        }



        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> TE<TBNode>
            (this IPathExtension source, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E<TBNode>(source as IPath, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> TE<TBNode>
            (this IPathExtension source, string B, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E<TBNode>(source as IPath, B, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TBNode> TE<TBNode>
            (this IPathExtension source, string R, string B, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E<TBNode>(source as IPath, R, B, dir);
        }



        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> TE<TANode, TBNode>
            (this IPathExtension source, Expression<Func<TANode, TBNode>> relationship)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, relationship);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> TE<TANode, TBNode>
            (this IPathExtension source, Expression<Func<TANode, TBNode>> relationship, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, relationship, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> TE<TANode, TBNode>
            (this IPathExtension source, Expression<Func<TANode, TBNode>> relationship, string B)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, relationship, B);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> TE<TANode, TBNode>
            (this IPathExtension source, Expression<Func<TANode, TBNode>> relationship, string B, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, relationship, B, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> TE<TANode, TBNode>
            (this IPathExtension source, Expression<Func<TANode, TBNode>> relationship, string R, string B)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, relationship, R, B);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> TE<TANode, TBNode>
            (this IPathExtension source, Expression<Func<TANode, TBNode>> relationship, string R, string B, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, relationship, R, B, dir);
        }



        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> TE<TANode, TBNode>
            (this IPathExtension source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, relationship);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> TE<TANode, TBNode>
            (this IPathExtension source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, relationship, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> TE<TANode, TBNode>
            (this IPathExtension source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string B)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, relationship, B);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> TE<TANode, TBNode>
            (this IPathExtension source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string B, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, relationship, B, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> TE<TANode, TBNode>
            (this IPathExtension source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string R, string B)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, relationship, R, B);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TBNode> TE<TANode, TBNode>
            (this IPathExtension source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string R, string B, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, relationship, R, B, dir);
        }



        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> TE<TRel, TBNode>
            (this IPathExtension source, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E<TRel, TBNode>(source as IPath, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> TE<TRel, TBNode>
            (this IPathExtension source, string B, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E<TRel, TBNode>(source as IPath, B, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<CypherObject, TRel, TBNode> TE<TRel, TBNode>
            (this IPathExtension source, string R, string B, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E<TRel, TBNode>(source as IPath, R, B, dir);
        }



        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> TE<TANode, TRel, TBNode>
            (this IPathExtension source, Expression<Func<TANode, TRel>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, beginRelationship, endRelationship);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> TE<TANode, TRel, TBNode>
            (this IPathExtension source, Expression<Func<TANode, TRel>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, beginRelationship, endRelationship, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> TE<TANode, TRel, TBNode>
            (this IPathExtension source, Expression<Func<TANode, TRel>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, string B)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, beginRelationship, endRelationship, B);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> TE<TANode, TRel, TBNode>
            (this IPathExtension source, Expression<Func<TANode, TRel>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, string B, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, beginRelationship, endRelationship, B, dir);
        }



        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> TE<TANode, TRel, TBNode>
            (this IPathExtension source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, beginRelationship, endRelationship);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> TE<TANode, TRel, TBNode>
            (this IPathExtension source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, beginRelationship, endRelationship, dir);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> TE<TANode, TRel, TBNode>
            (this IPathExtension source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, string B)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, beginRelationship, endRelationship, B);
        }

        /// <summary>
        /// (A)-[R]-(B)-[R2]-(C)-[R3]-(D).
        /// Extends an existing path even further.
        /// From the sample, C will be the new A for this method, R3 is the new R, and D is the new B.
        /// Because this new A (that is, C) has earlier on been represented in this pattern, the expression here does not consider the new A.
        /// Instead, it proceeds with new R, and new B. The method variables here work like they do in the <see cref="Pattern"/> method.
        /// </summary>
        /// <returns></returns>
        public static IPatternedPathExtension<TANode, TRel, TBNode> TE<TANode, TRel, TBNode>
            (this IPathExtension source, Expression<Func<TANode, IEnumerable<TRel>>> beginRelationship,
            Expression<Func<TRel, TBNode>> endRelationship, string B, RelationshipDirection dir)
        {
            //source is expected to be of type Path which also implements IPath.
            return E(source as IPath, beginRelationship, endRelationship, B, dir);
        }
        #endregion

        #region S
        /// <summary>
        /// P = shortestPath((A)-[R]-(B)).
        /// Calls the shortestPath function on the generated pattern.
        /// Note that Cypher allows only non-extended simple patterns for the shortestPath function.
        /// Also note that this method implicitly calls the <see cref="Assign"/> method because invoking the shortestPath function more likely means you need the path returned.
        /// Calling <see cref="Assign"/> means this should be the last method called on the pattern.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IPathExtent S(this IPath source)
        {
            return PathExtensions.SharedShortest(source);
        }

        /// <summary>
        /// P = shortestPath((A)-[R]-(B)).
        /// Calls the shortestPath function on the generated pattern.
        /// Note that Cypher allows only non-extended simple patterns for the shortestPath function.
        /// Also note that this method implicitly calls the <see cref="Assign"/> method because invoking the shortestPath function more likely means you need the path returned.
        /// Calling <see cref="Assign"/> means this should be the last method called on the pattern.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pathVariable">Use this to override the path variable (which is default to the lambda expression parameter).</param>
        /// <returns></returns>
        public static IPathExtent S(this IPath source, string pathVariable)
        {
            return PathExtensions.SharedShortest(source, pathVariable);
        }
        #endregion

        #region A
        /// <summary>
        /// Assigns the entire pattern to the path variable.
        /// That is, P = (A)-[R]-(B), 
        /// where P is the path variable as defined by the lambda expression variable of type <see cref="IPathBuilder"/> that generated this pattern.
        /// For example, Match((path) =&gt; path.Pattern("user").Assign()) would generate the Cypher: MATCH path = (user).
        /// Note that whenever the path variable is required, always make this the last method called on the <see cref="IPathBuilder"/>.
        /// Without this method being called, the generated pattern would not be assigned to a path variable.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IPathExtent A(this IPath source)
        {
            return PathExtensions.SharedAssign(source);
        }

        /// <summary>
        /// Assigns the entire pattern to the path variable.
        /// That is, P = (A)-[R]-(B), 
        /// where P is the path variable as defined by the lambda expression variable of type <see cref="IPathBuilder"/> that generated this pattern.
        /// For example, Match((path) =&gt; path.Pattern("user").Assign()) would generate the Cypher: MATCH path = (user).
        /// Note that whenever the path variable is required, always make this the last method called on the <see cref="IPathBuilder"/>.
        /// Without this method being called, the generated pattern would not be assigned to a path variable.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pathVariable">Use this to override the path variable (which is default to the lambda expression parameter).</param>
        /// <returns></returns>
        public static IPathExtent A(this IPath source, string pathVariable)
        {
            return PathExtensions.SharedAssign(source, pathVariable);
        }

        /// <summary>
        /// Assigns the entire pattern to the path variable.
        /// That is, P = (A)-[R]-(B), 
        /// where P is the path variable as defined by the lambda expression variable of type <see cref="IPathBuilder"/> that generated this pattern.
        /// For example, Match((path) =&gt; path.Pattern("user").Assign()) would generate the Cypher: MATCH path = (user).
        /// Note that whenever the path variable is required, always make this the last method called on the <see cref="IPathBuilder"/>.
        /// Without this method being called, the generated pattern would not be assigned to a path variable.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IPathExtent A(this IPathExtension source)
        {
            return PathExtensions.SharedAssign(source);
        }

        /// <summary>
        /// Assigns the entire pattern to the path variable.
        /// That is, P = (A)-[R]-(B), 
        /// where P is the path variable as defined by the lambda expression variable of type <see cref="IPathBuilder"/> that generated this pattern.
        /// For example, Match((path) =&gt; path.Pattern("user").Assign()) would generate the Cypher: MATCH path = (user).
        /// Note that whenever the path variable is required, always make this the last method called on the <see cref="IPathBuilder"/>.
        /// Without this method being called, the generated pattern would not be assigned to a path variable.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pathVariable">Use this to override the path variable (which is default to the lambda expression parameter).</param>
        /// <returns></returns>
        public static IPathExtent A(this IPathExtension source, string pathVariable)
        {
            return PathExtensions.SharedAssign(source, pathVariable);
        }
        #endregion
    }
}
