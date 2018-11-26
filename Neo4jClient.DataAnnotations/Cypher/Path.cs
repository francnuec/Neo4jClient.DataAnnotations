using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.Linq;
using Neo4jClient.Cypher;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public abstract class Path : Annotated, IAnnotated, IPathable,
        IPathExtent, IPath, IPathExtension,
        IPatternedPath, IPatternedPathExtension
    {
        public Path(Path existing)
            : this(existing?.Builder)
        {
            if (existing == null)
                throw new ArgumentNullException(nameof(existing));

            //PatternBuildStrategy = existing.PatternBuildStrategy;

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

        public Path(IPathBuilder builder)
            : this(builder, builder.AnnotationsContext)
        {
        }

        public Path(IPathBuilder builder, AnnotationsContext context)
            : base(builder.CypherQuery, context)
        {
            Builder = builder;
            //PatternBuildStrategy = builder.PatternBuildStrategy;
        }

        public List<Pattern> Patterns { get; } = new List<Pattern>();

        public IPattern Pattern => Patterns.Count > 0 ? Patterns[Patterns.Count - 1] : null;

        public IPathExtension Extension => this;

        //public IPath Origin => this;

        public IPathBuilder Builder { get; }

        public IPathExtent Current => this;

        protected override string InternalBuild()
        {
            var builder = new StringBuilder();

            var patternText = Patterns.Select(p => p.Build(ref CypherQuery)).Aggregate((pattern, extension) => pattern + extension);

            builder.Append(patternText);

            return builder.ToString();
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

        public Path(IPathBuilder builder)
            : base(builder)
        {
            Init();
        }

        public Path(IPathBuilder builder, AnnotationsContext context)
            : base(builder, context)
        {
            Init();
        }

        void Init()
        {
            //Add a new pattern
            Patterns.Add(new Pattern(CypherQuery, this, AnnotationsContext));
        }
    }
}
