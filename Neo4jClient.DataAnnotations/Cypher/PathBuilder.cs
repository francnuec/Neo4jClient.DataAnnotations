using Neo4jClient.Cypher;
using System;
using System.Text;
using System.Linq.Expressions;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public class PathBuilder : Annotated, IPathBuilder //, IPathFinish
    {
        public PathBuilder(ICypherFluentQuery query)
            :base(query)
        {

        }

        public PathBuilder(ICypherFluentQuery query, Expression<Func<IPathBuilder, IPathExtent>> expression)
            : this(query)
        {
            Path = expression?.Compile().Invoke(this);
            PathVariable = expression?.Parameters[0].Name;
        }

        public PathBuilder(ICypherFluentQuery query, IPathExtent path, string pathVariable)
            : this(query)
        {
            Path = path;
            PathVariable = pathVariable;
        }

        public IPathExtent Path { get; set; }

        public PropertiesBuildStrategy PatternBuildStrategy { get; set; }

        public string PathVariable { get; set; }

        public bool AssignPathVariable { get; protected internal set; }

        public bool FindShortestPath { get; protected internal set; }

        //public IPath Origin => throw new NotImplementedException();

        public override string Build()
        {
            var builder = new StringBuilder();

            var pathText = Path?.Build();

            var pathVariable = PathVariable;

            if (AssignPathVariable && pathVariable != null)
            {
                builder.Append($"{pathVariable}=");
            }

            if (FindShortestPath)
            {
                builder.Append($"shortestPath({pathText})");
            }
            else
            {
                builder.Append(pathText);
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            try
            {
                return $"PathBuilder: {Build()}";
            }
            catch
            {

            }

            return base.ToString();
        }
    }
}
