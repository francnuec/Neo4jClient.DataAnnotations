namespace Neo4jClient.DataAnnotations
{
    public interface IHaveAnnotationsContext : IHaveEntityService
    {
        AnnotationsContext AnnotationsContext { get; }
    }
}