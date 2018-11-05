using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Reflection;
using Neo4jClient.DataAnnotations.Serialization;

namespace Neo4jClient.DataAnnotations
{
    public class AnnotationsContext : IAnnotationsContext, IHaveEntityService
    {
        private static object staticLockObj = new object();
        private static IEntityService defaultEntityService;
        protected static IEntityService DefaultEntityService
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

        public AnnotationsContext(IGraphClient graphClient, IEntityService entityService) 
            : this(graphClient, null, null, entityService)
        {
        }

        public AnnotationsContext(IGraphClient graphClient, EntityResolver resolver) 
            : this(graphClient, resolver, null, null)
        {
        }

        public AnnotationsContext(IGraphClient graphClient, EntityResolver resolver, IEntityService entityService) 
            : this(graphClient, resolver, null, entityService)
        {
        }

        public AnnotationsContext(IGraphClient graphClient, EntityConverter converter) 
            : this(graphClient, null, converter, null)
        {
        }

        public AnnotationsContext(IGraphClient graphClient, EntityConverter converter, IEntityService entityService)
            : this(graphClient, null, converter, entityService)
        {
        }

        protected AnnotationsContext(
            IGraphClient graphClient, 
            EntityResolver resolver, 
            EntityConverter converter, 
            IEntityService entityService)
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

        public IGraphClient GraphClient { get; }
        public IEntityService EntityService { get; }
        public EntityResolver EntityResolver { get; }
        public EntityConverter EntityConverter { get; }
        public EntityResolverConverter EntityResolverConverter => EntityResolver?.DeserializeConverter;

        ///// <summary>
        ///// Replaces the <see cref="GraphClient.DefaultJsonContractResolver"/> 
        ///// with <see cref="EntityResolver"/> in order to handle entity JSON serialization and deserialization, while making other necessary configuration changes.
        ///// NOTE: THIS METHOD SHOULD BE CALLED ONLY ONCE, AND BEFORE ANY CODE THAT USES <see cref="Neo4jClient"/> IS CALLED. 
        ///// If you already use your own custom resolver, do not call this method, 
        ///// but call the <see cref="RegisterWithConverter()"/> method instead to use the <see cref="EntityConverter"/>.
        ///// </summary>
        //internal void AttachResolver(IGraphClient graphClient)
        //{
        //    AttachResolver(graphClient, new EntityResolver());
        //}

        ///// <summary>
        ///// Replaces the <see cref="GraphClient.DefaultJsonContractResolver"/> 
        ///// with <see cref="EntityResolver"/> in order to handle entity JSON serialization and deserialization, while making other necessary configuration changes.
        ///// NOTE: THIS METHOD SHOULD BE CALLED ONLY ONCE, AND BEFORE ANY CODE THAT USES <see cref="Neo4jClient"/> IS CALLED. 
        ///// If you already use your own custom resolver, do not call this method, 
        ///// but call the <see cref="RegisterWithConverter(IEnumerable{Type}, EntityConverter)"/> method instead to use the <see cref="EntityConverter"/>.
        ///// </summary>
        ///// <param name="entityTypes">All entity types (i.e., model classes) used in your project. 
        ///// Ideally, this library needs to know all your entity types early on so as to best determine how to construct the class hierarchies. 
        ///// For simple classes with no inheritances, you may probably skip adding any entity types. 
        ///// But if your models have derived types, especially for complex type models, it's best to input all entity types at the point of registration.
        ///// </param>
        ///// <param name="resolver">An instance of <see cref="EntityResolver"/> to use. An instance from a derived class is permitted.</param>
        //internal void AttachResolver(IGraphClient graphClient, EntityResolver resolver)
        //{
        //    InternalAttachment(graphClient, resolver, null);
        //}

        ///// <summary>
        ///// Adds <see cref="EntityConverter"/> to the <see cref="GraphClient.DefaultJsonConverters"/> array in order to handle entity JSON serialization and deserialization, 
        ///// while making other necessary configuration changes.
        ///// NOTE: THIS METHOD SHOULD BE CALLED ONLY ONCE, AND BEFORE ANY CODE THAT USES <see cref="Neo4jClient"/> IS CALLED. 
        ///// If you think the converter would undesirably interfere with your JSON serialization, and you do not already use your own custom resolver,  
        ///// then call the <see cref="RegisterWithResolver()"/> method instead to use the <see cref="EntityResolver"/>.
        ///// </summary>
        //internal void AttachConverter(IGraphClient graphClient)
        //{
        //    AttachConverter(graphClient, new EntityConverter());
        //}

        ///// <summary>
        ///// Adds <see cref="EntityConverter"/> to the <see cref="GraphClient.DefaultJsonConverters"/> array in order to handle entity JSON serialization and deserialization, 
        ///// while making other necessary configuration changes.
        ///// NOTE: THIS METHOD SHOULD BE CALLED ONLY ONCE, AND BEFORE ANY CODE THAT USES <see cref="Neo4jClient"/> IS CALLED. 
        ///// If you think the converter would undesirably interfere with your JSON serialization, and you do not already use your own custom resolver,  
        ///// then call the <see cref="RegisterWithResolver(IEnumerable{Type}, EntityResolver)"/> method instead to use the <see cref="EntityResolver"/>.
        ///// </summary>
        ///// <param name="entityTypes">All entity types (i.e., model classes) used in your project. 
        ///// Ideally, this library needs to know all your entity types early on so as to best determine how to construct the class hierarchies. 
        ///// For simple classes with no inheritances, you may probably skip adding any entity types. 
        ///// But if your models have derived types, especially for complex type models, it's best to input all entity types at the point of registration.
        ///// </param>
        ///// <param name="converter">An instance of <see cref="EntityConverter"/> to use. An instance from a derived class is permitted.</param>
        //internal void AttachConverter(IGraphClient graphClient, EntityConverter converter)
        //{
        //    InternalAttachment(graphClient, null, converter);
        //}

        protected static void Attach
            (IAnnotationsContext context, IGraphClient graphClient,
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

                //we may have to mix this two (resolver and conveter) eventually because of some choices of the neo4jclient team.
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

        public static IEntityService CreateNewEntityService()
        {
            return new EntityService();
        }
    }
}