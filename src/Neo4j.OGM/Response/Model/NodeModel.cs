namespace Neo4j.OGM.Response.Model;

public class NodeModel
{
    private readonly long _id;
    private IEnumerable<string> _labels = null!;
    private List<Tuple<string, object?>> _properties = new();

    public string Label => string.Join(",", _labels);

    public IEnumerable<string> Labels => _labels;

    public NodeModel(long id)
    {
        _id = id;
    }

    internal void SetLabels(IEnumerable<string> labels)
    {
        _labels = labels.OrderBy(x => x);
    }

    private object Id => _id;

    internal IDictionary<string, object> ToRow(string nodeIdTarget)
    {
        var rowMap = new Dictionary<string, object>
        {
            { nodeIdTarget, Id },
        };
        var props = new Dictionary<string, object?>
        {
            { "Label", Label }
        };
        foreach (var property in _properties)
        {
            props.Add(property.Item1, property.Item2);
        }

        rowMap.Add("props", props);
        return rowMap;
    }

    internal void SetProperties(List<Tuple<string, object?>> properties)
    {
        _properties = properties;
    }
}
