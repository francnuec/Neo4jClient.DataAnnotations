using Neo4jClient.DataAnnotations.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using Neo4jClient.DataAnnotations.Utils;

namespace Neo4jClient.DataAnnotations
{
    public static class GraphClientExtensions
    {
        /// <summary>
        /// Gets the <see cref="AnnotationsContext"/> attached to the <see cref="IGraphClient"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <returns>Returns an instance of <see cref="AnnotationsContext"/> or null if not found.</returns>
        public static AnnotationsContext GetAnnotationsContext(this IGraphClient graphClient)
        {
            AnnotationsContext context = null;

            var resolver = graphClient?.JsonContractResolver as EntityResolver;
            context = resolver?.AnnotationsContext;

            if (context == null)
            {
                var converter = graphClient?.JsonConverters.FirstOrDefault(c => c is EntityConverter) as EntityConverter;
                context = converter?.AnnotationsContext;
            }

            return context;
        }



        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <see cref="AnnotationsContext"/> to the <see cref="IGraphClient"/> 
        /// using a resolver of type <see cref="EntityResolver"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <returns></returns>
        public static IGraphClient WithAnnotations(this IGraphClient graphClient)
        {
            return WithAnnotations<AnnotationsContext>(graphClient);
        }

        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <see cref="AnnotationsContext"/> to the <see cref="IGraphClient"/> 
        /// using a resolver of type <see cref="EntityResolver"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <param name="entityService">The service that provides entity information. Ideally, only one instance of <see cref="EntityService"/> should exist.
        /// This parameter is optional and can be null, however, you can get a new instance from <see cref="AnnotationsContext.CreateNewEntityService"/>.
        /// </param>
        /// <returns></returns>
        public static IGraphClient WithAnnotations(this IGraphClient graphClient, EntityService entityService)
        {
            return WithAnnotations<AnnotationsContext>(graphClient, entityService);
        }

        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <typeparamref name="TContext"/> to the <see cref="IGraphClient"/> 
        /// using a resolver of type <see cref="EntityResolver"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <returns></returns>
        public static IGraphClient WithAnnotations<TContext>(this IGraphClient graphClient) where TContext : AnnotationsContext
        {
            return WithAnnotations<TContext, EntityResolver>(graphClient);
        }

        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <typeparamref name="TContext"/> to the <see cref="IGraphClient"/> 
        /// using a resolver of type <see cref="EntityResolver"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <param name="entityService">The service that provides entity information. Ideally, only one instance of <see cref="EntityService"/> should exist.
        /// This parameter is optional and can be null, however, you can get a new instance from <see cref="AnnotationsContext.CreateNewEntityService"/>.
        /// </param>
        /// <returns></returns>
        public static IGraphClient WithAnnotations<TContext>(this IGraphClient graphClient, EntityService entityService) 
            where TContext : AnnotationsContext
        {
            return WithAnnotations<TContext, EntityResolver>(graphClient, entityService);
        }

        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <typeparamref name="TContext"/> 
        /// using a resolver of type <typeparamref name="TResolver"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <returns></returns>
        public static IGraphClient WithAnnotations<TContext, TResolver>(this IGraphClient graphClient) 
            where TContext : AnnotationsContext
            where TResolver : EntityResolver, new()
        {
            return WithAnnotations<TContext, TResolver>(graphClient, null, out var context);
        }

        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <typeparamref name="TContext"/> 
        /// using a resolver of type <typeparamref name="TResolver"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <param name="entityService">The service that provides entity information. Ideally, only one instance of <see cref="EntityService"/> should exist.
        /// This parameter is optional and can be null, however, you can get a new instance from <see cref="AnnotationsContext.CreateNewEntityService"/>.
        /// </param>
        /// <returns></returns>
        public static IGraphClient WithAnnotations<TContext, TResolver>(this IGraphClient graphClient, EntityService entityService)
            where TContext : AnnotationsContext
            where TResolver : EntityResolver, new()
        {
            return WithAnnotations<TContext, TResolver>(graphClient, entityService, out var context);
        }

        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <typeparamref name="TContext"/> 
        /// using a resolver of type <typeparamref name="TResolver"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <param name="entityService">The service that provides entity information. Ideally, only one instance of <see cref="EntityService"/> should exist.
        /// This parameter is optional and can be null, however, you can get a new instance from <see cref="AnnotationsContext.CreateNewEntityService"/>.
        /// </param>
        /// <param name="context">The newly created <typeparamref name="TContext"/>.</param>
        /// <returns></returns>
        public static IGraphClient WithAnnotations<TContext, TResolver>(this IGraphClient graphClient, EntityService entityService, out TContext context)
            where TContext : AnnotationsContext
            where TResolver : EntityResolver, new()
        {
            return SharedWithAnnotations<TContext, TResolver, EntityConverter>(graphClient, entityService, out context, useConverter: false);
        }



        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <see cref="AnnotationsContext"/> to the <see cref="IGraphClient"/> 
        /// using a converter of type <see cref="EntityConverter"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <returns></returns>
        public static IGraphClient WithAnnotationsConverter(this IGraphClient graphClient)
        {
            return WithAnnotationsConverter<AnnotationsContext>(graphClient);
        }

        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <see cref="AnnotationsContext"/> to the <see cref="IGraphClient"/> 
        /// using a converter of type <see cref="EntityConverter"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <param name="entityService">The service that provides entity information. Ideally, only one instance of <see cref="EntityService"/> should exist.
        /// This parameter is optional and can be null, however, you can get a new instance from <see cref="AnnotationsContext.CreateNewEntityService"/>.
        /// </param>
        /// <returns></returns>
        public static IGraphClient WithAnnotationsConverter(this IGraphClient graphClient, EntityService entityService)
        {
            return WithAnnotationsConverter<AnnotationsContext>(graphClient, entityService);
        }

        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <typeparamref name="TContext"/> to the <see cref="IGraphClient"/> 
        /// using a converter of type <see cref="EntityConverter"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <returns></returns>
        public static IGraphClient WithAnnotationsConverter<TContext>(this IGraphClient graphClient) where TContext : AnnotationsContext
        {
            return WithAnnotationsConverter<TContext, EntityConverter>(graphClient, null);
        }

        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <typeparamref name="TContext"/> to the <see cref="IGraphClient"/> 
        /// using a converter of type <see cref="EntityConverter"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <param name="entityService">The service that provides entity information. Ideally, only one instance of <see cref="EntityService"/> should exist.
        /// This parameter is optional and can be null, however, you can get a new instance from <see cref="AnnotationsContext.CreateNewEntityService"/>.
        /// </param>
        /// <returns></returns>
        public static IGraphClient WithAnnotationsConverter<TContext>(this IGraphClient graphClient, EntityService entityService)
            where TContext : AnnotationsContext
        {
            return WithAnnotationsConverter<TContext, EntityConverter>(graphClient, entityService);
        }

        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <typeparamref name="TContext"/> 
        /// using a converter of type <typeparamref name="TConverter"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <returns></returns>
        public static IGraphClient WithAnnotationsConverter<TContext, TConverter>(this IGraphClient graphClient)
            where TContext : AnnotationsContext
            where TConverter : EntityConverter, new()
        {
            return WithAnnotationsConverter<TContext, TConverter>(graphClient, null, out var context);
        }

        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <typeparamref name="TContext"/> 
        /// using a converter of type <typeparamref name="TConverter"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <param name="entityService">The service that provides entity information. Ideally, only one instance of <see cref="EntityService"/> should exist.
        /// This parameter is optional and can be null, however, you can get a new instance from <see cref="AnnotationsContext.CreateNewEntityService"/>.
        /// </param>
        /// <returns></returns>
        public static IGraphClient WithAnnotationsConverter<TContext, TConverter>
            (this IGraphClient graphClient, EntityService entityService)
            where TContext : AnnotationsContext
            where TConverter : EntityConverter, new()
        {
            return WithAnnotationsConverter<TContext, TConverter>(graphClient, entityService, out var context);
        }

