using System;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class NeoScalarAttribute : Attribute
    {

    }
}
