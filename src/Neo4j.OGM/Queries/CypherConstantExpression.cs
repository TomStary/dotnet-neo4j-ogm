using System.Linq.Expressions;
using Neo4j.OGM.Queries.CypherExpressions;

namespace Neo4j.OGM.Queries;

internal class CypherConstantExpression : CypherExpression
{
    private readonly ConstantExpression _constantExpression;

    internal CypherConstantExpression(ConstantExpression constantExpression)
        : base(constantExpression.Type)
    {
        _constantExpression = constantExpression;
    }

    public virtual object? Value
            => _constantExpression.Value;
}
