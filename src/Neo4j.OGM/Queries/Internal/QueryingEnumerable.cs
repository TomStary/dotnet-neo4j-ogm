using System.Collections;
using Neo4j.Driver;
using Neo4j.OGM.Internals;
using Neo4j.OGM.Requests;
using Neo4j.OGM.Utils;

namespace Neo4j.OGM.Queries.Internal;

internal sealed class QueryingEnumerable<T> : IAsyncEnumerable<T>, IEnumerable<T>
{
    private readonly QueryContext _queryContext;
    private readonly MatchExpression _matchExpression;
    private readonly MatchExpression _returnExpression;
    private readonly IResultCursor _innerEnumerable;
    private readonly Func<QueryContext, T> _shaper;
    private readonly Type _contextType;

    public QueryingEnumerable(
        QueryContext queryContext,
        MatchExpression matchExpression,
        Func<QueryContext, T> shaper,
        Type contextType)
    {
        _queryContext = queryContext;
        _matchExpression = matchExpression;
        _shaper = shaper;
        _contextType = contextType;
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new Enumerator(this, cancellationToken);

    public IEnumerator<T> GetEnumerator()
        => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public IStatement GenerateQuery() => new CypherExpressionVisitor().GenerateCypherQuery(_matchExpression);

    private sealed class Enumerator : IEnumerator<T>, IAsyncEnumerator<T>
    {
        private readonly QueryContext _queryContext;
        private readonly IResultCursor _innerEnumerable;
        private readonly Func<QueryContext, T> _shaper;
        private readonly Type _contextType;
        private readonly QueryingEnumerable<T> _queryingEnumerable;
        private readonly CancellationToken _cancellationToken;
        private IResultCursor? _enumerator;
        private IAsyncSession? _session;
        private IAsyncTransaction _transaction;

        public Enumerator(QueryingEnumerable<T> queryingEnumerable, CancellationToken cancellationToken = default)
        {
            _queryContext = queryingEnumerable._queryContext;
            _innerEnumerable = queryingEnumerable._innerEnumerable;
            _shaper = queryingEnumerable._shaper;
            _contextType = queryingEnumerable._contextType;
            _queryingEnumerable = queryingEnumerable;
            _cancellationToken = cancellationToken;
            Current = default!;
        }

        public T Current { get; private set; }

        object IEnumerator.Current
            => Current!;

        public ValueTask<bool> MoveNextAsync()
        {
            try
            {
                _cancellationToken.ThrowIfCancellationRequested();

                return new ValueTask<bool>(MoveNextHelper());
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        private async Task<bool> MoveNextHelper()
        {
            try
            {
                if (_enumerator == null)
                {
                    var cypherQuery = _queryingEnumerable.GenerateQuery();

                    _session = _queryContext.Context.GetDatabaseSession();
                    _transaction = await _session.BeginTransactionAsync();
                    _enumerator = await _transaction.RunAsync(cypherQuery.Statement, cypherQuery.Parameters);
                }

                var hasNext = await _enumerator.FetchAsync().ConfigureAwait(false);
                _queryContext.QueryIResultCursor = _enumerator;

                Current = hasNext
                    ? _shaper(_queryContext)
                    : default!;

                return hasNext;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void Dispose()
        {
            _enumerator = null;
        }

        public void Reset()
            => throw new NotSupportedException();

        public ValueTask DisposeAsync()
        {
            _enumerator = null;

            return default;
        }

        bool IEnumerator.MoveNext()
        {
            throw new NotImplementedException();
        }
    }
}
