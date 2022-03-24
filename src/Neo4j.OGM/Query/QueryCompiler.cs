using System.Linq.Expressions;
using Neo4j.OGM.Query.Internal;

namespace Neo4j.OGM.Query;

public class QueryCompiler
{
    public QueryCompiler()
    { }

    public virtual TResult ExecuteAsync<TResult>(Expression query)
    {
        var queryContext = new QueryContext();

        query = ExtractParameters(query, queryContext);

        var compiledQuery = CompileQueryCore<TResult>(query, true);

        return compiledQuery(queryContext);
    }

    private Expression ExtractParameters(Expression query, IParameterValues paramaterValues, bool parametrize = true, bool generateContextAcessor = false)
    {
        var visitor = new ParameterExtractingExpressionVisitor(paramaterValues, parametrize, generateContextAcessor);

        return visitor.ExtractParameters(query, true);
    }

    private Func<QueryContext, TResult> CompileQueryCore<TResult>(Expression query, bool isAsync)
    {
        query = new QueryableMethodTranslationExpressionVisitor().Translate(query);
    }
}
