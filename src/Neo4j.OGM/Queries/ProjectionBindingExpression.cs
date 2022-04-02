using System.Linq.Expressions;

namespace Neo4j.OGM.Queries;

internal class ProjectionBindingExpression : Expression
{
    public ProjectionBindingExpression(
        Expression queryExpression,
        ProjectionMember projectionMember,
        Type type)
    {
        QueryExpression = queryExpression;
        ProjectionMember = projectionMember;
        Type = type;
    }

    public Expression QueryExpression { get; }
    public ProjectionMember ProjectionMember { get; }
    public override Type Type { get; }
    public sealed override ExpressionType NodeType
            => ExpressionType.Extension;
}
