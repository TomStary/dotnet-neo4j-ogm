using System.Linq.Expressions;
using System.Reflection;
using Neo4j.OGM.Queries;

namespace Neo4j.OGM;

public static class OGMIQueryableExtensions
{
    public static Task<TSource?> FirstOrDefaultAsync<TSource>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, bool>> predicate
    ) where TSource : class
    {
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null");
        }

        return ExecuteAsync<TSource, Task<TSource?>>(
            QueryableMethods.FirstOrDefaultWithPredicate, query, predicate
        );
    }

    public static async Task<List<TSource>> ToListAsync<TSource>(
        this IQueryable<TSource> source
    ) where TSource : class
    {
        var list = new List<TSource>();
        await foreach (var element in source.AsAsyncEnumerable().WithCancellation(default))
        {
            list.Add(element);
        }
        return list;
    }

    public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(
        this IQueryable<TSource> source
    ) where TSource : class
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "Source must not be null");
        }

        if (source is IAsyncEnumerable<TSource> asyncEnumerable)
        {
            return asyncEnumerable;
        }

        throw new InvalidOperationException("Cannot return an IAsyncEnumerable from a non-IAsyncEnumerable");
    }

    private static TResult ExecuteAsync<TSource, TResult>(
        MethodInfo operatorMethodInfo,
        IQueryable<TSource> source,
        Expression? expression
    ) where TSource : class
    {
        if (source.Provider is IAsyncQueryProvider provider)
        {
            if (operatorMethodInfo.IsGenericMethod)
            {
                operatorMethodInfo = operatorMethodInfo.GetGenericArguments().Length == 2
                    ? operatorMethodInfo.MakeGenericMethod(typeof(TSource), typeof(TResult).GetGenericArguments().Single())
                    : operatorMethodInfo.MakeGenericMethod(typeof(TSource));
            }

            return provider.ExecuteAsync<TResult>(
                Expression.Call(
                    instance: null,
                    method: operatorMethodInfo,
                    arguments: expression == null
                        ? new[] { source.Expression }
                        : new[] { source.Expression, expression }
                )
            );
        }

        throw new InvalidOperationException("The query provider is not an IAsyncQueryProvider");
    }
}
