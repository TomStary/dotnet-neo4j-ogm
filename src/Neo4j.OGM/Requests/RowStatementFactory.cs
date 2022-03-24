namespace Neo4j.OGM.Requests;

public class RowStatementFactory : IStatementFactory
{
    public IStatement CreateStatement(string statement, Dictionary<string, object?> parameters)
    {
        return new RowDataStatement(statement, parameters);
    }
}
