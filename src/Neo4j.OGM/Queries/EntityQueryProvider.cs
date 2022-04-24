using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Neo4j.OGM.Internals.Extensions;
using Neo4j.OGM.Queries.Internal;

namespace Neo4j.OGM.Queries;

public class EntityQueryProvider : IAsyncQueryProvider
{
    private static readonly MethodInfo _genericCreateQueryMethod
        = typeof(EntityQueryProvider).GetRuntimeMethods()
            .Single(m => (m.Name == "CreateQuery") && m.IsGenericMethod);

    private QueryCompiler _queryCompiler;
    private readonly ISession _session;

    public EntityQueryProvider(ISession session)
    {
        _session = session;
        _queryCompiler = new QueryCompiler(session);
    }

    [ExcludeFromCodeCoverage(Justification = "This is not a part of POC.")]
    public IQueryable CreateQuery(Expression expression)
        => (IQueryable)_genericCreateQueryMethod
                .MakeGenericMethod(expression.Type.GetSequenceType())
                .Invoke(this, new object[] { expression })!;

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new EntityQueryable<TElement>(this, expression);


    [ExcludeFromCodeCoverage(Justification = "This is not a part of POC.")]
    public object? Execute(Expression expression)
    {
        throw new NotSupportedException();
    }

    [ExcludeFromCodeCoverage(Justification = "This is not a part of POC.")]
    public TResult Execute<TResult>(Expression expression)
    {
        throw new NotSupportedException();
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        => _queryCompiler.ExecuteAsync<TResult>(expression);
}
