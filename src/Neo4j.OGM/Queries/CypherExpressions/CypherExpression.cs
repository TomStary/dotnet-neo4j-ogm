using System.Linq.Expressions;

namespace Neo4j.OGM.Queries.CypherExpressions;

public class CypherExpression : Expression
{
    public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

    public CypherExpression(Type type)
    {
        Type = type;
    }

    public override Type Type { get; }
}
