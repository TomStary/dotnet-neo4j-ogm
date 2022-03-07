using Neo4j.OGM.Annotations;

namespace Neo4j.OGM.Metadata.Schema;

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

    public INode Other(INode node)
    {
        throw new NotImplementedException();
    }

    public INode Start()
    {
        throw new NotImplementedException();
    }
}