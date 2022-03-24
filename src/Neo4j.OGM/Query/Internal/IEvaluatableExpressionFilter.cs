using System.Linq.Expressions;

namespace Neo4j.OGM.Query.Internal;

public interface IEvaluatableExpressionFilter
{
    bool IsEvaluatableExpression(Expression expression);
}
