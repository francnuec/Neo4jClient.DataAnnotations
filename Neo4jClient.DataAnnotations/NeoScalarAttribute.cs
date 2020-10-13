using System;

namespace Neo4jClient.DataAnnotations
{
    /// <summary>
    ///     All classes and interfaces are assumed to be non-scalars (i.e. navigational) that cannot be serialized to Neo4j.
    ///     Use this attribute on any class or interface to indicate otherwise.
    ///     Alternatively, you can add such class or interface or any other type to
    ///     <see cref="EntityService.KnownScalarTypes" /> list instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class NeoScalarAttribute : Attribute
    {
    }
}