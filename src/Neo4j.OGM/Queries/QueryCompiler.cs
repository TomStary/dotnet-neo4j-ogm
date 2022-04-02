using System.Linq.Expressions;
using Neo4j.OGM.Queries.Internal;

namespace Neo4j.OGM.Queries;

public class QueryCompiler
{
    private readonly ISession _session;

    public QueryCompiler(ISession session)
    {
        _session = session;
    }

    public virtual TResult ExecuteAsync<TResult>(Expression query)
    {
        var queryContext = new QueryContext(_session);

        query = ExtractParameters(query, queryContext);

        var compiledQuery = CompileQueryCore<TResult>(query, true);

        return compiledQuery(queryContext);
    }

    private Expression ExtractParameters(Expression query, IParameterValues paramaterValues, bool parametrize = true)
    {
        var visitor = new ParameterExtractingExpressionVisitor(paramaterValues, parametrize);

        return visitor.ExtractParameters(query, true);
    }

    private Func<QueryContext, TResult> CompileQueryCore<TResult>(Expression query, bool isAsync)
        => new QueryCompilationContext(isAsync).CreateQueryExecutor<TResult>(query);
}
