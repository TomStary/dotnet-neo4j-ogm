namespace Neo4j.OGM.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class NodeAttribute : Attribute
{
    public string Label { get; } = string.Empty;

    public NodeAttribute()
    { }

    public NodeAttribute(string label)
    {
        Label = label;
    }
}
