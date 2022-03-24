using System.Linq.Expressions;

namespace Neo4j.OGM.Internals.Extensions;

internal static class ExpressionExtensions
{
    internal static LambdaExpression UnwrapLambdaFromQuote(this Expression expression)
    {
        return (LambdaExpression)(expression is UnaryExpression unary && expression.NodeType == ExpressionType.Quote
                ? unary.Operand
                : expression);
    }

    internal static Expression? UnwrapTypeConversion(this Expression? expression, out Type? convertedType)
    {
        convertedType = null;
        while (expression is UnaryExpression unaryExpression
               && (unaryExpression.NodeType == ExpressionType.Convert
                   || unaryExpression.NodeType == ExpressionType.ConvertChecked
                   || unaryExpression.NodeType == ExpressionType.TypeAs))
        {
            expression = unaryExpression.Operand;
            if (unaryExpression.Type != typeof(object) // Ignore object conversion
                && !unaryExpression.Type.IsAssignableFrom(expression.Type)) // Ignore casting to base type/interface
            {
                convertedType = unaryExpression.Type;
            }
        }

        return expression;
    }
}
