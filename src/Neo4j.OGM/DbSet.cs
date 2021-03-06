using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Neo4j.OGM.Extensions.Internals;
using Neo4j.OGM.Queries;
using Neo4j.OGM.Queries.Internal;
using Neo4j.OGM.Utils;

namespace Neo4j.OGM;

public class DbSet<TEntity> : IQueryable<TEntity>, IAsyncEnumerable<TEntity> where TEntity : class
{
#pragma warning disable IDE0052 // TODO: Remove pragma if it becomes obsolete.
    private readonly ISession _session;
#pragma warning restore IDE0052
    private readonly EntityQueryProvider _provider;
    private readonly EntityQueryable<TEntity> _queryable;
    private EntityQueryable<TEntity> EntityQueryable => _queryable;

    [ExcludeFromCodeCoverage]
    public Type ElementType => EntityQueryable.ElementType;

    [ExcludeFromCodeCoverage]
    public Expression Expression => EntityQueryable.Expression;

    public IQueryProvider Provider => _provider;

    Expression IQueryable.Expression
            => EntityQueryable.Expression;

    public DbSet(ISession session)
    {
        _session = session;
        _provider = new EntityQueryProvider(session);
        _queryable = new EntityQueryable<TEntity>(_provider, typeof(TEntity));
    }

    public virtual Task<TEntity?> FindAsync(params object?[]? keyValues)
    {
        if (keyValues == null
            || keyValues.Any(key => key == null))
        {
            return Task.FromResult<TEntity?>(default);
        }

        //TODO: move to extension
        var keyProperties = typeof(TEntity).GetProperties()
            .Where(property => property.GetCustomAttribute<KeyAttribute>() != null)
            .ToArray();

        if (keyProperties.Length != keyValues.Length)
        {
            throw new ArgumentException("Incorrect number of key values");
        }

        return this.FirstOrDefaultAsync(BuildLambda(keyProperties, new ValueBuffer(keyValues)));
    }

    private static Expression<Func<TEntity, bool>> BuildLambda(PropertyInfo[] keyProperties, ValueBuffer keyValues)
    {
        var entityParameter = Expression.Parameter(typeof(TEntity), "e");

        return Expression.Lambda<Func<TEntity, bool>>(
            ExpressionExtensions.BuildPredicate(keyProperties, keyValues, entityParameter), entityParameter
        );
    }

    [ExcludeFromCodeCoverage]
    IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => EntityQueryable.GetEnumerator();

    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => EntityQueryable.GetEnumerator();

    [ExcludeFromCodeCoverage]
    IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetAsyncEnumerator(CancellationToken cancellationToken)
        => EntityQueryable.GetAsyncEnumerator(cancellationToken);
}
