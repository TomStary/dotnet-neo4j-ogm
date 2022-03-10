namespace Neo4j.OGM.Update;

public class UpdateCypherGenerator : IUpdateCypherGenerator
{
    public string CreateCreateOperation<TEntity>(TEntity entity, int depth) where TEntity : class
    {
        throw new NotImplementedException();
    }
}
