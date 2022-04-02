using System.Reflection;
using Neo4j.Driver;
using Neo4j.OGM.Context;
using Neo4j.OGM.Metadata;
using Neo4j.OGM.Requests;

namespace Neo4j.OGM.Internals;

/// <summary>
/// Concrete implementation of <see cref="ISession"/>.
/// </summary>
public class Session : ISession
{
    private readonly MetaData _metadata;
    private readonly IDriver _driver;
    private readonly SessionSaveDelegate _saveDelegate;
    private readonly MappingContext _mappingContext;

    public MetaData Metadata => _metadata;

    internal MappingContext MappingContext => _mappingContext;

    public Session(MetaData metadata, IDriver driver)
    {
        _metadata = metadata;
        _driver = driver;
        _saveDelegate = new SessionSaveDelegate(this);
        _mappingContext = new MappingContext(metadata);
    }

    public Session(string connectionString, params Assembly[] assemblies)
    {
        _metadata = new MetaData(assemblies);
        _driver = GraphDatabase.Driver(connectionString);
        _saveDelegate = new SessionSaveDelegate(this);
        _mappingContext = new MappingContext(_metadata);
    }

    public Task SaveAsync<TEntity>(TEntity entity) where TEntity : class
    {
        return _saveDelegate.SaveAsync(entity);
    }

    public DbSet<TEntity> Set<TEntity>() where TEntity : class => new(this);

    internal async Task<IResultCursor> GetResultCursorAsync(IStatement statement)
    {
        using var session = _driver.AsyncSession();
        using var transaction = await session.BeginTransactionAsync();
        var cursor = await transaction.RunAsync(statement.Statement, statement.Parameters);
        return cursor;
    }

    public IAsyncSession GetDatabaseSession()
    {
        return _driver.AsyncSession();
    }

    internal async Task<object> ExecuteAsync(DefaulterRequest defualtRequest)
    {
        var queries = CreateQueries(defualtRequest);
        using var session = _driver.AsyncSession();
        using var transaction = await session.BeginTransactionAsync();
        var resultList = new List<object>();
        try
        {
            foreach (var query in queries)
            {
                var resultSet = await transaction.RunAsync(query);
                resultList.Add(resultSet);
            }
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw new Exception(e.Message, e);
        }
        transaction.Dispose();
        session.Dispose();

        return resultList;
    }

    private static IEnumerable<Query> CreateQueries(DefaulterRequest defualtRequest)
    {
        var queries = new List<Query>();
        foreach (var statement in defualtRequest.Statements)
        {
            queries.Add(new Query(statement.Statement, statement.Parameters));
        }

        return queries;
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
