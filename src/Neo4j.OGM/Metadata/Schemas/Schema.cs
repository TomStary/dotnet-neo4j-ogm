namespace Neo4j.OGM.Metadata.Schemas;

internal class Schema : ISchema
{
    private readonly Dictionary<string, INode> Nodes = new();

    private readonly Dictionary<string, IRelationship> Relationships = new();


    public Schema()
    { }

    public INode FindNode(string label)
    {
        return Nodes.TryGetValue(label, out var node) ? node : throw new ArgumentException($"Could not find node with label: {label}.");
    }

    public IRelationship FindRelationship(string type)
    {
        return Relationships.TryGetValue(type, out var relationship) ? relationship : throw new ArgumentException($"Could not find relationship with type: {type}.");
    }

    public void AddNode(string label, INode node)
    {
        Nodes.Add(label, node);
    }

    public void AddRelationship(string type, IRelationship relationship)
    {
        Relationships.Add(type, relationship);
    }
}
