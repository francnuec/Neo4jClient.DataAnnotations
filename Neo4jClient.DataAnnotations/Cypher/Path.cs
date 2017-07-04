using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public abstract class Path : Annotated, IAnnotated, IPathable,
        IPathBuilder, IPathExtent, IPath, IPathExtension, IPathFinisher,
        IPatternedPath, IPatternedPathExtension
    {
        public Path(Path existing)
            : base(existing)
        {
            Expression = existing.Expression;

            //mutate by adding all the patterns from previous path to this one
            //and also change the path for the exisiting patterns to this
            foreach (var pattern in existing.Patterns)
            {
                pattern.Path = this;

                Patterns.Add(pattern);
            }

            ////finally add a new pattern
            //Patterns.Add(new Pattern(this, InternalCypherQuery));
        }

        public Path(Expression<Func<IPathBuilder, IPathable>> expression, ICypherFluentQuery query)
            : base(query)
        {
            Expression = expression;
        }

        protected internal List<Pattern> Patterns { get; } = new List<Pattern>();

        public IPattern Pattern => Patterns.Count > 0 ? Patterns[Patterns.Count - 1] : null;

        public IPathExtension Extension => this;

        public IPath Origin => this;

        public IPathBuilder Builder => this;

        public IPathExtent Current => this;

        public Expression<Func<IPathBuilder, IPathable>> Expression { get; }

        public string PathParameter { get; protected internal set; }

        public bool IsShortestPath => FindShortestPath;

        public IPathFinisher Finisher => this;

        public bool AssignPathParameter { get; protected internal set; }

        public bool FindShortestPath { get; protected internal set; }

        public override string Build()
        {
            throw new NotImplementedException();
        }
    }

    public class DummyPath: Path
    {
        public DummyPath(Path existing)
            : base(existing)
        {

        }

        public DummyPath(Expression<Func<IPathBuilder, IPathable>> expression, ICypherFluentQuery query)
            : base(expression, query)
        {

        }
    }

    public class Path<TANode, TRel, TBNode> : Path,
        IPatternedPath<TANode>, IPatternedPathExtension<TBNode>,
        IPatternedPath<TANode, TBNode>, IPatternedPathExtension<TANode, TBNode>,
        IPatternedPath<TANode, TRel, TBNode>, IPatternedPathExtension<TANode, TRel, TBNode>
    {
        public Path(Path existing)
            : base(existing)
        {
            Init();
        }

        public Path(Expression<Func<IPathBuilder, IPathable>> expression, ICypherFluentQuery query)
            : base(expression, query)
        {
            Init();
        }

        void Init()
        {
            //Add a new pattern
            Patterns.Add(new Pattern(this, InternalCypherQuery));
        }
    }
}
