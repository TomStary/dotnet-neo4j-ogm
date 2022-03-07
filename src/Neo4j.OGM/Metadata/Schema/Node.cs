namespace Neo4j.OGM.Metadata.Schema;

public class Node : INode
{
    public string? Label { get; init; }

    public Dictionary<string, IRelationship> Relationships => throw new NotImplementedException();

    public Node(string? label)
    {
        Label = label;
    }

    public void AddRelationship(string type, IRelationship relationship)
    {
        Relationships.Add(type, relationship);
    }
}