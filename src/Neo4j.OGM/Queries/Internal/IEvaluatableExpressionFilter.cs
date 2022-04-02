using System.Linq.Expressions;

namespace Neo4j.OGM.Queries.Internal;

public interface IEvaluatableExpressionFilter
{
    bool IsEvaluatableExpression(Expression expression);
}
