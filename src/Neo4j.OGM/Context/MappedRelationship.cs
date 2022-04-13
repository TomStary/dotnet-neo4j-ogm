using System.Diagnostics.CodeAnalysis;

namespace Neo4j.OGM.Context;

public class MappedRelationship
{
    private readonly long? _sourceId;
    private readonly string _type;
    private readonly Type _sourceType;
    private readonly Type _targetType;
    private readonly long? _targetId;

    public MappedRelationship(long? sourceId, string type, long? targetId, long? relationshipId, Type sourceType, Type targetType)
    {
        _sourceId = sourceId;
        _type = type;
        _targetId = targetId;
        RelationshipId = relationshipId;
        _sourceType = sourceType;
        _targetType = targetType;
    }

    [ExcludeFromCodeCoverage]
    internal long? RelationshipId { get; }

    [ExcludeFromCodeCoverage]
    internal long StartNodeId { get; }

    [ExcludeFromCodeCoverage]
    internal long? EndNodeId { get; }
}
