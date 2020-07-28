using System;
using Neo4jClient.DataAnnotations.Utils;

namespace Neo4jClient.DataAnnotations
{
    public abstract class EntitySet<T> : IHaveEntityInfo, IHaveEntityService
    {
        private EntityTypeInfo info;

        public EntitySet(EntityService entityService)
        {
            EntityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            entityService.AddEntityType(typeof(T));
        }

        public EntityTypeInfo EntityInfo
        {
            get
            {
                lock (this)
                {
                    if (info == null) info = EntityService.GetEntityTypeInfo(typeof(T));
                }

                return info;
            }
        }

        public EntityService EntityService { get; set; }
    }
}