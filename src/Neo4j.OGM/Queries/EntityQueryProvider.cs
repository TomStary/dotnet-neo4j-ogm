using System.Linq.Expressions;

namespace Neo4j.OGM.Queries;

public class EntityQueryProvider : IAsyncQueryProvider
{
    private QueryCompiler _queryCompiler;
    private readonly ISession _session;

    public EntityQueryProvider(ISession session)
    {
        _session = session;
        _queryCompiler = new QueryCompiler(session);
    }

    public IQueryable CreateQuery(Expression expression)
    {
        throw new NotSupportedException();
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        throw new NotSupportedException();
    }

    public object? Execute(Expression expression)
    {
        throw new NotSupportedException();
    }

    public TResult Execute<TResult>(Expression expression)
    {
        throw new NotSupportedException();
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        => _queryCompiler.ExecuteAsync<TResult>(expression);
}
