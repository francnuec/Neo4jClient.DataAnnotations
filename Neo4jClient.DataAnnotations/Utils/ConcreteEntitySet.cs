namespace Neo4jClient.DataAnnotations.Utils
{
    internal class ConcreteEntitySet<T> : EntitySet<T>
    {
        public ConcreteEntitySet(DataAnnotations.EntityService entityService) : base(entityService)
        {
        }
    }
}
