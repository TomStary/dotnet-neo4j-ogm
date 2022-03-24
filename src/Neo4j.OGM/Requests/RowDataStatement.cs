namespace Neo4j.OGM.Requests;

public class RowDataStatement : IStatement
{
    private readonly string _statement;
    private readonly Dictionary<string, object?> _parameters = new();

    public string Statement => _statement;

    public Dictionary<string, object?> Parameters => _parameters;

    public RowDataStatement(string statement, Dictionary<string, object?> parameters)
    {
        _statement = statement;
        _parameters = parameters;
    }
}

