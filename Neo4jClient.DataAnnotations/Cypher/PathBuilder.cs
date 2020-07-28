﻿using System;
using System.Linq.Expressions;
using System.Text;
using Neo4jClient.Cypher;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public class PathBuilder : Annotated, IPathBuilder //, IPathFinish
    {
        public PathBuilder(ICypherFluentQuery query, AnnotationsContext context = null)
            : base(query, context)
        {
        }

        public PathBuilder(ICypherFluentQuery query,
            Expression<Func<IPathBuilder, IPathExtent>> expression,
            AnnotationsContext context = null)
            : this(query, context)
        {
            Path = expression?.Compile().Invoke(this);
            PathVariable = expression?.Parameters[0].Name;
        }

        public PathBuilder(ICypherFluentQuery query, IPathExtent path,
            string pathVariable, AnnotationsContext context)
            : this(query, context)
        {
            Path = path;
            PathVariable = pathVariable;
        }

        public IPathExtent Path { get; protected internal set; }

        public PropertiesBuildStrategy PatternBuildStrategy { get; set; }

        public string PathVariable { get; protected internal set; }

        public bool AssignPathVariable { get; protected internal set; }

        public bool FindShortestPath { get; protected internal set; }

        //public IPath Origin => throw new NotImplementedException();

        protected override string InternalBuild()
        {
            var builder = new StringBuilder();

            var pathText = Path?.Build(ref CypherQuery);

            var pathVariable = PathVariable;

            if (AssignPathVariable && pathVariable != null) builder.Append($"{pathVariable}=");

            if (FindShortestPath)
                builder.Append($"shortestPath({pathText})");
            else
                builder.Append(pathText);

            return builder.ToString();
        }

        public override string ToString()
        {
            try
            {
                return $"PathBuilder: {InternalBuild()}";
            }
            catch
            {
            }

            return base.ToString();
        }
    }
}