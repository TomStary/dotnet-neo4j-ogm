using Neo4j.Driver;

namespace Neo4j.OGM
{
    public class Session : IDisposable
    {
        //TODO: implementation
        private bool _disposed = true;

        private readonly IDriver _driver;

        public Session(
            IDriver driver
        )
        {
            _driver = driver;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _driver.Dispose();
            }

            _disposed = true;
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
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}