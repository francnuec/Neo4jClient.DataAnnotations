﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Utils;

namespace Neo4jClient.DataAnnotations //.Cypher
{
    public static partial class CypherPathExtensions
    {
        #region Pattern

        internal static IPatternedPath SharedPattern(this IPathBuilder source,
            string A, string R, string B, RelationshipDirection? dir)
        {
            return SharedPattern<CypherObject, CypherObject, CypherObject>(source, A, R, B, dir);
        }

        internal static IPatternedPath<TANode> SharedPattern<TANode>
            (this IPathBuilder source, string A, string R, string B, RelationshipDirection? dir)
        {
            return (IPatternedPath<TANode>)SharedPattern<TANode, CypherObject, CypherObject>(source, A, R, B, dir);
        }

        internal static IPatternedPath<TANode, TBNode> SharedPattern<TANode, TBNode>
            (this IPathBuilder source, string A, string R, string B, RelationshipDirection? dir)
        {
            return (IPatternedPath<TANode, TBNode>)SharedPattern<TANode, CypherObject, TBNode>(source, A, R, B, dir);
        }

        internal static IPatternedPath<TANode, TBNode> SharedPattern<TANode, TBNode>
        (this IPathBuilder source, Expression<Func<TANode, TBNode>> relationship, string R, string B,
            RelationshipDirection? dir)
        {
            return SharedPattern(source, relationship, null, R, B, dir);
        }

        internal static IPatternedPath<TANode, TBNode> SharedPattern<TANode, TBNode>
        (this IPathBuilder source, Expression<Func<TANode, TBNode>> relationship, string A, string R, string B,
            RelationshipDirection? dir)
        {
            return (IPatternedPath<TANode, TBNode>)SharedPattern<TANode, CypherObject, TBNode>(source, relationship, A,
                R, B, dir);
        }

        internal static IPatternedPath<TANode, TBNode> SharedPattern<TANode, TBNode>
        (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string R, string B,
            RelationshipDirection? dir)
        {
            return SharedPattern(source, relationship, null, R, B, dir);
        }

        internal static IPatternedPath<TANode, TBNode> SharedPattern<TANode, TBNode>
        (this IPathBuilder source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string A, string R,
            string B, RelationshipDirection? dir)
        {
            return (IPatternedPath<TANode, TBNode>)SharedPattern<TANode, CypherObject, TBNode>(source, relationship, A,
                R, B, dir);
        }


        /// <summary>
        ///     This method should always be called for new patterns.
        /// </summary>
        /// <typeparam name="TANode"></typeparam>
        /// <typeparam name="TRel"></typeparam>
        /// <typeparam name="TBNode"></typeparam>
        /// <param name="source"></param>
        /// <param name="A"></param>
        /// <param name="R"></param>
        /// <param name="B"></param>
        /// <param name="dir"></param>
        /// <param name="testARBForNull"></param>
        /// <returns></returns>
        internal static IPatternedPath SharedPattern<TANode, TRel, TBNode>
        (this IPathable source, string A, string R, string B, RelationshipDirection? dir,
            bool testARBForNull = true)
        {
            if (testARBForNull && string.IsNullOrWhiteSpace(A) && string.IsNullOrWhiteSpace(R) &&
                string.IsNullOrWhiteSpace(B))
                //you must provide at least one variable
                throw new InvalidOperationException(Messages.NullARBVariablesError);

            var builder = source as IPathBuilder;
            var oldPath = source as Path;

            Path path = null;

            if (builder != null)
                path = new Path<TANode, TRel, TBNode>(builder);
            else if (oldPath != null)
                //create new path as we may have new entity types.
                path = new Path<TANode, TRel, TBNode>(oldPath);
            else
                throw new ArgumentException(Messages.PathableNotRecognizedError, nameof(source));

            var pattern = path.Pattern as Pattern;

            pattern.AVariable = A;
            pattern.AType = typeof(TANode);

            pattern.RVariable = R;
            pattern.RType = typeof(TRel);

            pattern.BVariable = B;
            pattern.BType = typeof(TBNode);

            pattern.Direction = dir;

            return path;
        }

