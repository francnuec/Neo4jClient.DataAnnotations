using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Concurrent;
using Neo4jClient.DataAnnotations.Serialization;

namespace Neo4jClient.DataAnnotations
{
    public class Neo4jAnnotations
    {
        private Neo4jAnnotations()
        {

        }

        private static ConcurrentDictionary<Type, EntityTypeInfo> entityTypeInfo { get; set; }
            = new ConcurrentDictionary<Type, EntityTypeInfo>();

        private static ConcurrentDictionary<Type, object> entityTypes 
            = new ConcurrentDictionary<Type, object>();

        public static List<Type> KnownScalarTypes { get; } = new List<Type>()
        {
            typeof(string), typeof(Uri), typeof(Guid),
            typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
            typeof(Decimal), typeof(IntPtr), typeof(Version)
        };

        public static List<Type> KnownNonScalarTypes { get; } = new List<Type>();

        internal static ConcurrentDictionary<Type, bool> processedScalarTypes { get; }
        = new ConcurrentDictionary<Type, bool>();

        public static ICollection<Type> EntityTypes { get { return entityTypes.Keys; } }


        /// <summary>
        /// Replaces the <see cref="GraphClient.DefaultJsonContractResolver"/> 
        /// with <see cref="EntityResolver"/> in order to handle entity JSON serialization and deserialization, while making other necessary configuration changes.
        /// NOTE: THIS METHOD SHOULD BE CALLED ONLY ONCE, AND BEFORE ANY CODE THAT USES <see cref="Neo4jClient"/> IS CALLED. 
        /// If you already use your own custom resolver, do not call this method, 
        /// but call the <see cref="RegisterWithConverter()"/> method instead to use the <see cref="EntityConverter"/>.
        /// </summary>
        public static void RegisterWithResolver()
        {
            RegisterWithResolver(null, new EntityResolver());
        }

        /// <summary>
        /// Replaces the <see cref="GraphClient.DefaultJsonContractResolver"/> 
        /// with <see cref="EntityResolver"/> in order to handle entity JSON serialization and deserialization, while making other necessary configuration changes.
        /// NOTE: THIS METHOD SHOULD BE CALLED ONLY ONCE, AND BEFORE ANY CODE THAT USES <see cref="Neo4jClient"/> IS CALLED. 
        /// If you already use your own custom resolver, do not call this method, 
        /// but call the <see cref="RegisterWithConverter(IEnumerable{Type})"/> method instead to use the <see cref="EntityConverter"/>.
        /// </summary>
        /// <param name="entityTypes">All entity types (i.e., model classes) used in your project. 
        /// Ideally, this library needs to know all your entity types early on so as to best determine how to construct the class hierarchies. 
        /// For simple classes with no inheritances, you may probably skip adding any entity types. 
        /// But if your models have derived types, especially for complex type models, it's best to input all entity types at the point of registration.
        /// </param>
        public static void RegisterWithResolver(IEnumerable<Type> entityTypes)
        {
            RegisterWithResolver(entityTypes, new EntityResolver());
        }

        /// <summary>
        /// Replaces the <see cref="GraphClient.DefaultJsonContractResolver"/> 
        /// with <see cref="EntityResolver"/> in order to handle entity JSON serialization and deserialization, while making other necessary configuration changes.
        /// NOTE: THIS METHOD SHOULD BE CALLED ONLY ONCE, AND BEFORE ANY CODE THAT USES <see cref="Neo4jClient"/> IS CALLED. 
        /// If you already use your own custom resolver, do not call this method, 
        /// but call the <see cref="RegisterWithConverter(EntityConverter)"/> method instead to use the <see cref="EntityConverter"/>.
        /// </summary>
        /// <param name="resolver">An instance of <see cref="EntityResolver"/> to use. An instance from a derived class is permitted.</param>
        public static void RegisterWithResolver(EntityResolver resolver)
        {
            RegisterWithResolver(null, resolver);
        }

