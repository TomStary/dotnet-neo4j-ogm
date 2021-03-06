using System.Linq.Expressions;
using Neo4j.OGM.Queries.Internal;

namespace Neo4j.OGM.Queries;


public class QueryCompilationContext
{
    public const string QueryParameterPrefix = "param_";
    public static readonly ParameterExpression QueryContextParameter = Expression.Parameter(typeof(QueryContext), "queryContext");
    public bool IsAsync { get; }

    public QueryCompilationContext(bool isAsync)
    {
        IsAsync = isAsync;
    }

    public virtual Func<QueryContext, TResult> CreateQueryExecutor<TResult>(Expression query)
    {
        query = new QueryableMethodTranslationExpressionVisitor(this).Visit(query);

        query = new ShapedQueryCompilingExpressionVisitor(this).Visit(query);

        var queryExecutorExpression = Expression.Lambda<Func<QueryContext, TResult>>(query, QueryContextParameter);

        try
        {
            return queryExecutorExpression.Compile();
        }
        finally
        {
            _ = true; // maybe implementation
        }
    }
}
