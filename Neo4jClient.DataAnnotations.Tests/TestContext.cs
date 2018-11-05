using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using NSubstitute;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient.DataAnnotations.Tests
{
    public abstract class TestContext
    {
        public TestContext()
        {

        }

        private IGraphClient client;
        protected virtual IGraphClient Client
        {
            get
            {
                if (client == null)
                {
                    client = Substitute.For<IRawGraphClient>();

                    client.JsonConverters.Returns(GraphClient.DefaultJsonConverters?.ToList() ?? new List<JsonConverter>());
                    client.JsonContractResolver.Returns(GraphClient.DefaultJsonContractResolver);

                    client.Serializer.Returns((info) =>
                    {
                        return new CustomJsonSerializer()
                        {
                            JsonContractResolver = client.JsonContractResolver,
                            JsonConverters = client.JsonConverters
                        };
                    });
                }

                return client;
            }
            set
            {
                client = value;
            }
        }

        public abstract IAnnotationsContext AnnotationsContext { get; set; }

        protected virtual IEntityService EntityService => AnnotationsContext.EntityService;
        public virtual IEnumerable<Type> EntityTypes => EntityService.EntityTypes;

        public abstract JsonSerializerSettings SerializerSettings { get; set; }

        private JsonSerializerSettings deserializerSettings;
        public virtual JsonSerializerSettings DeserializerSettings
        {
            get
            {
                if (deserializerSettings == null)
                {
                    deserializerSettings = SerializerSettings;
                }

                return deserializerSettings;
            }
            set
            {
                deserializerSettings = value;
            }
        }

        private Func<object, string> serializer;
        public virtual Func<object, string> Serializer
        {
            get
            {
                if (serializer == null)
                {
                    Attach();
                    serializer = (entity) => JsonConvert.SerializeObject(entity, SerializerSettings);
                }

                return serializer;
            }
            set
            {
                serializer = value;
            }
        }

        private Func<string, Type, object> deserializer;
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
            set
            {
                deserializer = value;
            }
        }

        private ICypherFluentQuery query;
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
            set
            {
                query = value;
            }
        }

        private QueryContext queryContext;
        public virtual QueryContext QueryContext
        {
            get
            {
                if (queryContext == null)
                {
                    Attach();
                    queryContext = new QueryContext()
                    {
                        AnnotationsContext = AnnotationsContext,
                        Client = Client,
                        SerializeCallback = Serializer
                    };
                }

                return queryContext;
            }
            set
            {
                queryContext = value;
            }
        }

        protected void Attach()
        {
            //simply get the annotations context
            var context = AnnotationsContext;
        }
    }

    public class ResolverTestContext : TestContext
    {
        public ResolverTestContext()
        {

        }

        private EntityResolver resolver;
        protected EntityResolver Resolver
        {
            get
            {
                if (resolver == null)
                {
                    resolver = AnnotationsContext.EntityResolver; //new EntityResolver();
                }

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

        private JsonSerializerSettings serializerSettings;
        public override JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (serializerSettings == null)
                {
                    serializerSettings = new JsonSerializerSettings()
                    {
                        Converters = new List<JsonConverter>() { Resolver.DeserializeConverter },
                        ContractResolver = Resolver,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    };
                }

                return serializerSettings;
            }
            set
            {
                serializerSettings = value;
            }
        }

        private IAnnotationsContext annotationsContext;
        public override IAnnotationsContext AnnotationsContext
        {
            get
            {
                if (annotationsContext == null)
                {
                    annotationsContext = Client.WithAnnotations<TestAnnotationsContext, EntityResolver>().GetAnnotationsContext(); //new TestAnnotationsContext(Client, Resolver);
                }

                return annotationsContext;
            }
            set
            {
                annotationsContext = value;
                Resolver = null;
            }
        }

        //public override QueryContext QueryContext
        //{
        //    get
        //    {
        //        var qContext = base.QueryContext;
        //        qContext.Resolver = Resolver;
        //        return qContext;
        //    }
        //    set => base.QueryContext = value;
        //}
    }

    public class ConverterTestContext : TestContext
    {
        public ConverterTestContext()
        {
        }

        private EntityConverter converter;
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

        private JsonSerializerSettings serializerSettings;
        public override JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (serializerSettings == null)
                {
                    serializerSettings = new JsonSerializerSettings()
                    {
                        Converters = new List<JsonConverter>() { Converter },
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    };
                }

                return serializerSettings;
            }
            set
            {
                serializerSettings = value;
            }
        }

        private IAnnotationsContext annotationsContext;
        public override IAnnotationsContext AnnotationsContext
        {
            get
            {
                if (annotationsContext == null)
                {
                    annotationsContext = Client.WithAnnotationsConverter<TestAnnotationsContext, EntityConverter>().GetAnnotationsContext(); //new TestAnnotationsContext(Client, Converter);
                }

                return annotationsContext;
            }
            set
            {
                annotationsContext = value;
                Converter = null;
            }
        }

        //public override QueryContext QueryContext
        //{
        //    get
        //    {
        //        var qContext = base.QueryContext;
        //        qContext.Converter = Converter;
        //        return qContext;
        //    }
        //    set => base.QueryContext = value;
        //}
    }
}