        /// <summary>
        /// Replaces the <see cref="GraphClient.DefaultJsonContractResolver"/> 
        /// with <see cref="EntityResolver"/> in order to handle entity JSON serialization and deserialization, while making other necessary configuration changes.
        /// NOTE: THIS METHOD SHOULD BE CALLED ONLY ONCE, AND BEFORE ANY CODE THAT USES <see cref="Neo4jClient"/> IS CALLED. 
        /// If you already use your own custom resolver, do not call this method, 
        /// but call the <see cref="RegisterWithConverter(IEnumerable{Type}, EntityConverter)"/> method instead to use the <see cref="EntityConverter"/>.
        /// </summary>
        /// <param name="entityTypes">All entity types (i.e., model classes) used in your project. 
        /// Ideally, this library needs to know all your entity types early on so as to best determine how to construct the class hierarchies. 
        /// For simple classes with no inheritances, you may probably skip adding any entity types. 
        /// But if your models have derived types, especially for complex type models, it's best to input all entity types at the point of registration.
        /// </param>
        /// <param name="resolver">An instance of <see cref="EntityResolver"/> to use. An instance from a derived class is permitted.</param>
        public static void RegisterWithResolver(IEnumerable<Type> entityTypes, EntityResolver resolver)
        {
            InternalRegister(entityTypes, resolver, null);
        }

        /// <summary>
        /// Adds <see cref="EntityConverter"/> to the <see cref="GraphClient.DefaultJsonConverters"/> array in order to handle entity JSON serialization and deserialization, 
        /// while making other necessary configuration changes.
        /// NOTE: THIS METHOD SHOULD BE CALLED ONLY ONCE, AND BEFORE ANY CODE THAT USES <see cref="Neo4jClient"/> IS CALLED. 
        /// If you think the converter would undesirably interfere with your JSON serialization, and you do not already use your own custom resolver,  
        /// then call the <see cref="RegisterWithResolver()"/> method instead to use the <see cref="EntityResolver"/>.
        /// </summary>
        public static void RegisterWithConverter()
        {
            RegisterWithConverter(null, new EntityConverter());
        }

        /// <summary>
        /// Adds <see cref="EntityConverter"/> to the <see cref="GraphClient.DefaultJsonConverters"/> array in order to handle entity JSON serialization and deserialization, 
        /// while making other necessary configuration changes.
        /// NOTE: THIS METHOD SHOULD BE CALLED ONLY ONCE, AND BEFORE ANY CODE THAT USES <see cref="Neo4jClient"/> IS CALLED. 
        /// If you think the converter would undesirably interfere with your JSON serialization, and you do not already use your own custom resolver,  
        /// then call the <see cref="RegisterWithResolver(IEnumerable{Type})"/> method instead to use the <see cref="EntityResolver"/>.
        /// </summary>
        /// <param name="entityTypes">All entity types (i.e., model classes) used in your project. 
        /// Ideally, this library needs to know all your entity types early on so as to best determine how to construct the class hierarchies. 
        /// For simple classes with no inheritances, you may probably skip adding any entity types. 
        /// But if your models have derived types, especially for complex type models, it's best to input all entity types at the point of registration.
        /// </param>
        public static void RegisterWithConverter(IEnumerable<Type> entityTypes)
        {
            RegisterWithConverter(entityTypes, new EntityConverter());
        }

        /// <summary>
        /// Adds <see cref="EntityConverter"/> to the <see cref="GraphClient.DefaultJsonConverters"/> array in order to handle entity JSON serialization and deserialization, 
        /// while making other necessary configuration changes.
        /// NOTE: THIS METHOD SHOULD BE CALLED ONLY ONCE, AND BEFORE ANY CODE THAT USES <see cref="Neo4jClient"/> IS CALLED. 
        /// If you think the converter would undesirably interfere with your JSON serialization, and you do not already use your own custom resolver,  
        /// then call the <see cref="RegisterWithResolver(EntityResolver)"/> method instead to use the <see cref="EntityResolver"/>.
        /// </summary>
        /// <param name="converter">An instance of <see cref="EntityConverter"/> to use. An instance from a derived class is permitted.</param>
        public static void RegisterWithConverter(EntityConverter converter)
        {
            RegisterWithConverter(null, converter);
        }

