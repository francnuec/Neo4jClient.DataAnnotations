﻿using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations
{
    /// <summary>
    /// All structs and enums are assumed to be scalars that can be serialized to Neo4j. 
    /// Use this attribute on any struct or enum to indicate otherwise.
    /// Alternatively, you can add such struct or enum or any other type to <see cref="IEntityService.KnownNonScalarTypes"/> list instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false)]
    public class NeoNonScalarAttribute : Attribute
    {
    }
}