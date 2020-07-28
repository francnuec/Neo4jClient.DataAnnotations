using System;
using Microsoft.Extensions.DependencyInjection;
using Neo4jClient.DataAnnotations.Serialization;

namespace Neo4jClient.DataAnnotations
{
    public class AnnotationsBuilder
    {
        public AnnotationsBuilder(Type contextType, IServiceCollection services)
        {
            ContextType = contextType ?? throw new ArgumentNullException(nameof(contextType));
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        private IServiceCollection Services { get; }
        private Type ContextType { get; }

        public IServiceCollection WithResolver()
        {
            return WithResolver<EntityResolver>();
        }

        public IServiceCollection WithResolver<TResolver>() where TResolver : EntityResolver
        {
            Services.AddScoped<EntityResolver, TResolver>();
            return Services;
        }

        public IServiceCollection WithConverter()
        {
            return WithConverter<EntityConverter>();
        }

        public IServiceCollection WithConverter<TConverter>() where TConverter : EntityConverter
        {
            Services.AddScoped<EntityConverter, TConverter>();
            return Services;
        }
    }
}