        /// <summary>
        /// Adds <see cref="EntityConverter"/> to the <see cref="GraphClient.DefaultJsonConverters"/> array in order to handle entity JSON serialization and deserialization, 
        /// while making other necessary configuration changes.
        /// NOTE: THIS METHOD SHOULD BE CALLED ONLY ONCE, AND BEFORE ANY CODE THAT USES <see cref="Neo4jClient"/> IS CALLED. 
        /// If you think the converter would undesirably interfere with your JSON serialization, and you do not already use your own custom resolver,  
        /// then call the <see cref="RegisterWithResolver(IEnumerable{Type}, EntityResolver)"/> method instead to use the <see cref="EntityResolver"/>.
        /// </summary>
        /// <param name="entityTypes">All entity types (i.e., model classes) used in your project. 
        /// Ideally, this library needs to know all your entity types early on so as to best determine how to construct the class hierarchies. 
        /// For simple classes with no inheritances, you may probably skip adding any entity types. 
        /// But if your models have derived types, especially for complex type models, it's best to input all entity types at the point of registration.
        /// </param>
        /// <param name="converter">An instance of <see cref="EntityConverter"/> to use. An instance from a derived class is permitted.</param>
        public static void RegisterWithConverter(IEnumerable<Type> entityTypes, EntityConverter converter)
        {
            InternalRegister(entityTypes, null, converter);
        }


