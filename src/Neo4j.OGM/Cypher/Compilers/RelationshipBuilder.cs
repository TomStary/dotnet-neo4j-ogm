using Neo4j.OGM.Annotations;
using Neo4j.OGM.Exceptions;

namespace Neo4j.OGM.Cypher.Compilers;

public class RelationshipBuilder
{
    private string _type;
    private readonly bool _mapBothDirections;
    private long _relId;
    private RelationshipAttribute.DirectionEnum _direction;
    private bool _singleton = true;
    private long _targetRef;
    private bool _relationshipEntity = false;
    private long? _endNodeId;
    private long? _startNodeId;

    public bool Singleton { get => _singleton; internal set => _singleton = value; }
    public long Reference { get => _targetRef; internal set => _targetRef = value; }
    public bool RelationshipEntity { get => _relationshipEntity; internal set => _relationshipEntity = value; }

    public string Type => _type;

    private readonly List<Tuple<string, object?>> _properties = new();

    public RelationshipBuilder(string type, bool mapBothDirections = false)
    {
        _type = type;
        _mapBothDirections = mapBothDirections;
    }

    public RelationshipBuilder(string type, long relId)
    {
        _type = type;
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

    internal long? EndNode()
    {
        return _endNodeId;
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
