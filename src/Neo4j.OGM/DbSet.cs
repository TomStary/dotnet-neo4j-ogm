using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Neo4j.OGM.Extensions.Internals;
using Neo4j.OGM.Queries;
using Neo4j.OGM.Queries.Internal;
using Neo4j.OGM.Utils;

namespace Neo4j.OGM;

public class DbSet<TEntity> : IQueryable<TEntity>, IAsyncEnumerable<TEntity> where TEntity : class
{
    private readonly ISession _session;
    private readonly EntityQueryProvider _provider;

    public Type ElementType { get; }
    public Expression Expression { get; }
    public IQueryProvider Provider => _provider;

    private readonly EntityQueryable<TEntity> _queryable;
    private EntityQueryable<TEntity> EntityQueryable => _queryable;

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

    public IEnumerator<TEntity> GetEnumerator() => throw new NotImplementedException();

    IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => throw new NotSupportedException();

    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

    IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetAsyncEnumerator(CancellationToken cancellationToken)
        => throw new NotSupportedException();
}
