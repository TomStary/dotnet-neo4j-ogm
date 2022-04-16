using System.Reflection;
using Neo4j.Driver;
using Neo4j.OGM.Context;
using Neo4j.OGM.Internals;
using Neo4j.OGM.Metadata;

namespace Neo4j.OGM;

/// <summary>
/// Session factory handler creating <see cref="ISession" /> instances.
/// </summary>
public class SessionFactory : IDisposable
{
    private readonly MetaData _metadata;
    private readonly IDriver _driver;
    private bool _disposed = false;

    /// <summary>
    /// SessionFactory constructor, create <see cref="IDriver"/> instance and creates an instance of <see cref="MetaData"/>.
    /// </summary>
    public SessionFactory(string connectionString, IAuthToken token, params Assembly[] assemblies)
    {
        _driver = GraphDatabase.Driver(connectionString, token);
        _metadata = new MetaData(assemblies);
    }

    /// <summary>
    /// Creates new <see cref="ISession"/> instance.
    /// </summary>
    public ISession Create()
    {
        CheckDisposed();
        // TODO: Register this to DIC
        var mappingContext = new MappingContext(_metadata);
        var entityGraphMapper = new EntityGraphMapper(mappingContext);
        return new Session(_metadata, _driver, entityGraphMapper);
    }

    #region IDisposeable

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _driver.Dispose();
            }
        }
        _disposed = true;
    }

    private void CheckDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name, "The session has been disposed.");
        }
    }

    #endregion /* IDisposeable */
}
