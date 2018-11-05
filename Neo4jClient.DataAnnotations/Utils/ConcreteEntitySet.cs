namespace Neo4jClient.DataAnnotations.Utils
{
    internal class ConcreteEntitySet<T> : EntitySet<T>
    {
        public ConcreteEntitySet(IEntityService entityService) : base(entityService)
        {
        }
    }
}
