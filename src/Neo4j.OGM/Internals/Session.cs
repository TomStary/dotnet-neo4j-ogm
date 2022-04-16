using System.Collections;
using Neo4j.Driver;
using Neo4j.OGM.Context;
using Neo4j.OGM.Cypher.Compilers;
using Neo4j.OGM.Internals.Extensions;
using Neo4j.OGM.Metadata;
using Neo4j.OGM.Requests;

namespace Neo4j.OGM.Internals;

/// <summary>
/// Concrete implementation of <see cref="ISession"/>.
/// </summary>
public class Session : ISession
{
    private MetaData _metadata;
    private IDriver _driver;
    private IEntityMapper _entityGraphMapper;
    internal MetaData Metadata
    {
        get
        {
            CheckDisposed();
            return _metadata;
        }
    }

    private bool _disposed = false;

    internal Session(MetaData metadata, IDriver driver, IEntityMapper entityGraphMapper)
    {
        _metadata = metadata;
        _driver = driver;
        _entityGraphMapper = entityGraphMapper;
    }

    /// <summary>
    /// Save entity or list of entities to the database.
    /// </summary>
    public async Task SaveAsync<TEntity>(TEntity entity) where TEntity : class
    {
        CheckDisposed();

        // tranform entity/entities into array
        IEnumerable objects;
        if (typeof(IEnumerable).IsAssignableFrom(entity.GetType()))
        {
            objects = (IEnumerable)entity;
        }
        else
        {
            objects = new[] { entity };
        }

        // map objects into graph and create statements
        foreach (var item in objects)
        {
            _entityGraphMapper.Map(item, -1);
        }

        // execute statements
        await ExecuteSave(_entityGraphMapper.CompilerContext());
    }


    public DbSet<TEntity> Set<TEntity>() where TEntity : class
    {
        CheckDisposed();

        return new(this);
    }

    public IAsyncSession GetDatabaseSession()
    {
        CheckDisposed();

        return _driver.AsyncSession();
    }

    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _metadata = null;
        _driver = null;
        _entityGraphMapper = null;
        _disposed = true;
    }

    private async Task<object> ExecuteAsync(DefaulterRequest defualtRequest)
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

    private async Task ExecuteSave(ICompilerContext context)
    {
        var compiler = context.Compiler;
        compiler.UseStatementFactory(new RowStatementFactory());

        if (compiler.HasStatementDependentOnNewNode())
        {
            await ExecuteStatementsAsync(context, compiler.CreateNodesStatements());

            var statements = new List<IStatement>();
            statements.AddRange(compiler.CreateRelationshipsStatements());
            statements.AddRange(compiler.UpdateNodesStatements());
            statements.AddRange(compiler.UpdateRelationshipsStatements());

            await ExecuteStatementsAsync(context, statements);
        }
        else
        {
            await ExecuteStatementsAsync(context, compiler.GetAllStatements());
        }
    }

    private async Task ExecuteStatementsAsync(ICompilerContext context, IEnumerable<IStatement> statements)
    {
        if (statements.Count() > 0)
        {
            var defualtRequest = new DefaulterRequest(statements);
            try
            {
                var response = await ExecuteAsync(defualtRequest);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
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

    private void CheckDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name, "The session has been disposed.");
        }
    }
}
