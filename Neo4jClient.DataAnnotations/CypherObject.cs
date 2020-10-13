using System;
using System.Collections.Generic;

namespace Neo4jClient.DataAnnotations //.Cypher
{
    public class CypherObject : Dictionary<string, object>, IEquatable<object>
    {
        protected internal CypherObject()
        {
        }
    }
}