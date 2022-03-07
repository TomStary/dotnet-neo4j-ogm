using static Neo4j.OGM.Annotations.RelationshipAttribute;

namespace Neo4j.OGM.Metadata.Schema;

public interface IRelationship
{
    string Type { get; }

    DirectionEnum Direction { get; }

    INode Start();

    INode Other(INode node);
}
