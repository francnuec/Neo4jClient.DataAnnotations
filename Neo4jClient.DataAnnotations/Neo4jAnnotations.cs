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

        public static ICollection<Type> EntityTypes { get { return entityTypes.Keys; } }

        public static void RegisterWithResolver()
        {
            RegisterWithResolver(null, new EntityResolver());
        }

        public static void RegisterWithResolver(IEnumerable<Type> entityTypes)
        {
            RegisterWithResolver(entityTypes, new EntityResolver());
        }

        public static void RegisterWithConverter()
        {
            RegisterWithConverter(null, new EntityConverter());
        }

        public static void RegisterWithConverter(IEnumerable<Type> entityTypes)
        {
            RegisterWithConverter(entityTypes, new EntityConverter());
        }

        public static void RegisterWithResolver(EntityResolver resolver)
        {
            RegisterWithResolver(null, resolver);
        }

        public static void RegisterWithResolver(IEnumerable<Type> entityTypes, EntityResolver resolver)
        {
            InternalRegister(entityTypes, resolver, null);
        }

        public static void RegisterWithConverter(EntityConverter converter)
        {
            RegisterWithConverter(null, converter);
        }

        public static void RegisterWithConverter(IEnumerable<Type> entityTypes, EntityConverter converter)
        {
            InternalRegister(entityTypes, null, converter);
        }

        public static void AddEntityType(Type entityType)
        {
            if (!EntityTypes.Contains(entityType))
            {
                entityTypes[entityType] = null;
            }
        }

        public static void RemoveEntityType(Type entityType)
        {
            if (EntityTypes.Contains(entityType))
            {
                entityTypes.TryRemove(entityType, out var val);
            }
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

            if (entityResolver != null)
            {
                var defaultResolver = GraphClient.DefaultJsonContractResolver;

                if (defaultResolver == null || !typeof(EntityResolver).IsAssignableFrom(defaultResolver.GetType()))
                {
                    Defaults.EntityResolver = entityResolver;

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

                var defaultConverters = GraphClient.DefaultJsonConverters;

                if (defaultConverters == null || !defaultConverters.Any(c => entityConverterType.IsAssignableFrom(c.GetType())))
                {
                    Defaults.EntityConverter = entityConverter;

                    var _converters = new List<JsonConverter>();
                    _converters.Add(entityConverter);
                    _converters.AddRange(defaultConverters ?? new JsonConverter[0]);

                    try
                    {
                        //try reflection to insert this converter in the original array
                        typeof(GraphClient)
                            .GetField("DefaultJsonConverters", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                            .SetValue(null, _converters.ToArray());
                    }
                    catch
                    {

                    }
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

        internal static EntityTypeInfo GetEntityTypeInfo(Type type)
        {
            EntityTypeInfo info;
            if (!entityTypeInfo.TryGetValue(type, out info))
            {
                info = new EntityTypeInfo(type);
                entityTypeInfo[type] = info;
            }

            return info;
        }

        internal static List<EntityTypeInfo> GetDerivedEntityTypeInfos(Type baseType, bool getFromEntityTypesToo = false)
        {
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

        internal static bool ContainsEntityType(Type entityType, bool includeBaseClasses = true)
        {
            var ret = EntityTypes.Contains(entityType)
                || entityType.GetTypeInfo().IsGenericType && EntityTypes.Contains(entityType.GetGenericTypeDefinition()) //search generics too
                || (includeBaseClasses && EntityTypes.Any(baseType => baseType.IsAssignableFrom(entityType)
                || baseType.IsGenericAssignableFrom(entityType))) //optional
                ;

            return ret;
        }

        internal static List<Type> GetDerivedEntityTypes(Type baseType)
        {
            var existing = EntityTypes.Where(type => baseType.IsAssignableFrom(type)
            || baseType.IsGenericAssignableFrom(type));

            var list = existing?.ToList();

            if (list != null)
                //sort them
                list.Sort((x, y) => x.IsGenericAssignableFrom(y) ? -1 : (y.IsGenericAssignableFrom(x) ? 1 : (x.Name.CompareTo(y.Name))));

            return list;
        }

        //internal static void Register(IGraphClient client)
        //{
        //    if (client.JsonContractResolver == null)
        //    {
        //        throw new InvalidOperationException("You need a json contract resolver of type DefaultContractResolver.");
        //    }

        //    var entityConverterType = typeof(EntityConverter);

        //    var entityConverter = new EntityConverter();

        //    IList<JsonConverter> converters = client.JsonConverters;

        //    if (converters == null)
        //    {
        //        throw new InvalidOperationException("You need a list of JsonConverters set.");
        //    }

        //    AddConverter(entityConverter, entityConverterType, converters);

        //    var executionConfig = client.ExecutionConfiguration;

        //    if (executionConfig != null && executionConfig.JsonConverters != converters)
        //    {
        //        converters = executionConfig.JsonConverters as IList<JsonConverter>;

        //        if (converters != null)
        //        {
        //            AddConverter(entityConverter, entityConverterType, converters);
        //        }
        //    }

        //    InternalRegister(entityConverter, entityConverterType);
        //}
    }
}
