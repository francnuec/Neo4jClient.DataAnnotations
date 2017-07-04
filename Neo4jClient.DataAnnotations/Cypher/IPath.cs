using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public interface IPathable
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        IPath Origin { get; }
    }

    public interface IPathBuilder : IPathable, IAnnotated
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        Expression<Func<IPathBuilder, IPathable>> Expression { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        string PathParameter { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsShortestPath { get; }
    }

    public interface IPathExtent : IPathable, IAnnotated
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        IPattern Pattern { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        IPathExtension Extension { get; }
    }

    public interface IPath : IPathExtent, IAnnotated
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        IPathBuilder Builder { get; }

        IPathFinisher Finisher { get; }
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
        [EditorBrowsable(EditorBrowsableState.Never)]
        IPathExtent Current { get;  }
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


    public interface IPathFinisher : IPathable, IAnnotated
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool AssignPathParameter { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool FindShortestPath { get; }
    }
}
