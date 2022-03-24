using System.Linq.Expressions;
using System.Reflection;
using Neo4j.OGM.Query.CypherExpressions;

namespace Neo4j.OGM.Query;

public class CypherTranslatingExpressionVisitor : ExpressionVisitor
{
    private static readonly List<MethodInfo> SingleResultMethodInfos = new()
    {
        QueryableMethods.FirstOrDefaultWithPredicate,
    };

    internal CypherExpression? Translate(Expression expression)
    {
        var result = Visit(expression);

        if (result is CypherExpression translation)
        {
            translation = ApplyDefaultMapping(translation);

            if (translation?.TypeMapping == null)
            {
                return null;
            }

            return translation;
        }

        return null;
    }

    private CypherExpression? ApplyDefaultMapping(CypherExpression? cypherExpression)
        => cypherExpression == null
            || cypherExpression.TypeMapping != null
            ? cypherExpression
            : ApplyTypeMapping(cypherExpression);

    private CypherExpression? ApplyTypeMapping(CypherExpression cypherExpression)
    {
        return cypherExpression;
    }
}
