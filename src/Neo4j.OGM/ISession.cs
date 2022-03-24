namespace Neo4j.OGM;

public interface ISession : IDisposable
{
    Task SaveAsync<TEntity>(TEntity entity) where TEntity : class;

    DbSet<TEntity> Set<TEntity>(TEntity entity) where TEntity : class;
}
