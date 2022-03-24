using System.Linq.Expressions;

namespace Neo4j.OGM.Query;

public class ReplacingExpressionVisitor : ExpressionVisitor
{
    private IReadOnlyList<Expression> _originals;
    private IReadOnlyList<Expression> _replacements;

    public ReplacingExpressionVisitor(Expression[] originals, Expression[] replacements)
    {
        _originals = originals;
        _replacements = replacements;
    }

    internal static object Replace(Expression original, Expression replacement, Expression tree)
        => new ReplacingExpressionVisitor(new[] { original }, new[] { replacement }).Visit(tree);

    public override Expression? Visit(Expression? expression)
    {
        if (expression == null)
        {
            return null;
        }

        for (var i = 0; i < _originals.Count; i++)
        {
            if (expression.Equals(_originals[i]))
            {
                return _replacements[i];
            }
        }

        return base.Visit(expression);
    }
}
