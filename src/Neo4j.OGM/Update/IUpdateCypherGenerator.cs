namespace Neo4j.OGM.Update
{
    public interface IUpdateCypherGenerator
    {
        string CreateCreateOperation<TEntity>(TEntity entity, int depth) where TEntity : class;
        string CreateSetOperation<TEntity>(TEntity entity, int depth) where TEntity : class;
        string CreateDeleteOperation<TEntity>(TEntity entity, int depth) where TEntity : class;
    }
}