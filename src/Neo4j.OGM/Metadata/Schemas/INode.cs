namespace Neo4j.OGM.Metadata.Schemas;

public interface INode
{
    string? Label { get; }

    Dictionary<string, IRelationship> Relationships { get; }

    void AddRelationship(string type, IRelationship relationship);
}
