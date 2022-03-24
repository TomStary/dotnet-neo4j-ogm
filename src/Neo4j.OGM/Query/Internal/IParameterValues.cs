namespace Neo4j.OGM.Query.Internal;

public interface IParameterValues
{
    IReadOnlyDictionary<string, object?> ParameterValues { get; }

    void AddParameter(string name, object? value);
}
