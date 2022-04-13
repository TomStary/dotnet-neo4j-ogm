using Neo4j.OGM.Cypher.Compilers;
using Neo4j.OGM.Exceptions;
using Neo4j.OGM.Response.Model;

namespace Neo4j.OGM.Tests.Cypher.Compilers;

public class NodeBuilderTest
{
    [Fact]
    public void PublicPropertiesTests()
    {
        var node = new NodeBuilder(1);

        Assert.Equal(1, node.Id);
        Assert.NotNull(node.Node);
        Assert.IsAssignableFrom<NodeModel>(node.Node);
    }

    [Fact]
    public void AddLabelsTest()
    {
        var node = new NodeBuilder(1);
        node.AddLabels(new[] { "label1", "label2" });

        Assert.Equal(2, node.Node.Labels.Count());
        Assert.Contains("label1", node.Node.Labels);
        Assert.Contains("label2", node.Node.Labels);

        node.AddLabels(Array.Empty<string>());

        Assert.Empty(node.Node.Labels);
    }

    [Fact]
    public void AddPropertyTest()
    {
        var node = new NodeBuilder(1);

        node.AddProperty("name", "value");

        // Check if calling method two times throws correct exception
        Assert.Throws<MappingException>(() => node.AddProperty("name", "value"));
    }
}
