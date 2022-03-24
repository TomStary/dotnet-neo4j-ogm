namespace Neo4j.OGM.Context;

internal class MappedRelationship
{
    private readonly long _startNodeId;
    private readonly long _sourceId;
    private readonly string _type;
    private readonly long? _endNodeId;
    private readonly Type _sourceType;
    private readonly Type _targetType;
    private readonly long _targetId;
    private readonly long? _relationshipId;

    public MappedRelationship(long sourceId, string type, long targetId, long? relationshipId, Type sourceType, Type targetType)
    {
        _sourceId = sourceId;
        _type = type;
        _targetId = targetId;
        _relationshipId = relationshipId;
        _sourceType = sourceType;
        _targetType = targetType;
    }

    internal long? GetRelationshipId()
    {
        return _relationshipId;
    }

    internal long GetStartNodeId()
    {
        return _startNodeId;
    }

    internal long? GetEndNodeId()
    {
        return _endNodeId;
    }
}
