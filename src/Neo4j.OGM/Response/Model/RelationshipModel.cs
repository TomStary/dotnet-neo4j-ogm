namespace Neo4j.OGM.Response.Model;

public class RelationshipModel
{
    public RelationshipModel(
        long id,
        string type,
        long? startNodeId,
        long? endNodeId,
        Dictionary<string, object?> properties)
    {
        Id = id;
        Type = type;
        StartNodeId = startNodeId;
        EndNodeId = endNodeId;
        Properties = properties;
    }

    public long Id { get; }
    public string Type { get; }
    public long? StartNodeId { get; }
    public long? EndNodeId { get; }
    public Dictionary<string, object?> Properties { get; }
}
