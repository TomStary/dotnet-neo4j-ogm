using Neo4j.OGM.Queries.CypherExpressions;

namespace Neo4j.OGM.Queries;

public class CypherFunctionExpression : CypherExpression
{
    public string Function { get; }
    public CypherExpression Selector { get; }

    private readonly Type _type;
    public CypherExpression Argument { get; }

    public CypherFunctionExpression(string function, Type type, CypherExpression selector, CypherExpression argument)
        : base(type)
    {
        Function = function;
        _type = type;
        Selector = selector;
        Argument = argument;
    }
}
