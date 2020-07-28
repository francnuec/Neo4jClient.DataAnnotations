﻿using System.ComponentModel;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public interface IPathable
    {
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //IPath Origin { get; }
    }

    public interface IPathBuilder : IPathable, IAnnotated
    {
        IPathExtent Path { get; }

        string PathVariable { get; }

        PropertiesBuildStrategy PatternBuildStrategy { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool AssignPathVariable { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool FindShortestPath { get; }
    }

    public interface IPathExtent : IPathable, IAnnotated
    {
        IPathBuilder Builder { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        IPattern Pattern { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        IPathExtension Extension { get; }
    }


    public interface IPath : IPathExtent, IAnnotated
    {
    }

    public interface IPatternedPath : IPath
    {
    }

    public interface IPatternedPath<out TANode> : IPath
    {
    }

    public interface IPatternedPath<out TANode, out TBNode> : IPath
    {
    }

    public interface IPatternedPath<out TANode, out TRel, out TBNode> : IPath
    {
    }


    public interface IPathExtension : IPathExtent, IAnnotated
    {
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //IPathExtent Current { get;  }
    }

    public interface IPatternedPathExtension : IPathExtension
    {
    }

    public interface IPatternedPathExtension<out TBNode> : IPathExtension
    {
    }

    public interface IPatternedPathExtension<out TANode, out TBNode> : IPathExtension
    {
    }

    public interface IPatternedPathExtension<out TANode, out TRel, out TBNode> : IPathExtension
    {
    }
}