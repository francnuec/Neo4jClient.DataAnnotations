using System;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public class CypherObject : Dictionary<string, object>, IEquatable<object>
    {
        ///// <summary>
        ///// Pseudo-casts this object as a certain type so as to use its properties directly
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //public T As<T>()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
