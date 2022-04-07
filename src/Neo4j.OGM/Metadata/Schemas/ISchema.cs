namespace Neo4j.OGM.Metadata.Schemas;

public interface ISchema
{
    void AddNode(string label, INode node);
    void AddRelationship(string type, IRelationship relationship);
    INode FindNode(string label);
    IRelationship FindRelationship(string type);
}
