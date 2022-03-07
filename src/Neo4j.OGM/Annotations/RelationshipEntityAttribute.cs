namespace Neo4j.OGM.Annotations;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class RelationshipEntityAttribute : Attribute
{
    public string Type { get; }

    public RelationshipEntityAttribute(string type)
    {
        Type = type;
    }
}