using System.Linq.Expressions;
using Neo4j.OGM.Queries.CypherExpressions;

namespace Neo4j.OGM.Queries;

internal class CypherBinaryExpression : CypherExpression
{
#pragma warning disable IDE1006
    private static readonly ISet<ExpressionType> AllowedOperators = new HashSet<ExpressionType>
    {
        ExpressionType.AndAlso,
        ExpressionType.Equal,
    };
#pragma warning restore IDE1006

    public virtual ExpressionType OperatorType { get; }

    public virtual CypherExpression Left { get; }

    public virtual CypherExpression Right { get; }

    internal static bool IsValidOperator(ExpressionType operatorType)
        => AllowedOperators.Contains(operatorType);

    public CypherBinaryExpression(ExpressionType operatorType, CypherExpression left, CypherExpression right, Type type)
        : base(type)
    {
        if (!IsValidOperator(operatorType))
        {
            throw new InvalidOperationException("Invalid operator type.");
        }

        OperatorType = operatorType;
        Left = left;
        Right = right;
    }
}
