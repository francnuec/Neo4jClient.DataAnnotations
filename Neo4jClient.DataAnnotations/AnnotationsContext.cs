using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neo4j.Driver;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Extensions.Driver;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Utils;
using Newtonsoft.Json;

namespace Neo4jClient.DataAnnotations
{
    /// <summary>
    ///     The context class for entities.
    /// </summary>
    public class AnnotationsContext : IAnnotationsContext, IHaveEntityService
    {
        private static readonly object staticLockObj = new object();
        private static EntityService defaultEntityService;

        public AnnotationsContext(IGraphClient graphClient)
            : this(graphClient, null, null, null)
        {
        }

        public AnnotationsContext(IGraphClient graphClient, EntityService entityService)
            : this(graphClient, null, null, entityService)
        {
        }

        public AnnotationsContext(IGraphClient graphClient, EntityResolver resolver)
            : this(graphClient, resolver, null, null)
        {
        }

        public AnnotationsContext(IGraphClient graphClient, EntityResolver resolver, EntityService entityService)
            : this(graphClient, resolver, null, entityService)
        {
        }

        public AnnotationsContext(IGraphClient graphClient, EntityConverter converter)
            : this(graphClient, null, converter, null)
        {
        }

        public AnnotationsContext(IGraphClient graphClient, EntityConverter converter, EntityService entityService)
            : this(graphClient, null, converter, entityService)
        {
        }

        protected AnnotationsContext(
            IGraphClient graphClient,
            EntityResolver resolver,
            EntityConverter converter,
            EntityService entityService)
        {
            GraphClient = graphClient ?? throw new ArgumentNullException(nameof(graphClient));

            if (entityService == null) entityService = DefaultEntityService;

            EntityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            EntityResolver = resolver;
            EntityConverter = EntityResolver == null ? converter : null;

            if (EntityResolver == null && EntityConverter == null)
                EntityResolver = new EntityResolver(); //use resolver by default

            Init();
        }

        protected static EntityService DefaultEntityService
        {
            get
            {
                lock (staticLockObj)
                {
                    if (defaultEntityService == null) defaultEntityService = CreateNewEntityService();
                }

                return defaultEntityService;
            }
        }

        /// <summary>
        ///     The attached <see cref="IGraphClient" />
        /// </summary>
        public IGraphClient GraphClient { get; }

        /// <summary>
        ///     The attached <see cref="DataAnnotations.EntityService" />
        /// </summary>
        public EntityService EntityService { get; }

        /// <summary>
        ///     The attached <see cref="EntityResolver" />. This should be <code>null</code> if <see cref="EntityConverter" /> is
        ///     present.
        /// </summary>
        public EntityResolver EntityResolver { get; }

        /// <summary>
        ///     The attached <see cref="EntityConverter" />. This should be <code>null</code> if <see cref="EntityResolver" /> is
        ///     present.
        /// </summary>
        public EntityConverter EntityConverter { get; }

        /// <summary>
        ///     The attached <see cref="EntityResolverConverter" /> used for deserialization purposes.
        /// </summary>
        public EntityResolverConverter EntityResolverConverter => EntityResolver?.DeserializeConverter;

        /// <summary>
        ///     A shortcut to the <see cref="ICypherGraphClient.Cypher" /> property.
        /// </summary>
        public ICypherFluentQuery Cypher => GraphClient?.Cypher;

        public bool IsBoltClient => GraphClient is IBoltGraphClient;

        protected void Init()
        {
            Attach(this, GraphClient, EntityResolver, EntityConverter);

            //get all properties of type entityset and add them to the entity service
            var entitySetProperties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.CanWrite && Defaults.EntitySetType.IsGenericAssignableFrom(p.PropertyType));

            var setConstructorParams = new object[] { EntityService };
            foreach (var prop in entitySetProperties)
            {
                var set = Utils.Utilities.CreateInstance(
                    Defaults.ConcreteEntitySetType
                        .MakeGenericType(prop.PropertyType.GenericTypeArguments.First()),
                    parameters: setConstructorParams);
                prop.SetValue(this, set);
            }
        }

        protected static void Attach
        (AnnotationsContext context, IGraphClient graphClient,
            EntityResolver resolver, EntityConverter converter)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (graphClient == null)
                throw new ArgumentNullException(nameof(graphClient));

            if (resolver == null && converter == null)
                throw new InvalidOperationException(Messages.NoResolverOrConverterError);
            if (resolver != null && converter != null)
                throw new InvalidOperationException(Messages.BothResolverAndConverterError);

            //if (entityTypes == null || entityTypes.FirstOrDefault() == null)
            //{
            //    throw new ArgumentNullException(nameof(entityTypes), "Neo4jClient.DataAnnotations needs to know all your entity types (including complex types) and their derived types aforehand in order to do efficient work.");
            //}

            var _converters = graphClient.JsonConverters;
            if (_converters == null) _converters = new List<JsonConverter>();

            if (resolver != null)
            {
                resolver.AnnotationsContext = context;
                //EntityResolver = entityResolver;

                var dummyConverterType = typeof(EntityResolverConverter);
                if (_converters.FirstOrDefault(c => dummyConverterType.IsAssignableFrom(c.GetType())) is
                    EntityResolverConverter existingConverter) _converters.Remove(existingConverter);

                graphClient.JsonContractResolver = resolver;

                //add a dummy converter that just proxies entityresolver deserialization
                //we do this because neo4jclient currently doesn't use ContractResolvers at deserialization, but they use converters.
                _converters.Add(resolver.DeserializeConverter);
            }

            if (converter != null)
            {
                converter.AnnotationsContext = context;
                //EntityConverter = entityConverter;

                var entityConverterType = typeof(EntityConverter);
                if (_converters.FirstOrDefault(c => entityConverterType.IsAssignableFrom(c.GetType())) is
                    EntityConverter existingConverter) _converters.Remove(existingConverter);

                //we may have to mix this two (resolver and converter) eventually because of some choices of the neo4jclient team.
                //entityConverter._canRead = true;
                _converters.Add(converter);
            }

            if (_converters.Count > 0 && graphClient.JsonConverters != _converters) //!= existingConverters?.Length)
                try
                {
                    //try reflection to set the converters in the original array
                    Utils.Utilities.GetBackingField(graphClient.GetType()
                            .GetProperty("JsonConverters",
                                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                        .SetValue(graphClient, _converters);
                }
                catch
                {
                }

            if (graphClient is IBoltGraphClient)
            {
                if (!graphClient.IsConnected)
                    //connection is required at this point for bolt clients
                    throw new InvalidOperationException(Messages.ClientIsNotConnectedError);

                dynamic driverMemberInfo = null;

                var driverProperty = graphClient
                    .GetType()
                    .GetProperty("Driver", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (driverProperty != null && !driverProperty.CanWrite)
                {
                    FieldInfo driverBackingField = null;

                    try
                    {
                        //try reflection to set the converters in the original array
                        driverBackingField = Utils.Utilities.GetBackingField(driverProperty);
                        driverMemberInfo = driverBackingField;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    driverMemberInfo = driverProperty;
                }

                var driver = driverMemberInfo?.GetValue(graphClient) as IDriver;
                if (driver == null)
                    //this isn't supposed to happen
                    throw new InvalidOperationException(Messages.ClientHasNoDriverError);

                //now wrap the driver with our wrappers
                driver = new DriverWrapper(driver);

                try
                {
                    //replace the driver
                    driverMemberInfo?.SetValue(graphClient, driver);
                }
                catch
                {
                }
            }
        }

        public static EntityService CreateNewEntityService()
        {
            return new EntityService();
        }
    }
}