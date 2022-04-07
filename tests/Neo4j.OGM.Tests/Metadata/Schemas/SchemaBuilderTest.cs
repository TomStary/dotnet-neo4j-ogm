using System.Reflection;
using Neo4j.OGM.Metadata;
using Neo4j.OGM.Metadata.Schemas;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests.Metadata.Schemas;

public class SchemaBuilderTest
{
    [Fact]
    public void BuildTestOk()
    {
        // preparation
        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes())
            .Returns(new[] { typeof(Person), typeof(Post) });
        var domainInfo = new DomainInfo(assembly.Object);

        // create builder
        var builder = new SchemaBuilder(domainInfo);

        var schema = builder.Build();

        Assert.IsAssignableFrom<ISchema>(schema);
    }

    [Fact]
    public void BuildTestMissingRelationship()
    {
        // preparation
        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes())
            .Returns(new[] { typeof(Person) });
        var domainInfo = new DomainInfo(assembly.Object);

        // create builder
        var builder = new SchemaBuilder(domainInfo);

        // Could not find node with label
        Assert.Throws<ArgumentException>(() => builder.Build());
    }
}
