using System.Collections.Generic;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class SpecialNodePath
    {
        public List<object> Path { get; set; }
        public SpecialNode Node { get; set; }

        public SpecialNodePath(List<object> path, SpecialNode node)
        {
            Path = path;
            Node = node;
        }
    }
}
