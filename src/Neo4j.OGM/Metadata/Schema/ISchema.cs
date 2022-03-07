namespace Neo4j.OGM.Metadata.Schema;

public interface ISchema
{
    INode FindNode(string label);

    IRelationship FindRelationship(string type);
}
