namespace Neo4jClient.DataAnnotations.Cypher
{
    /// <summary>
    ///     Mostly dummy interface. It allows us to execute some cypher query methods with annotations.
    /// </summary>
    public interface IAnnotatedQuery : IAnnotated
    {
    }

    public interface IAnnotatedQuery<TResult> : IAnnotatedQuery
    {
    }

    public interface IOrderedAnnotatedQuery : IAnnotatedQuery
    {
    }
}