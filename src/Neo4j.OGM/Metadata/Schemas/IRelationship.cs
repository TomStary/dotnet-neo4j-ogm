using static Neo4j.OGM.Annotations.RelationshipAttribute;

namespace Neo4j.OGM.Metadata.Schemas;

public interface IRelationship
{
    string Type { get; }

    DirectionEnum Direction { get; }
}
