using Neo4j.Driver;

namespace Neo4j.OGM.Tests.TestModels;

public class PersonEntity : IEntity
{
    public object this[string key] => Properties[key];

    public IReadOnlyDictionary<string, object> Properties { get; }

    public long Id { get; }

    public PersonEntity(long key, IReadOnlyDictionary<string, object> properties)
    {
        Id = key;
        Properties = properties;
    }
}
