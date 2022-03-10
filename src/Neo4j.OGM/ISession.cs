namespace Neo4j.OGM;

public interface ISession : IDisposable
{
    Task Add<TEntity>(TEntity entity) where TEntity : class;
    Task AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    Task Update<TEntity>(TEntity entity) where TEntity : class;
    Task UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    Task Remove<TEntity>(TEntity entity) where TEntity : class;
    Task RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
}
