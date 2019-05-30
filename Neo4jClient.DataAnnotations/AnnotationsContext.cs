﻿using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Reflection;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.Cypher;

namespace Neo4jClient.DataAnnotations
{
    /// <summary>
    /// The context class for entities.
    /// </summary>
    public class AnnotationsContext : IAnnotationsContext, IHaveEntityService
    {
        private static object staticLockObj = new object();
        private static EntityService defaultEntityService;
        protected static EntityService DefaultEntityService
        {
            get
            {
                lock (staticLockObj)
                {
                    if (defaultEntityService == null)
                    {
                        defaultEntityService = CreateNewEntityService();
                    }
                }

                return defaultEntityService;
            }
        }

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

            if (entityService == null)
            {
                entityService = DefaultEntityService;
            }

            EntityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            EntityResolver = resolver;
            EntityConverter = EntityResolver == null ? converter : null;

            if (EntityResolver == null && EntityConverter == null)
            {
                EntityResolver = new EntityResolver(); //use resolver by default
            }

            Init();
        }

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

        /// <summary>
        /// The attached <see cref="IGraphClient"/>
        /// </summary>
        public IGraphClient GraphClient { get; }
        /// <summary>
        /// The attached <see cref="DataAnnotations.EntityService"/>
        /// </summary>
        public EntityService EntityService { get; }
        /// <summary>
        /// The attached <see cref="EntityResolver"/>. This should be <code>null</code> if <see cref="EntityConverter"/> is present.
        /// </summary>
        public EntityResolver EntityResolver { get; }
        /// <summary>
        /// The attached <see cref="EntityConverter"/>. This should be <code>null</code> if <see cref="EntityResolver"/> is present.
        /// </summary>
        public EntityConverter EntityConverter { get; }
        /// <summary>
        /// The attached <see cref="EntityResolverConverter"/> used for deserialization purposes.
        /// </summary>
        public EntityResolverConverter EntityResolverConverter => EntityResolver?.DeserializeConverter;
        /// <summary>
        /// A shortcut to the <see cref="ICypherGraphClient.Cypher"/> property.
        /// </summary>
        public ICypherFluentQuery Cypher => GraphClient?.Cypher;

        protected static void Attach
            (AnnotationsContext context, IGraphClient graphClient,
            EntityResolver resolver, EntityConverter converter)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (graphClient == null)
                throw new ArgumentNullException(nameof(graphClient));

            if (resolver == null && converter == null)
            {
                throw new InvalidOperationException(Messages.NoResolverOrConverterError);
            }
            else if (resolver != null && converter != null)
            {
                throw new InvalidOperationException(Messages.BothResolverAndConverterError);
            }

            //if (entityTypes == null || entityTypes.FirstOrDefault() == null)
            //{
            //    throw new ArgumentNullException(nameof(entityTypes), "Neo4jClient.DataAnnotations needs to know all your entity types (including complex types) and their derived types aforehand in order to do efficient work.");
            //}

            List<JsonConverter> _converters = graphClient.JsonConverters;
            if (_converters == null)
            {
                _converters = new List<JsonConverter>();
            }

            if (resolver != null)
            {
                resolver.AnnotationsContext = context;
                //EntityResolver = entityResolver;

                var dummyConverterType = typeof(EntityResolverConverter);
                if (_converters.FirstOrDefault(c => dummyConverterType.IsAssignableFrom(c.GetType())) is EntityResolverConverter existingConverter)
                {
                    _converters.Remove(existingConverter);
                }

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
                if (_converters.FirstOrDefault(c => entityConverterType.IsAssignableFrom(c.GetType())) is EntityConverter existingConverter)
                {
                    _converters.Remove(existingConverter);
                }

                //we may have to mix this two (resolver and converter) eventually because of some choices of the neo4jclient team.
                //entityConverter._canRead = true;
                _converters.Add(converter);
            }

            if (_converters.Count > 0 && graphClient.JsonConverters != _converters) //!= existingConverters?.Length)
            {
                try
                {
                    //try reflection to set the converters in the original array
                    Utils.Utilities.GetBackingField(graphClient.GetType() //typeof(IGraphClient)
                        .GetProperty("JsonConverters", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                        .SetValue(graphClient, _converters);
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