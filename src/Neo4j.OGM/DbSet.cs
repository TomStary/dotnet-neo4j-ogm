using System.Collections;
using System.Linq.Expressions;

namespace Neo4j.OGM;

public abstract class DbSet<TEntity> : IQueryable<TEntity> where TEntity : class
{
    public abstract Type ElementType { get; }
    public abstract Expression Expression { get; }
    public abstract IQueryProvider Provider { get; }

    public abstract IEnumerator<TEntity> GetEnumerator();

    IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => throw new NotSupportedException();

    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
}
