using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neo4jClient.DataAnnotations.Utils;

namespace Neo4jClient.DataAnnotations
{
    /// <summary>
    ///     This class provides entity information.
    /// </summary>
    public sealed class EntityService : IEntityService
    {
        private readonly ConcurrentDictionary<Type, List<Type>> entityTypeToCovariantTypes
            = new ConcurrentDictionary<Type, List<Type>>();

        private readonly List<Type> knownNonScalars = new List<Type>();

        private readonly List<Type> knownScalars = new List<Type>
        {
            typeof(string), typeof(Uri), typeof(Guid),
            typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
            typeof(decimal), typeof(IntPtr), typeof(Version)
        };

        internal EntityService()
        {
        }

        internal EntityService(IEnumerable<Type> entityTypes) : this()
        {
            AddEntityTypes(entityTypes);
        }

        private ConcurrentDictionary<Type, EntityTypeInfo> entityTypeInfos { get; }
            = new ConcurrentDictionary<Type, EntityTypeInfo>();

        internal ConcurrentDictionary<Type, bool> processedScalarTypes { get; }
            = new ConcurrentDictionary<Type, bool>();

        public List<Type> KnownScalarTypes
        {
            get
            {
                lock (this)
                {
                    return knownScalars;
                }
            }
        }

        public List<Type> KnownNonScalarTypes
        {
            get
            {
                lock (this)
                {
                    return knownNonScalars;
                }
            }
        }

        public ICollection<Type> EntityTypes
        {
            get
            {
                lock (this)
                {
                    return entityTypeToCovariantTypes.Keys;
                }
            }
        }

        public void AddEntityTypes(IEnumerable<Type> types)
        {
            if (types != null)
                lock (this)
                {
                    foreach (var type in types) AddEntityType(type);
                }
        }

        /// <summary>
        ///     Registers a <see cref="System.Type" /> with Neo4jClient.DataAnnotations.
        ///     This method also autmatically adds contained complex types. However, derived and/or base types must be added
        ///     independently.
        /// </summary>
        /// <param name="entityType"></param>
        public void AddEntityType(Type entityType)
        {
            lock (this)
            {
                if (!EntityTypes.Contains(entityType ?? throw new ArgumentNullException(nameof(entityType))))
                {
                    entityTypeToCovariantTypes[entityType] = null;

                    //check if they have complex properties and add automatically
                    var complexTypes = entityType.GetProperties()?
                        .Select(p => p.PropertyType)
                        .Distinct()
                        .Where(t => t.IsComplex());

                    if (complexTypes != null)
                        foreach (var complexType in complexTypes)
                            AddEntityType(complexType);

                    //auto add base class too
                    if (entityType.GetTypeInfo().BaseType is Type baseType
                        && baseType != Defaults.ObjectType
                        && baseType != typeof(ValueType))
                    {
                        AddEntityType(baseType);

                        if (entityTypeToCovariantTypes.TryGetValue(baseType, out var derivedList) &&
                            derivedList != null)
                            //add it as a derived type for base type if already calculated
                            if (!derivedList.Contains(entityType))
                                derivedList.Add(entityType);
                    }
                }
            }
        }

        public void RemoveEntityType(Type entityType)
        {
            lock (this)
            {
                if (EntityTypes.Contains(entityType ?? throw new ArgumentNullException(nameof(entityType))))
                    entityTypeToCovariantTypes.TryRemove(entityType, out var val);
            }
        }

        public bool ContainsEntityType(Type entityType, bool includeBaseClasses = true)
        {
            lock (this)
            {
                var ret = EntityTypes.Contains(entityType ?? throw new ArgumentNullException(nameof(entityType)))
                          || entityType.GetTypeInfo().IsGenericType &&
                          EntityTypes.Contains(entityType.GetGenericTypeDefinition()) //search generics too
                          || includeBaseClasses && EntityTypes.Any(baseType => baseType.IsAssignableFrom(entityType)
                                                                               || baseType.IsGenericAssignableFrom(
                                                                                   entityType)) //optional
                    ;

                return ret;
            }
        }

        /// <summary>
        ///     Retrieves all types that can be assigned to the particular baseType (and not just its direct subclasses).
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public List<Type> GetDerivedEntityTypes(Type baseType)
        {
            if (baseType == null)
                throw new ArgumentNullException(nameof(baseType));

            lock (this)
            {
                if (!entityTypeToCovariantTypes.TryGetValue(baseType, out var derivedTypes)
                    || derivedTypes == null)
                {
                    derivedTypes = EntityTypes.Where(type => type != baseType
                                                             && (baseType.IsAssignableFrom(type) ||
                                                                 baseType.IsGenericAssignableFrom(type)))?.ToList();

                    if (derivedTypes != null) entityTypeToCovariantTypes[baseType] = derivedTypes;
                }

                derivedTypes = new List<Type>(derivedTypes); //protect the original copy
                if (!derivedTypes.Contains(baseType))
                    derivedTypes.Add(baseType);

                var list = derivedTypes;

                if (list != null)
                    //sort them
                    list.Sort((x, y) =>
                        x.IsGenericAssignableFrom(y) ? -1 :
                        y.IsGenericAssignableFrom(x) ? 1 : x.Name.CompareTo(y.Name));

                return list;
            }
        }

        public EntityTypeInfo GetEntityTypeInfo(Type type)
        {
            return GetEntityTypeInfo(type, false);
        }

        public EntityTypeInfo GetEntityTypeInfo(Type type, bool addToEntityTypes = false)
        {
            lock (this)
            {
                EntityTypeInfo info;
                if (!entityTypeInfos.TryGetValue(type ?? throw new ArgumentNullException(nameof(type)), out info))
                {
                    info = new EntityTypeInfo(type, this);
                    entityTypeInfos[type] = info;

                    if (addToEntityTypes)
                        AddEntityType(type);
                }

                return info;
            }
        }

        public List<EntityTypeInfo> GetDerivedEntityTypeInfos(Type baseType, bool getFromEntityTypesToo = false)
        {
            if (baseType == null)
                throw new ArgumentNullException(nameof(baseType));

            lock (this)
            {
                var existing = entityTypeInfos.Where(pair => baseType.IsAssignableFrom(pair.Key)
                                                             || baseType.IsGenericAssignableFrom(pair.Key))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);

                existing = existing ?? new Dictionary<Type, EntityTypeInfo>();

                if (existing.Count == 0 || !existing.ContainsKey(baseType))
                    existing[baseType] = GetEntityTypeInfo(baseType);

                if (getFromEntityTypesToo)
                {
                    //check entity types
                    var derivedTypes = GetDerivedEntityTypes(baseType);
                    foreach (var derivedType in derivedTypes)
                        if (!existing.ContainsKey(derivedType))
                            existing[derivedType] = GetEntityTypeInfo(derivedType);
                }

                var list = existing.Values.ToList();

                if (list != null)
                    //sort them
                    list.Sort((x, y) =>
                        x.Type.IsGenericAssignableFrom(y.Type) ? -1 :
                        y.Type.IsGenericAssignableFrom(x.Type) ? 1 : x.Type.Name.CompareTo(y.Type.Name));

                return list;
            }
        }
    }
}