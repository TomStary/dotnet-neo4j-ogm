using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Neo4j.OGM.Queries.Internal;

public class EntityQueryable<TResult>
    : IOrderedQueryable<TResult>,
            IAsyncEnumerable<TResult>,
            IListSource
{
    private readonly IAsyncQueryProvider _queryProvider;

    public Type ElementType => throw new NotImplementedException();

    public virtual Expression Expression { get; }

    public IQueryProvider Provider => throw new NotImplementedException();

    public bool ContainsListCollection => throw new NotImplementedException();

    public EntityQueryable(IAsyncQueryProvider queryProvider, Type entityType)
        : this(queryProvider, new QueryRootExpression(queryProvider, entityType))
    { }

    public EntityQueryable(IAsyncQueryProvider queryProvider, Expression expression)
    {
        _queryProvider = queryProvider;
        Expression = expression;
    }

    public IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<TResult> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public IList GetList()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
