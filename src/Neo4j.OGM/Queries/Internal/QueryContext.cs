using Neo4j.Driver;

namespace Neo4j.OGM.Queries.Internal;

public class QueryContext : IParameterValues
{
    private readonly IDictionary<string, object?> _parameterValues = new Dictionary<string, object?>();
    public IReadOnlyDictionary<string, object?> ParameterValues => (IReadOnlyDictionary<string, object?>)_parameterValues;

    public void AddParameter(string name, object? value) => _parameterValues.Add(name, value);

    public IResultCursor QueryIResultCursor { get; set; }

    public QueryContext(ISession context)
    {
        Context = context;
    }

    public virtual ISession Context { get; }
}
