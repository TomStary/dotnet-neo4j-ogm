using Neo4j.Driver;
using Neo4j.OGM.Metadata;

namespace Neo4j.OGM;

public interface ISession : IDisposable
{
    Task SaveAsync<TEntity>(TEntity entity) where TEntity : class;

    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    IAsyncSession GetDatabaseSession();
}
