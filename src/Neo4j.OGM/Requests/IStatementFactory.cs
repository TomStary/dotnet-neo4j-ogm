namespace Neo4j.OGM.Requests;

public interface IStatementFactory
{
    IStatement CreateStatement(string statement, Dictionary<string, object> parameters);
}
