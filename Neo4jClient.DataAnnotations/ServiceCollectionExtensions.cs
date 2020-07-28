using Neo4jClient.DataAnnotations;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static AnnotationsBuilder AddNeo4jAnnotations(this IServiceCollection services)
        {
            return AddNeo4jAnnotations<AnnotationsContext>(services);
        }

        public static AnnotationsBuilder AddNeo4jAnnotations<TContext>(this IServiceCollection services)
            where TContext : AnnotationsContext
        {
            try
            {
                services.AddSingleton<EntityService, EntityService>(provider =>
                {
                    return AnnotationsContext.CreateNewEntityService();
                });
            }
            catch
            {
            }

            services.AddScoped<AnnotationsContext, TContext>();

            return new AnnotationsBuilder(typeof(TContext), services);
        }
    }
}