        internal static void InternalRegister(IEnumerable<Type> entityTypes, EntityResolver entityResolver, EntityConverter entityConverter)
        {
            if (entityResolver == null && entityConverter == null)
            {
                throw new InvalidOperationException(Messages.NoResolverOrConverterError);
            }
            else if (entityResolver != null && entityConverter != null)
            {
                throw new InvalidOperationException(Messages.BothResolverAndConverterError);
            }

            //if (entityTypes == null || entityTypes.FirstOrDefault() == null)
            //{
            //    throw new ArgumentNullException(nameof(entityTypes), "Neo4jClient.DataAnnotations needs to know all your entity types (including complex types) and their derived types aforehand in order to do efficient work.");
            //}

            var _converters = new List<JsonConverter>();
            var defaultConverters = GraphClient.DefaultJsonConverters;

            if (defaultConverters != null)
                _converters.AddRange(defaultConverters);

            if (entityResolver != null)
            {
                var defaultResolver = GraphClient.DefaultJsonContractResolver;

                if (defaultResolver == null || !typeof(EntityResolver).IsAssignableFrom(defaultResolver.GetType()))
                {
                    Defaults.EntityResolver = entityResolver;

                    var dummyConverterType = typeof(ResolverDummyConverter);
                    if (defaultConverters == null || !defaultConverters.Any(c => dummyConverterType.IsAssignableFrom(c.GetType())))
                    {
                        //add a dummy converter that just proxies entityresolver deserialization
                        //we do this because neo4jclient currently doesn't use ContractResolvers at deserialization, but they use converters.
                        Defaults.ResolverDummyConverter = new ResolverDummyConverter();
                        _converters.Add(Defaults.ResolverDummyConverter);
                    }

                    try
                    {
                        //try reflection to set the default resolver
                        typeof(GraphClient)
                            .GetField("DefaultJsonContractResolver", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                            .SetValue(null, entityResolver);
                    }
                    catch
                    {

                    }
                }
            }

            if (entityConverter != null)
            {
                var entityConverterType = typeof(EntityConverter);

                if (defaultConverters == null || !defaultConverters.Any(c => entityConverterType.IsAssignableFrom(c.GetType())))
                {
                    //we may have to mix this two (resolver and conveter) eventually because of some choices of the neo4jclient team.
                    //entityConverter._canRead = true;
                    Defaults.EntityConverter = entityConverter;

                    _converters.Add(entityConverter);
                }
            }

            if (_converters.Count != defaultConverters?.Length)
            {
                try
                {
                    //try reflection to set the converters in the original array
                    typeof(GraphClient)
                        .GetField("DefaultJsonConverters", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                        .SetValue(null, _converters.ToArray());
                }
                catch
                {

                }
            }
            
            if (entityTypes != null)
            {
                foreach (var entityType in entityTypes)
                {
                    AddEntityType(entityType);
                }
            }
        }

        private static void AddConverter(JsonConverter converter, Type converterType, IList<JsonConverter> converters)
        {
            if (!converters.Any(c => converterType.IsAssignableFrom(c.GetType())))
            {
                try
                {
                    converters.Insert(0, converter);
                }
                catch
                {
                    converters.Add(converter);
                }
            }
        }


        /// <summary>
        /// Registers a <see cref="System.Type"/> with Neo4jClient.DataAnnotations.
        /// This method also autmatically adds contained complex types. However, derived and/or base types must be added independently.
        /// </summary>
        /// <param name="entityType"></param>
        public static void AddEntityType(Type entityType)
        {
            if (!EntityTypes.Contains(entityType ?? throw new ArgumentNullException(nameof(entityType))))
            {
                entityTypes[entityType] = null;

                //check if they have complex properties and add automatically
                var complexTypes = entityType.GetProperties()?
                    .Select(p => p.PropertyType)
                    .Distinct()
                    .Where(t => t.IsComplex());

                if (complexTypes != null)
                {
                    foreach (var complexType in complexTypes)
                    {
                        AddEntityType(complexType);
                    }
                }

                //auto add base class too
                if (entityType.GetTypeInfo().BaseType is Type baseType 
                    && baseType != Defaults.ObjectType
                    && baseType != typeof(ValueType))
                    AddEntityType(baseType);
            }
        }

        public static void RemoveEntityType(Type entityType)
        {
            if (EntityTypes.Contains(entityType ?? throw new ArgumentNullException(nameof(entityType))))
            {
                entityTypes.TryRemove(entityType, out var val);
            }
        }

        public static bool ContainsEntityType(Type entityType, bool includeBaseClasses = true)
        {
            var ret = EntityTypes.Contains(entityType ?? throw new ArgumentNullException(nameof(entityType)))
                || entityType.GetTypeInfo().IsGenericType && EntityTypes.Contains(entityType.GetGenericTypeDefinition()) //search generics too
                || (includeBaseClasses && EntityTypes.Any(baseType => baseType.IsAssignableFrom(entityType)
                || baseType.IsGenericAssignableFrom(entityType))) //optional
                ;

            return ret;
        }

        public static List<Type> GetDerivedEntityTypes(Type baseType)
        {
            if (baseType == null)
                throw new ArgumentNullException(nameof(baseType));

            var existing = EntityTypes.Where(type => baseType.IsAssignableFrom(type)
            || baseType.IsGenericAssignableFrom(type));

            var list = existing?.ToList();

            if (list != null)
                //sort them
                list.Sort((x, y) => x.IsGenericAssignableFrom(y) ? -1 : (y.IsGenericAssignableFrom(x) ? 1 : (x.Name.CompareTo(y.Name))));

            return list;
        }


        public static EntityTypeInfo GetEntityTypeInfo(Type type)
        {
            EntityTypeInfo info;
            if (!entityTypeInfo.TryGetValue(type ?? throw new ArgumentNullException(nameof(type)), out info))
            {
                info = new EntityTypeInfo(type);
                entityTypeInfo[type] = info;
            }

            return info;
        }

        public static List<EntityTypeInfo> GetDerivedEntityTypeInfos(Type baseType, bool getFromEntityTypesToo = false)
        {
            if (baseType == null)
                throw new ArgumentNullException(nameof(baseType));

            var existing = entityTypeInfo.Where(pair => baseType.IsAssignableFrom(pair.Key)
            || baseType.IsGenericAssignableFrom(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);

            existing = existing ?? new Dictionary<Type, EntityTypeInfo>();

            if (existing.Count == 0 || !existing.ContainsKey(baseType))
            {
                existing[baseType] = GetEntityTypeInfo(baseType);
            }

            if (getFromEntityTypesToo)
            {
                //check entity types
                var derivedTypes = GetDerivedEntityTypes(baseType);
                foreach (var derivedType in derivedTypes)
                {
                    if (!existing.ContainsKey(derivedType))
                        existing[derivedType] = GetEntityTypeInfo(derivedType);
                }
            }

            var list = existing.Values.ToList();

            if (list != null)
                //sort them
                list.Sort((x, y) => x.Type.IsGenericAssignableFrom(y.Type) ? -1 : (y.Type.IsGenericAssignableFrom(x.Type) ? 1 : (x.Type.Name.CompareTo(y.Type.Name))));

            return list;
        }
    }
}