        internal static IPatternedPath SharedPattern<TANode, TRel, TBNode>
        (this IPathable source, LambdaExpression relationship, string A, string R, string B, RelationshipDirection? dir,
            bool testARBForNull = false)
        {
            //for the sake of our tests, leave this here, and don't fall for the VS intellisense refactoring bait.
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));

            var path = SharedPattern<TANode, TRel, TBNode>(source, A, R, B, dir,
                testARBForNull); //set the other properties

            var pattern = path.Pattern as Pattern;
            pattern.ABSelector = relationship;
            pattern.Direction = dir;

            return path;
        }

        internal static IPatternedPath SharedPattern<TANode, TRel, TBNode>
        (this IPathable source, LambdaExpression beginRelationship,
            LambdaExpression endRelationship, string A, string R, string B, RelationshipDirection? dir,
            bool testARBForNull = false)
        {
            //for the sake of our tests, leave this here, and don't fall for the VS intellisense refactoring bait.
            if (beginRelationship == null) throw new ArgumentNullException(nameof(beginRelationship));

            var path = SharedPattern<TANode, TRel, TBNode>(source, A, R, B, dir,
                testARBForNull); //set the other properties
            var pattern = path.Pattern as Pattern;

            pattern.ARSelector = beginRelationship;

            pattern.RBSelector = endRelationship;

            pattern.Direction = dir;

            return path;
        }

        #endregion

        #region Label

        internal static IPatternedPath SharedLabel
        (this IPath source, IEnumerable<string> A, IEnumerable<string> R, IEnumerable<string> B,
            bool replaceA, bool replaceR, bool replaceB)
        {
            var pattern = source.Pattern as Pattern;

            pattern.ALabels = A;
            pattern.RTypes = R;
            pattern.BLabels = B;

            pattern.UseGivenALabelsOnly = replaceA;
            pattern.UseGivenRTypesOnly = replaceR;
            pattern.UseGivenBLabelsOnly = replaceB;

            return (IPatternedPath)source;
        }

        internal static IPatternedPathExtension SharedLabel
        (this IPathExtension source, IEnumerable<string> R, IEnumerable<string> B,
            bool replaceR, bool replaceB)
        {
            return (IPatternedPathExtension)SharedLabel(source as IPath, null, R, B, false, replaceR, replaceB);
        }

        #endregion

        #region Hop

        public static IPatternedPath SharedHop<TANode, TRel, TBNode>
            (this IPath source, int? from, int? to)
        {
            var pattern = source.Pattern as Pattern;
            pattern.RHops = new Tuple<int?, int?>(from, to);

            return (IPatternedPath)source;
        }

        public static IPatternedPathExtension SharedHop<TANode, TRel, TBNode>
            (this IPathExtension source, int? from, int? to)
        {
            return (IPatternedPathExtension)SharedHop<TANode, TRel, TBNode>(source as IPath, from, to);
        }

        #endregion

        #region Constrain

        internal static IPatternedPath SharedConstrain<TANode, TRel, TBNode>
        (this IPath source,
            Expression<Func<TANode, bool>> A,
            Expression<Func<TRel, bool>> R,
            Expression<Func<TBNode, bool>> B)
        {
            var pattern = source.Pattern as Pattern;

            if (A != null && pattern.AProperties != null)
                throw new InvalidOperationException(string.Format(Messages.PropsAndConstraintsClashError, nameof(A)));

            if (R != null && pattern.RProperties != null)
                throw new InvalidOperationException(string.Format(Messages.PropsAndConstraintsClashError, nameof(R)));

            if (B != null && pattern.BProperties != null)
                throw new InvalidOperationException(string.Format(Messages.PropsAndConstraintsClashError, nameof(B)));

            pattern.AConstraints = A;
            pattern.RConstraints = R;
            pattern.BConstraints = B;

            return (IPatternedPath)source;
        }

        internal static IPatternedPathExtension SharedConstrain<TANode, TRel, TBNode>
        (this IPathExtension source,
            Expression<Func<TRel, bool>> R,
            Expression<Func<TBNode, bool>> B)
        {
            return (IPatternedPathExtension)SharedConstrain<TANode, TRel, TBNode>(source as IPath, null, R, B);
        }

        #endregion

        #region Prop

        internal static IPatternedPath SharedProp<TANode, TRel, TBNode>
        (this IPath source,
            Expression<Func<object>> A,
            Expression<Func<object>> R,
            Expression<Func<object>> B)
        {
            var pattern = source.Pattern as Pattern;

            if (A != null && pattern.AConstraints != null)
                throw new InvalidOperationException(string.Format(Messages.PropsAndConstraintsClashError, nameof(A)));

            if (R != null && pattern.RConstraints != null)
                throw new InvalidOperationException(string.Format(Messages.PropsAndConstraintsClashError, nameof(R)));

            if (B != null && pattern.BConstraints != null)
                throw new InvalidOperationException(string.Format(Messages.PropsAndConstraintsClashError, nameof(B)));

            pattern.AProperties = A;
            pattern.RProperties = R;
            pattern.BProperties = B;

            return (IPatternedPath)source;
        }

        internal static IPatternedPathExtension SharedProp<TANode, TRel, TBNode>
        (this IPathExtension source,
            Expression<Func<object>> R,
            Expression<Func<object>> B)
        {
            return (IPatternedPathExtension)SharedProp<TANode, TRel, TBNode>(source as IPath, null, R, B);
        }

        #endregion

        #region Type

        internal static IPatternedPath SharedType
            (this IPath source, Type A, Type R, Type B)
        {
            var pattern = source.Pattern as Pattern;

            Func<Type, Type, bool> assertTypeAssignment = (oldType, newType) =>
            {
                if (oldType != null && newType != null && !oldType.IsAssignableFrom(newType))
                    //the old type must be assignable from the new type
                    throw new InvalidOperationException(string.Format(Messages.UnassignableTypeError, newType,
                        oldType));

                return true;
            };

            if (A != null && assertTypeAssignment(pattern.AType, A)) pattern.AType = A;

            if (R != null && assertTypeAssignment(pattern.RType, R)) pattern.RType = R;

            if (B != null && assertTypeAssignment(pattern.BType, B)) pattern.BType = B;

            return (IPatternedPath)source;
        }

        internal static IPatternedPathExtension SharedType
            (this IPathExtension source, Type R, Type B)
        {
            return (IPatternedPathExtension)SharedType(source as IPath, null, R, B);
        }

        #endregion

        #region AlreadyBound

        internal static IPatternedPath SharedAlreadyBound
            (this IPath source, bool? A, bool? R, bool? B)
        {
            var pattern = source.Pattern as Pattern;

            pattern.AIsAlreadyBound = A;
            pattern.RIsAlreadyBound = R;
            pattern.BIsAlreadyBound = B;

            return (IPatternedPath)source;
        }

        internal static IPatternedPathExtension SharedAlreadyBound
            (this IPathExtension source, bool? R, bool? B)
        {
            return (IPatternedPathExtension)SharedAlreadyBound(source as IPath, null, R, B);
        }

        #endregion

        #region Extend

        internal static IPatternedPathExtension SharedExtend(this IPath source,
            string R, string B, RelationshipDirection? dir)
        {
            return SharedExtend<CypherObject, CypherObject, CypherObject>(source, R, B, dir);
        }

        internal static IPatternedPathExtension<TBNode> SharedExtend<TBNode>
            (this IPath source, string R, string B, RelationshipDirection? dir)
        {
            return (IPatternedPathExtension<TBNode>)SharedExtend<CypherObject, CypherObject, TBNode>(source, R, B,
                dir);
        }

        internal static IPatternedPathExtension<TANode, TBNode> SharedExtend<TANode, TBNode>
        (this IPath source, Expression<Func<TANode, TBNode>> relationship, string R, string B,
            RelationshipDirection? dir)
        {
            return (IPatternedPathExtension<TANode, TBNode>)SharedExtend<TANode, CypherObject, TBNode>(source,
                relationship, R, B, dir);
        }

        internal static IPatternedPathExtension<TANode, TBNode> SharedExtend<TANode, TBNode>
        (this IPath source, Expression<Func<TANode, IEnumerable<TBNode>>> relationship, string R, string B,
            RelationshipDirection? dir)
        {
            return (IPatternedPathExtension<TANode, TBNode>)SharedExtend<TANode, CypherObject, TBNode>(source,
                relationship, R, B, dir);
        }


        private static void AssertLastBMatchesNewA(IPath source, Type aType, out Pattern oldPattern)
        {
            oldPattern = source.Pattern as Pattern;
            var lastBType = oldPattern.BType;

            if (aType == Defaults.CypherObjectType
                || lastBType == null
                || lastBType == Defaults.CypherObjectType)
                //we have no case here
                return;

            if (lastBType != aType)
                throw new InvalidOperationException
                    (string.Format(Messages.ExtendLastBNewAMismatchError, lastBType.FullName, aType.FullName));
        }

        private static Path ProcessSharedExtend<TANode, TRel, TBNode>(IPath source, Func<Path> getPath)
        {
            Pattern oldPattern;
            AssertLastBMatchesNewA(source, typeof(TANode), out oldPattern);

            //pass it on to sharedpattern method
            var path = getPath();

            //make sure to indicate new pattern as an extension
            FinishPathExtensionSet(path.Pattern as Pattern, oldPattern);

            return path;
        }

        private static void FinishPathExtensionSet(Pattern current, Pattern old)
        {
            current.IsExtension = true;

            var lastBType = old.BType;

            if (current.aType == Defaults.CypherObjectType
                && lastBType != null)
                current.AType = lastBType;

            var oldBVar = old.BVariable;
            var currentAVar = current.AVariable;

            if (!old.BVarIsAuto)
            {
                //swap
                currentAVar = oldBVar;
                oldBVar = null;
            }
            else if (!current.AVarIsAuto)
            {
                //swap
                //replace the old with the new
                oldBVar = currentAVar;
                currentAVar = null;
            }
            else
            {
                oldBVar = null;
                currentAVar = null;
            }

            if (currentAVar != null)
                current.AVariable = currentAVar;

            if (oldBVar != null)
                old.BVariable = oldBVar;
        }


        internal static IPatternedPathExtension SharedExtend<TANode, TRel, TBNode>
            (this IPath source, string R, string B, RelationshipDirection? dir)
        {
            //pass it on to sharedpattern method
            return ProcessSharedExtend<TANode, TRel, TBNode>
            (source, () => (Path)SharedPattern<TANode, TRel, TBNode>
                (source, null, R, B, dir, false));
        }

        internal static IPatternedPathExtension SharedExtend<TANode, TRel, TBNode>
            (this IPath source, LambdaExpression relationship, string R, string B, RelationshipDirection? dir)
        {
            return ProcessSharedExtend<TANode, TRel, TBNode>
            (source, () => (Path)SharedPattern<TANode, TRel, TBNode>
                (source, relationship, null, R, B, dir));
        }

        internal static IPatternedPathExtension SharedExtend<TANode, TRel, TBNode>
        (this IPath source, LambdaExpression beginRelationship, LambdaExpression endRelationship,
            string R, string B, RelationshipDirection? dir)
        {
            return ProcessSharedExtend<TANode, TRel, TBNode>
            (source, () => (Path)SharedPattern<TANode, TRel, TBNode>
                (source, beginRelationship, endRelationship, null, R, B, dir));
        }

        #endregion

        #region Shortest

        public static IPathExtent SharedShortest(this IPath source)
        {
            var path = source as Path;
            (path.Builder as PathBuilder).FindShortestPath = true;

            return SharedAssign(path);
        }

        public static IPathExtent SharedShortest(this IPath source, string pathVariable)
        {
            var path = SharedShortest(source);

            return SharedAssign(path, pathVariable);
        }

        #endregion

        #region Assign

        internal static IPathExtent SharedAssign(this IPathExtent source)
        {
            var path = source as Path;
            (path.Builder as PathBuilder).AssignPathVariable = true;

            return source;
        }

        internal static IPathExtent SharedAssign(this IPathExtent source, string pathVariable)
        {
            source = SharedAssign(source);
            (source.Builder as PathBuilder).PathVariable =
                pathVariable ?? throw new ArgumentNullException(nameof(pathVariable));

            return source;
        }

        #endregion
    }
}