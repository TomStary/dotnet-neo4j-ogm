namespace Neo4j.OGM.Requests;

public class DefaulterRequest
{
    private IEnumerable<IStatement> _statements = new List<IStatement>();

    public IEnumerable<IStatement> Statements => _statements;

    public DefaulterRequest(IEnumerable<IStatement> statements)
    {
        _statements = statements;
    }
}
