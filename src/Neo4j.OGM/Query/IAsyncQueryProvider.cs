using System.Linq.Expressions;

namespace Neo4j.OGM.Query;

public interface IAsyncQueryProvider : IQueryProvider
{
    TResult ExecuteAsync<TResult>(Expression expression);
}
