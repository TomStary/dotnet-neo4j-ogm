using System.Linq.Expressions;
using System.Reflection;
using Neo4j.Driver;
using Neo4j.OGM.Extensions.Internals;
using Neo4j.OGM.Internals.Extensions;
using Neo4j.OGM.Queries.Internal;
using Neo4j.OGM.Utils;

namespace Neo4j.OGM.Queries;

// Influenced by: https://github.com/dotnet/efcore
internal class ShapedQueryCompilingExpressionVisitor : ExpressionVisitor
{
    internal ShapedQueryCompilingExpressionVisitor(
        QueryCompilationContext queryCompilationContext
    )
    {
        QueryCompilationContext = queryCompilationContext;

    }

    /// <summary>
    ///     The query compilation context object for current compilation.
    /// </summary>
    protected virtual QueryCompilationContext QueryCompilationContext { get; }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
        {
            var serverEnumerable = VisitShapedQuery(shapedQueryExpression);
            switch (shapedQueryExpression.ResultCardinality)
            {
                case ResultCardinality.Enumerable:
                    return serverEnumerable;

                case ResultCardinality.Single:
                    return QueryCompilationContext.IsAsync
                        ? Expression.Call(
                            _singleAsyncMethodInfo.MakeGenericMethod(serverEnumerable.Type.GetSequenceType()),
                            serverEnumerable,
                            Expression.Constant(CancellationToken.None))
                        : Expression.Call(
                            EnumerableMethods.SingleWithoutPredicate.MakeGenericMethod(serverEnumerable.Type.GetSequenceType()),
                            serverEnumerable);
                case ResultCardinality.SingleOrDefault:
                    return QueryCompilationContext.IsAsync
                        ? Expression.Call(
                            _singleOrDefaultAsyncMethodInfo.MakeGenericMethod(serverEnumerable.Type.GetSequenceType()),
                            serverEnumerable,
                            Expression.Constant(CancellationToken.None))
                        : Expression.Call(
                            EnumerableMethods.SingleOrDefaultWithoutPredicate.MakeGenericMethod(
                                serverEnumerable.Type.GetSequenceType()),
                            serverEnumerable);
            }
        }

        return base.VisitExtension(extensionExpression);
    }

    private static readonly MethodInfo _singleAsyncMethodInfo
        = typeof(ShapedQueryCompilingExpressionVisitor).GetTypeInfo()
            .GetDeclaredMethods(nameof(SingleAsync))
            .Single(mi => mi.GetParameters().Length == 2);

    private static readonly MethodInfo _singleOrDefaultAsyncMethodInfo
        = typeof(ShapedQueryCompilingExpressionVisitor).GetTypeInfo()
            .GetDeclaredMethods(nameof(SingleOrDefaultAsync))
            .Single(mi => mi.GetParameters().Length == 2);

    private static async Task<TSource> SingleAsync<TSource>(
        IAsyncEnumerable<TSource> asyncEnumerable,
        CancellationToken cancellationToken = default)
    {
        var enumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
        await using var _ = enumerator.ConfigureAwait(false);

        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            throw new InvalidOperationException();
        }

        var result = enumerator.Current;

        if (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            throw new InvalidOperationException();
        }

        return result;
    }

    private static async Task<TSource> SingleOrDefaultAsync<TSource>(
        IAsyncEnumerable<TSource> asyncEnumerable,
        CancellationToken cancellationToken = default)
    {
        var enumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
        await using var _ = enumerator.ConfigureAwait(false);

        if (!(await enumerator.MoveNextAsync().ConfigureAwait(false)))
        {
            return default!;
        }

        var result = enumerator.Current;

        if (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            throw new InvalidOperationException();
        }

        return result;
    }

    /// <summary>
    ///     Visits given shaped query expression to create an expression of enumerable.
    /// </summary>
    /// <param name="shapedQueryExpression">The shaped query expression to compile.</param>
    /// <returns>An expression of enumerable.</returns>
    protected Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        var resultCursor = Expression.Parameter(typeof(IResultCursor), "resultCursor");

        var shaperBody = shapedQueryExpression.ShaperExpression;
        shaperBody = new ResultCursorInjectingExpressionVisitor().Visit(shaperBody);

        switch (shapedQueryExpression.QueryExpression)
        {
            case MatchExpression matchExpression:
                var shaperLambda = Expression.Lambda(shaperBody, QueryCompilationContext.QueryContextParameter);

                return Expression.New(
                    typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                    QueryCompilationContext.QueryContextParameter,
                    Expression.Constant(matchExpression),
                    Expression.Constant(shaperLambda.Compile()),
                    Expression.Constant(matchExpression.EntityType));
            default:
                throw new NotSupportedException($"{shapedQueryExpression.QueryExpression.GetType().Name} is not supported.");

        }
    }

    private class ResultCursorInjectingExpressionVisitor : ExpressionVisitor
    {
        private MethodInfo _mapValuesToTypeMethodInfo = typeof(IRecordExtension).GetTypeInfo()
            .GetDeclaredMethod(nameof(IRecordExtension.MapRecordToType))!;
        private MethodInfo _getCurrentResultCursorMethodInfo = typeof(IResultCursor).GetTypeInfo()
            .GetDeclaredProperty(nameof(IResultCursor.Current))!.GetGetMethod()!;

        private MethodInfo _getResultCursorMethodInfo = typeof(QueryContext).GetTypeInfo()
            .GetDeclaredProperty(nameof(QueryContext.QueryIResultCursor))!.GetGetMethod()!;

        public ResultCursorInjectingExpressionVisitor()
        {
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case EntityShaperExpression shaperExpression:
                    // translate IRecord to Entity projection
                    var valueBufferExpression = Visit(shaperExpression.ValueBufferExpression);

                    return valueBufferExpression;

                case ProjectionBindingExpression projectionBindingExpression:
                    /*
                        pseudocode to the rescue
                        record comes from IResultCursor.Current
                        if (record == null)
                            return null
                        var result = IRecordExtension.MapValuesToType(record, alias, typeof(TEntity));

                        return result;
                    */
                    var vars = new List<ParameterExpression>
                    {
                        Expression.Variable(typeof(IRecord), "record"),
                        Expression.Variable(((MatchExpression)projectionBindingExpression.QueryExpression).ReturnExpression.Type, "result"),
                    };

                    var exprs = new List<Expression>
                    {
                        Expression.Assign(vars[0], Expression.Call(
                            Expression.Call(QueryCompilationContext.QueryContextParameter, _getResultCursorMethodInfo), _getCurrentResultCursorMethodInfo)),
                        Expression.Condition(
                            Expression.Equal(vars[0], Expression.Constant(null, typeof(IRecord))),
                            Expression.Assign(vars[1], Expression.Constant(null, ((MatchExpression)projectionBindingExpression.QueryExpression).ReturnExpression.Type)),
                            Expression.Assign(vars[1], Expression.Call(
                                _mapValuesToTypeMethodInfo.MakeGenericMethod(((MatchExpression)projectionBindingExpression.QueryExpression).ReturnExpression.Type),
                                vars[0],
                                Expression.Constant(((MatchExpression)projectionBindingExpression.QueryExpression).ReturnExpression.Alias))
                        )),
                    };

                    return Expression.Block(((MatchExpression)projectionBindingExpression.QueryExpression).ReturnExpression.Type, vars, exprs);
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