        /// <summary>
        /// Attaches an <see cref="AnnotationsContext"/> of type <typeparamref name="TContext"/> 
        /// using a converter of type <typeparamref name="TConverter"/>.
        /// </summary>
        /// <param name="graphClient">The graph client.</param>
        /// <param name="entityService">The service that provides entity information. Ideally, only one instance of <see cref="EntityService"/> should exist.
        /// This parameter is optional and can be null, however, you can get a new instance from <see cref="AnnotationsContext.CreateNewEntityService"/>.
        /// </param>
        /// <param name="context">The newly created <typeparamref name="TContext"/>.</param>
        /// <returns></returns>
        public static IGraphClient WithAnnotationsConverter<TContext, TConverter>
            (this IGraphClient graphClient, EntityService entityService, out TContext context)
            where TContext : AnnotationsContext
            where TConverter : EntityConverter, new()
        {
            return SharedWithAnnotations<TContext, EntityResolver, TConverter>(graphClient, entityService, out context, useConverter: true);
        }



        internal static IGraphClient SharedWithAnnotations<TContext, TResolver, TConverter>
            (this IGraphClient graphClient, EntityService entityService, out TContext context, bool useConverter)
            where TContext : AnnotationsContext
            where TResolver : EntityResolver, new()
            where TConverter : EntityConverter, new()
        {
            if (graphClient == null)
            {
                throw new ArgumentNullException(nameof(graphClient));
            }

            context = graphClient.GetAnnotationsContext() as TContext;

            if (context != null 
                && (!useConverter ? context.EntityResolver?.GetType() == typeof(TResolver)
                : context.EntityConverter?.GetType() == typeof(TConverter))
                && (entityService == null || context.EntityService.GetType() == entityService.GetType()))
            {
                //already attached
                return graphClient;
            }

            TResolver resolver = null; TConverter converter = null;
            if (!useConverter)
                resolver = new TResolver();
            else
                converter = new TConverter();

            object resolverConverterObj = resolver ?? (object)converter;

            //find best constructor
            var parameters = new object[] { graphClient, resolverConverterObj, entityService }.Where(p => p != null).ToArray();

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var orderedConstructors = typeof(TContext).GetConstructors(flags)
                .OrderByDescending(c => c.GetParameters().Length)
                .Select(c => new
                {
                    ctor = c,
                    validParams = c.GetParameters().Select(p => parameters.FirstOrDefault
                        (ip => p.ParameterType.IsAssignableFrom(ip.GetType())))
                        .ToArray()
                })
                .OrderBy(nc => nc.validParams.Count(p => p == null))
                .ToArray();

            if (orderedConstructors?.Length > 0)
            {
                //find a constructor willing to create a valid instance with the available parameters
                foreach (var octor in orderedConstructors)
                {
                    try
                    {
                        context = octor.ctor.Invoke(octor.validParams) as TContext;
                        break;
                    }
                    catch
                    {

                    }
                }
            }

            if (context == null)
                throw new InvalidOperationException(string.Format(Messages.NoValidConstructorError, typeof(TContext).Name));
            
            return graphClient;
        }
    }
}
