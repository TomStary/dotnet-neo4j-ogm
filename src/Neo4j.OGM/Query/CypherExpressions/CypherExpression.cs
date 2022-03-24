using System.Linq.Expressions;

namespace Neo4j.OGM.Query.CypherExpressions;

public class CypherExpression : Expression
{
    public object TypeMapping { get; internal set; }
}
