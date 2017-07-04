using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Reflection;

namespace Neo4jClient.DataAnnotations
{
    public class Neo4jAnnotations
    {
        private static Dictionary<Type, EntityTypeInfo> entityTypeInfo { get; set; }
            = new Dictionary<Type, EntityTypeInfo>();

        internal static List<Type> EntityTypes { get; } = new List<Type>();

        internal static EntityTypeInfo GetEntityTypeInfo(Type type)
        {
            EntityTypeInfo info;
            if(!entityTypeInfo.TryGetValue(type, out info))
            {
                info = new EntityTypeInfo(type);
                entityTypeInfo[type] = info;
            }

            return info;
        }

        internal static List<EntityTypeInfo> GetDerivedEntityTypeInfos(Type baseType)
        {
            var existing = entityTypeInfo.Where(pair => baseType.IsAssignableFrom(pair.Key)
            || baseType.IsGenericAssignableFrom(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);

            existing = existing ?? new Dictionary<Type, EntityTypeInfo>();

            if(existing.Count == 0 || !existing.ContainsKey(baseType))
            {
                existing[baseType] = GetEntityTypeInfo(baseType);
            }

            return existing.Values.ToList();
        }

        public static void AddEntityType(Type entityType)
        {
            if (!EntityTypes.Contains(entityType))
            {
                EntityTypes.Add(entityType);
            }
        }

        internal static void Register(IGraphClient client)
        {
            if (client.JsonContractResolver == null)
            {
                throw new InvalidOperationException("You need a json contract resolver of type DefaultContractResolver.");
            }

            var entityConverterType = typeof(EntityConverter);

            var entityConverter = new EntityConverter();

            IList<JsonConverter> converters = client.JsonConverters;

            if (converters == null)
            {
                throw new InvalidOperationException("You need a list of JsonConverters set.");
            }

            AddConverter(entityConverter, entityConverterType, converters);

            var executionConfig = client.ExecutionConfiguration;

            if (executionConfig != null && executionConfig.JsonConverters != converters)
            {
                converters = executionConfig.JsonConverters as IList<JsonConverter>;

                if (converters != null)
                {
                    AddConverter(entityConverter, entityConverterType, converters);
                }
            }

            InternalRegister(entityConverter, entityConverterType);
        }

        public static void Register()
        {
            InternalRegister(new EntityConverter(), typeof(EntityConverter));
        }

        internal static void InternalRegister(EntityConverter entityConverter, Type entityConverterType)
        {
            entityConverter = entityConverter ?? new EntityConverter();
            entityConverterType = entityConverterType ?? typeof(EntityConverter);

            var defaultConverters = GraphClient.DefaultJsonConverters;

            var _converters = new List<JsonConverter>();
            _converters.Add(entityConverter);
            _converters.AddRange(defaultConverters ?? new JsonConverter[0]);
            
            if (defaultConverters != null && !defaultConverters.Any(c => c.GetType() == entityConverterType))
            {
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

            //also try json default settings
            //only set if null
            if(JsonConvert.DefaultSettings == null)
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
                {
                    Converters = _converters
                };
            }
        }

        private static void AddConverter(JsonConverter converter, Type converterType, IList<JsonConverter> converters)
        {
            if (!converters.Any(c => c.GetType() == converterType))
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
    }
}
