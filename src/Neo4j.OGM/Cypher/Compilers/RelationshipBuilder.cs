using System.Diagnostics.CodeAnalysis;
using Neo4j.OGM.Annotations;
using Neo4j.OGM.Exceptions;

namespace Neo4j.OGM.Cypher.Compilers;

public class RelationshipBuilder
{
    private readonly bool _mapBothDirections;
    private long _relId;
    private RelationshipAttribute.DirectionEnum _direction;
    private long? _endNodeId;
    private long? _startNodeId;

    [ExcludeFromCodeCoverage]
    public bool Singleton { get; internal set; } = true;

    [ExcludeFromCodeCoverage]
    public long Reference { get; internal set; }

    [ExcludeFromCodeCoverage]
    public bool RelationshipEntity { get; internal set; } = false;

    public string Type { get; }

    private readonly List<Tuple<string, object?>> _properties = new();

    public RelationshipBuilder(string type, bool mapBothDirections = false)
    {
        Type = type;
        _mapBothDirections = mapBothDirections;
    }

    public RelationshipBuilder(string type, long relId)
    {
        Type = type;
        _relId = relId;
    }

    internal void Direction(RelationshipAttribute.DirectionEnum direction)
    {
        _direction = direction;
    }

    internal bool HasDirection(RelationshipAttribute.DirectionEnum direction)
    {
        return _direction == direction;
    }

    internal long? StartNode()
    {
        return _startNodeId;
    }

    internal void SetStartNode(long startNodeId)
    {
        _startNodeId = startNodeId;
    }

    internal long? EndNode()
    {
        return _endNodeId;
    }

    internal void SetEndNode(long endNodeId)
    {
        _endNodeId = endNodeId;
    }

    public void AddProperty(string name, object? value)
    {
        if (_properties.Any(property => property.Item1 == name))
        {
            throw new MappingException($"Property: {name} already mapped.");
        }

        _properties.Add(new Tuple<string, object?>(name, value));
    }
}
