using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Neo4j.OGM.Queries;

internal class ProjectionBindingExpression : Expression
{
    public ProjectionBindingExpression(
        Expression queryExpression,
        Type type)
    {
        QueryExpression = queryExpression;
        Type = type;
    }

    public Expression QueryExpression { get; }

    [ExcludeFromCodeCoverage]
    public override Type Type { get; }

    [ExcludeFromCodeCoverage]
    public sealed override ExpressionType NodeType
            => ExpressionType.Extension;
}
