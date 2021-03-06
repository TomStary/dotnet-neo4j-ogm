using System.Reflection;
using Neo4j.OGM.Internals.Extensions;
using Neo4j.OGM.Queries.CypherExpressions;

namespace Neo4j.OGM.Queries.Internal;

internal class MethodCallTranslator
{
    private readonly CypherExpressionFactory _cypherExpressionFactory;

    private static readonly MethodInfo _containsMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Contains), typeof(string));

    internal MethodCallTranslator(CypherExpressionFactory cypherExpressionFactory)
    {
        _cypherExpressionFactory = cypherExpressionFactory;
    }

    internal CypherExpression Translate(CypherExpression instance, MethodInfo method, IReadOnlyList<CypherExpression> arguments)
    {
        CypherExpression? left = null;
        CypherExpression? right = null;

        if (method.Name == nameof(object.Equals)
                && instance != null
                && arguments.Count == 1)
        {
            left = instance;
            right = arguments[0];
        }
        else if (_containsMethodInfo.Equals(method))
        {
            return new CypherFunctionExpression(
                "CONTAINS",
                typeof(bool),
                instance!,
                arguments[0]);
        }
        else if (instance == null
            && method.Name == nameof(object.Equals)
            && arguments.Count == 2)
        {
            left = arguments[0];
            right = arguments[1];
        }

        return left != null
            && right != null
            ? (right.Type == typeof(object) && (right is CypherParameterExpression || right is CypherConstantExpression))
                || (left.Type == typeof(object) && (left is CypherParameterExpression || left is CypherConstantExpression))
                    ? _cypherExpressionFactory.Equal(left, right)
                    : _cypherExpressionFactory.Constant(false)
            : throw new InvalidOperationException("Method call translation failed");
    }
}
