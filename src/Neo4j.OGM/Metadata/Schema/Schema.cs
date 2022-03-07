namespace Neo4j.OGM.Metadata.Schema;

public class SchemaObj : ISchema
{
    private readonly Dictionary<string, INode> Nodes = new();

    private readonly Dictionary<string, IRelationship> Relationships = new();


    public SchemaObj()
    { }

    public INode FindNode(string label)
    {
        throw new NotImplementedException();
    }

    public IRelationship FindRelationship(string type)
    {
        throw new NotImplementedException();
    }

    internal void AddNode(string label, INode node)
    {
        Nodes.Add(label, node);
    }

    internal void AddRelationship(string type, IRelationship relationship)
    {
        Relationships.Add(type, relationship);
    }

    internal bool HasRelationship(string relationshipType)
    {
        return Relationships.ContainsKey(relationshipType);
    }
}