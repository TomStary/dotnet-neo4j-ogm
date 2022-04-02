using System.Linq.Expressions;

namespace Neo4j.OGM.Queries;

internal class ReturnExpression : Expression
{
    public override Type Type { get; }
    public string Alias { get; }

    public override ExpressionType NodeType => ExpressionType.Extension;

    public ReturnExpression(Type entityType, string rootAlias)
    {
        Type = entityType;
        Alias = rootAlias;
    }

    public override string ToString()
    {
        return $"{Alias}";
    }
}
