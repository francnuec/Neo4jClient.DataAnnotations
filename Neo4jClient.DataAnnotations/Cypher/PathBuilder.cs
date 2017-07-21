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
            PathParameter = expression?.Parameters[0].Name;
        }

        public PathBuilder(ICypherFluentQuery query, IPathExtent path, string pathParameter)
            : this(query)
        {
            Path = path;
            PathParameter = pathParameter;
        }

        public IPathExtent Path { get; set; }

        public PatternBuildStrategy PatternBuildStrategy { get; set; }

        public string PathParameter { get; set; }

        public bool AssignPathParameter { get; protected internal set; }

        public bool FindShortestPath { get; protected internal set; }

        //public IPath Origin => throw new NotImplementedException();

        public override string Build()
        {
            var builder = new StringBuilder();

            var pathText = Path?.Build();

            var pathParameter = PathParameter;

            if (AssignPathParameter && pathParameter != null)
            {
                builder.Append($"{pathParameter}=");
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
