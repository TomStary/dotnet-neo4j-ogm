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
    }
}