using System.Linq.Expressions;
using Neo4j.OGM.Queries.CypherExpressions;

namespace Neo4j.OGM.Queries;

internal class CypherParameterExpression : CypherExpression
{
    private ParameterExpression _parameterExpression;
    private string? _name;

    public string Name
            => _name;

    public CypherParameterExpression(ParameterExpression parameterExpression)
           : base(parameterExpression.Type)
    {
        _parameterExpression = parameterExpression;
        _name = parameterExpression.Name;
    }
}
