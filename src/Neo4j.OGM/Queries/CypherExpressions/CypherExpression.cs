using System.Linq.Expressions;

namespace Neo4j.OGM.Queries.CypherExpressions;

public class CypherExpression : Expression
{
    public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

    public CypherExpression(Type type, CypherTypeMapping? typeMapping)
    {
        Type = type;
        TypeMapping = typeMapping;
    }

    public override Type Type { get; }
    public CypherTypeMapping? TypeMapping { get; internal set; }
}
