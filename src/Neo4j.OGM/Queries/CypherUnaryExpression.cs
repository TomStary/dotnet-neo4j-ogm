using System.Linq.Expressions;
using Neo4j.OGM.Queries.CypherExpressions;

namespace Neo4j.OGM.Queries;

public class CypherUnaryExpression : CypherExpression
{
    private static readonly ISet<ExpressionType> _allowedOperators = new HashSet<ExpressionType>
    {
        ExpressionType.Equal,
    };

    public CypherUnaryExpression(
        ExpressionType operatorType,
        CypherExpression operand,
        Type type)
        : base(type)
    {
        if (!_allowedOperators.Contains(operatorType))
        {
            throw new InvalidOperationException("Invalid operator type.");
        }

        OperatorType = operatorType;
        Operand = operand;
    }

    public ExpressionType OperatorType { get; }
    public CypherExpression Operand { get; }
}
