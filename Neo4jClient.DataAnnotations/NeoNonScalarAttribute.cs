using System;

namespace Neo4jClient.DataAnnotations
{
    /// <summary>
    ///     All structs and enums are assumed to be scalars that can be serialized to Neo4j.
    ///     Use this attribute on any struct or enum to indicate otherwise.
    ///     Alternatively, you can add such struct or enum or any other type to
    ///     <see cref="EntityService.KnownNonScalarTypes" /> list instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Enum)]
    public class NeoNonScalarAttribute : Attribute
    {
    }
}