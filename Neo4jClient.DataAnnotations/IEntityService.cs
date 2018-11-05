using Neo4jClient.DataAnnotations.Utils;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;

namespace Neo4jClient.DataAnnotations
{
    public interface IEntityService
    {
        ICollection<Type> EntityTypes { get; }
        List<Type> KnownNonScalarTypes { get; }
        List<Type> KnownScalarTypes { get; }

        void AddEntityTypes(IEnumerable<Type> types);
        void AddEntityType(Type entityType);
        bool ContainsEntityType(Type entityType, bool includeBaseClasses = true);
        List<EntityTypeInfo> GetDerivedEntityTypeInfos(Type baseType, bool getFromEntityTypesToo = false);
        List<Type> GetDerivedEntityTypes(Type baseType);
        EntityTypeInfo GetEntityTypeInfo(Type type);
        void RemoveEntityType(Type entityType);
    }
}