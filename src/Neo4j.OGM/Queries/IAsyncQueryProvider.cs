using System.Linq.Expressions;

namespace Neo4j.OGM.Queries;

public interface IAsyncQueryProvider : IQueryProvider
{
    TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default);
}
