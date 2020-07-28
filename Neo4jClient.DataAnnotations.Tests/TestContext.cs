using System;
using System.Collections.Generic;
using System.Linq;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using NSubstitute;

namespace Neo4jClient.DataAnnotations.Tests
{
    public abstract class TestContext
    {
        private IGraphClient client;

        private Func<string, Type, object> deserializer;

        private JsonSerializerSettings deserializerSettings;

        private ICypherFluentQuery query;

        private QueryContext queryContext;

        private Func<object, string> serializer;

        protected internal virtual IGraphClient Client
        {
            get
            {
                if (client == null)
                {
                    client = Substitute.For<IRawGraphClient>();

                    client.JsonConverters.Returns(GraphClient.DefaultJsonConverters?.ToList() ??
                                                  new List<JsonConverter>());
                    client.JsonContractResolver.Returns(GraphClient.DefaultJsonContractResolver);

                    client.Serializer.Returns(info =>
                    {
                        return new CustomJsonSerializer
                        {
                            JsonContractResolver = client.JsonContractResolver,
                            JsonConverters = client.JsonConverters
                        };
                    });

                    client.IsConnected.Returns(true);
                    client.ServerVersion.Returns(new Version(3, 0));
                    client.CypherCapabilities.Returns(CypherCapabilities.Cypher23);
                }

                return client;
            }
            set => client = value;
        }

        public abstract AnnotationsContext AnnotationsContext { get; set; }

        protected virtual EntityService EntityService => AnnotationsContext.EntityService;
        public virtual IEnumerable<Type> EntityTypes => EntityService.EntityTypes;

        public abstract JsonSerializerSettings SerializerSettings { get; set; }

        public virtual JsonSerializerSettings DeserializerSettings
        {
            get
            {
                if (deserializerSettings == null) deserializerSettings = SerializerSettings;

                return deserializerSettings;
            }
            set => deserializerSettings = value;
        }

        public virtual Func<object, string> Serializer
        {
            get
            {
                if (serializer == null)
                {
                    Attach();
                    serializer = entity => JsonConvert.SerializeObject(entity, SerializerSettings);
                }

                return serializer;
            }
            set => serializer = value;
        }

        public virtual Func<string, Type, object> Deserializer
        {
            get
            {
                if (deserializer == null)
                {
                    Attach();
                    deserializer = (value, type) => JsonConvert.DeserializeObject(value, type, DeserializerSettings);
                }

                return deserializer;
            }
            set => deserializer = value;
        }

        public virtual ICypherFluentQuery Query
        {
            get
            {
                if (query == null)
                {
                    Attach();
                    query = new CypherFluentQuery(Client);
                }

                return query;
            }
            set => query = value;
        }

        public virtual QueryContext QueryContext
        {
            get
            {
                if (queryContext == null)
                {
                    Attach();
                    queryContext = new QueryContext
                    {
                        AnnotationsContext = AnnotationsContext,
                        Client = Client,
                        SerializeCallback = Serializer
                    };
                }

                return queryContext;
            }
            set => queryContext = value;
        }

        protected void Attach()
        {
            //simply get the annotations context
            var context = AnnotationsContext;
        }
    }

    public class ResolverTestContext : TestContext
    {
        private AnnotationsContext annotationsContext;

        private EntityResolver resolver;

        private JsonSerializerSettings serializerSettings;

        protected EntityResolver Resolver
        {
            get
            {
                if (resolver == null) resolver = AnnotationsContext.EntityResolver; //new EntityResolver();

                return resolver;
            }
            private set
            {
                resolver = value;
                DeserializerSettings = null;
                SerializerSettings = null;
                Deserializer = null;
                Serializer = null;
            }
        }

        public override JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (serializerSettings == null)
                    serializerSettings = new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { Resolver.DeserializeConverter },
                        ContractResolver = Resolver,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                return serializerSettings;
            }
            set => serializerSettings = value;
        }

        public override AnnotationsContext AnnotationsContext
        {
            get
            {
                if (annotationsContext == null)
                    annotationsContext = Client.WithAnnotations<TestAnnotationsContext, EntityResolver>()
                        .GetAnnotationsContext(); //new TestAnnotationsContext(Client, Resolver);

                return annotationsContext;
            }
            set
            {
                annotationsContext = value;
                Resolver = null;
            }
        }
    }

    public class ConverterTestContext : TestContext
    {
        private AnnotationsContext annotationsContext;

        private EntityConverter converter;

        private JsonSerializerSettings serializerSettings;

        protected EntityConverter Converter
        {
            get
            {
                if (converter == null)
                    converter = AnnotationsContext.EntityConverter; //new EntityConverter();
                return converter;
            }
            private set
            {
                converter = value;
                DeserializerSettings = null;
                SerializerSettings = null;
                Deserializer = null;
                Serializer = null;
            }
        }

        public override JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (serializerSettings == null)
                    serializerSettings = new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { Converter },
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                return serializerSettings;
            }
            set => serializerSettings = value;
        }

        public override AnnotationsContext AnnotationsContext
        {
            get
            {
                if (annotationsContext == null)
                    annotationsContext = Client.WithAnnotationsConverter<TestAnnotationsContext, EntityConverter>()
                        .GetAnnotationsContext(); //new TestAnnotationsContext(Client, Converter);

                return annotationsContext;
            }
            set
            {
                annotationsContext = value;
                Converter = null;
            }
        }
    }
}