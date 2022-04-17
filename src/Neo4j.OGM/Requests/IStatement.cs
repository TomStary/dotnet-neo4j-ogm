namespace Neo4j.OGM.Requests;

public interface IStatement
{
    string Statement { get; }
    Dictionary<string, object> Parameters { get; }
}
