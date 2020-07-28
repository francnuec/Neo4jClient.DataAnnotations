namespace Neo4jClient.DataAnnotations.Utils
{
    internal class ConcreteEntitySet<T> : EntitySet<T>
    {
        public ConcreteEntitySet(EntityService entityService) : base(entityService)
        {
        }
    }
}