using System.Linq.Expressions;
using Neo4j.OGM.Queries.CypherExpressions;

namespace Neo4j.OGM.Queries;

internal class MatchExpression : Expression
{
    private const string RootAlias = "c";

    public CypherExpression? Limit { get; private set; }

    public CypherExpression? Predicate { get; private set; }

    private readonly Expression _projectionMapping = null!;

    public override Type Type
            => typeof(object);

    public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

    public Type EntityType { get; }
    public ReturnExpression ReturnExpression { get; }

    internal MatchExpression(Type entityType)
    {
        EntityType = entityType;
        ReturnExpression = new ReturnExpression(entityType, RootAlias);
        _projectionMapping = new EntityProjectionExpression(entityType, ReturnExpression);
    }

    internal void ApplyLimit(CypherExpression? cypherExpression)
    {
        Limit = cypherExpression;
    }

    internal void ApplyPredicate(CypherExpression translation)
    {
        Predicate = Predicate == null
            ? translation
            : new CypherBinaryExpression(
                ExpressionType.AndAlso,
                Predicate,
                translation,
                typeof(bool));
    }

    internal Expression GetMappedProjection()
    {
        return _projectionMapping;
    }
}
