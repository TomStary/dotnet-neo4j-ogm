using Neo4j.OGM.Annotations;

namespace Neo4j.OGM.Metadata.Schemas;

public class Relationship : IRelationship
{
    public string Type { get; init; }
    public RelationshipAttribute.DirectionEnum Direction { get; init; }
    public INode StartNode { get; }
    public INode EndNode { get; }

    public Relationship(string type, RelationshipAttribute.DirectionEnum direction, INode start, INode end)
    {
        Type = type;
        Direction = direction;
        StartNode = start;
        EndNode = end;
    }
}
