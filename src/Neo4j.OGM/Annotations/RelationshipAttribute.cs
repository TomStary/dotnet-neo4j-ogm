namespace Neo4j.OGM.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class RelationshipAttribute : Attribute
{
    public string Type { get; }

    public DirectionEnum Direction { get; }

    public enum DirectionEnum
    {
        Outgoing,
        Incoming,
        Undirected,
    };

    public RelationshipAttribute(string type, DirectionEnum direction = DirectionEnum.Outgoing)
    {
        Type = type;
        Direction = direction;
    }
}