namespace Neo4j.OGM.Queries.Internal;

public interface IParameterValues
{
    IReadOnlyDictionary<string, object?> ParameterValues { get; }

    void AddParameter(string name, object? value);
}
