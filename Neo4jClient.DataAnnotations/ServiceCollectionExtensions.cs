using Neo4jClient;
using Neo4jClient.DataAnnotations;
using System;
using Neo4jClient.DataAnnotations.Utils;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static AnnotationsBuilder AddNeo4jAnnotations(this IServiceCollection services)
        {
            return AddNeo4jAnnotations<AnnotationsContext>(services);
        }

        public static AnnotationsBuilder AddNeo4jAnnotations<TContext>(this IServiceCollection services) where TContext : AnnotationsContext
        {
            services.TryAddSingleton<EntityService, EntityService>();
            services.AddScoped<AnnotationsContext, TContext>();

            return new AnnotationsBuilder(typeof(TContext), services);
        }
    }
}
