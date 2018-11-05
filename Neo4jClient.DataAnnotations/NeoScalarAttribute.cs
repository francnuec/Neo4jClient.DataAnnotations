using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations
{
    /// <summary>
    /// All classes and interfaces are assumed to be non-scalars (i.e. navigational) that cannot be serialized to Neo4j.
    /// Use this attribute on any class or interface to indicate otherwise.
    /// Alternatively, you can add such class or interface or any other type to <see cref="IEntityService.KnownScalarTypes"/> list instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class NeoScalarAttribute : Attribute
    {
    }
}
