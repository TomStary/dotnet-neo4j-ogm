using System.Linq.Expressions;

namespace Neo4j.OGM.Queries;

internal class EntityShaperExpression : Expression
{
    public EntityShaperExpression(Type entityType, Expression valueBufferExpression, bool nullable)
    {
        Type = entityType;
        ValueBufferExpression = valueBufferExpression;
        IsNullable = nullable;
    }

    public override Type Type { get; }
    public Expression ValueBufferExpression { get; }
    public bool IsNullable { get; }

    public sealed override ExpressionType NodeType
       => ExpressionType.Extension;
}

