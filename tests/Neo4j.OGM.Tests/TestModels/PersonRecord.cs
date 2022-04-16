using Neo4j.Driver;

namespace Neo4j.OGM.Tests.TestModels;

public class PersonRecord : IRecord
{
    public object this[int index] => throw new NotImplementedException();

    public object this[string key] => Values[key];

    public IReadOnlyDictionary<string, object> Values { get; }

    public IReadOnlyList<string> Keys { get; }

    public PersonRecord(IReadOnlyDictionary<string, object> values)
    {
        Values = values;
        Keys = values.Keys.ToList();
    }
}
