using Neo4j.OGM.Exceptions;
using Neo4j.OGM.Response.Model;

namespace Neo4j.OGM.Cypher.Compilers;

public class NodeBuilder
{
    private readonly NodeModel _node;
    private readonly List<Tuple<string, object?>> _properties = new();

    public NodeBuilder(long id)
    {
        _node = new NodeModel(id);
    }

    public NodeModel Node => _node;

    public NodeBuilder AddLabels(IEnumerable<string> labels)
    {
        _node.SetLabels(labels);
        return this;
    }

    public void AddProperty(string name, object? value)
    {
        if (_properties.Any(property => property.Item1 == name))
        {
            throw new MappingException($"Property: {name} already mapped.");
        }

        _properties.Add(new Tuple<string, object?>(name, value));
    }

    internal void SetProperties()
    {
        _node.SetProperties(_properties);
    }
}
