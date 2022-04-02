namespace Neo4j.OGM.Requests;

internal class QueryStatement : IStatement
{
    public string Statement { get; }

    public Dictionary<string, object?> Parameters { get; }

    public QueryStatement(string statement, Dictionary<string, object?> parameters)
    {
        Statement = statement;
        Parameters = parameters;
    }
}
