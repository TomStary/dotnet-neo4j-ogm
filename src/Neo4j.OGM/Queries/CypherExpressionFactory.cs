using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Neo4j.OGM.Queries.CypherExpressions;

namespace Neo4j.OGM.Queries;

internal class CypherExpressionFactory
{
    internal MatchExpression Match(Type entityType)
    {
        var matchExpression = new MatchExpression(entityType);

        return matchExpression;
    }

    internal Expression MakeBinary(ExpressionType operatorType, CypherExpression left, CypherExpression right)
    {
        var returnType = right.Type;
        switch (operatorType)
        {
            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
                returnType = typeof(bool);
                break;
        }

        return new CypherBinaryExpression(operatorType, left, right, returnType);
    }

    [ExcludeFromCodeCoverage(Justification = "This is not a part of POC.")]
    internal CypherExpression Equal(CypherExpression left, CypherExpression right)
    {
        throw new NotImplementedException();
    }

    [ExcludeFromCodeCoverage(Justification = "This is not a part of POC.")]
    internal CypherExpression Constant(bool v)
    {
        throw new NotImplementedException();
    }
}
