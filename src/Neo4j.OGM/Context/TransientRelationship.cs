using System.Diagnostics.CodeAnalysis;

namespace Neo4j.OGM.Context;

[ExcludeFromCodeCoverage(Justification = "This is not a part of POC.")]
public class TransientRelationship
{
    public TransientRelationship(long idSource, long reference, string type, long idTarget, Type sourceType, Type targetType)
    {
        IdSource = idSource;
        Reference = reference;
        Type = type;
        IdTarget = idTarget;
        TargetType = targetType;
        SourceType = sourceType;
    }

    public long IdSource { get; }
    public long Reference { get; }
    public string Type { get; }
    public long IdTarget { get; }
    public Type TargetType { get; }
    public Type SourceType { get; }
}
