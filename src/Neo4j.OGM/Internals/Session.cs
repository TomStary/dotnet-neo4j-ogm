using Neo4j.Driver;
using Neo4j.OGM.Metadata;

namespace Neo4j.OGM.Internals
{
    /// <summary>
    /// Concrete implementation of <see cref="ISession"/>.
    /// </summary>
    public class Session : ISession
    {
        private readonly MetaData metadata;
        private readonly IDriver driver;

        public Session(MetaData metadata, IDriver driver)
        {
            this.metadata = metadata;
            this.driver = driver;
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Session()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            // Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public Task Add<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public Task AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public Task Remove<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public Task RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public Task Update<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public Task UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            throw new NotImplementedException();
        }
    }
}