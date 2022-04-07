using System.Reflection;
using Neo4j.OGM.Internals.Extensions;
using Neo4j.OGM.Metadata;
using Neo4j.OGM.Metadata.Schemas;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests.Metadata.Schemas;

public class SchemaBuilderTest
{
    [Fact]
    public void BuildTestOkSimpleObject()
    {
        // preparation
        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes())
            .Returns(new[] { typeof(SimplePerson) });
        var domainInfo = new DomainInfo(assembly.Object);

        // create builder
        var builder = new SchemaBuilder(domainInfo);

        var schema = builder.Build();

        Assert.IsAssignableFrom<ISchema>(schema);
        Assert.NotNull(schema.FindNode(typeof(SimplePerson).GetNeo4jName()));
    }

    [Fact]
    public void BuildTestOkObjectWithRelationship()
    {
        // preparation
        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes())
            .Returns(new[] { typeof(Person), typeof(Post) });
        var domainInfo = new DomainInfo(assembly.Object);

        // create builder
        var builder = new SchemaBuilder(domainInfo);

        var schema = builder.Build();
        var personNode = schema.FindNode(typeof(Person).GetNeo4jName());

        Assert.IsAssignableFrom<ISchema>(schema);
        Assert.NotNull(personNode);
        Assert.NotNull(schema.FindNode(typeof(Post).GetNeo4jName()));
        Assert.NotNull(personNode.Relationships);
        Assert.NotEmpty(personNode.Relationships);
    }


    [Fact]
    public void BuildTestOkObjectWithRelationshipEntity()
    {
        // preparation
        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes())
            .Returns(new[] { typeof(Author), typeof(AuthorsRelationship), typeof(Note) });
        var domainInfo = new DomainInfo(assembly.Object);

        // create builder
        var builder = new SchemaBuilder(domainInfo);

        var schema = builder.Build();

        Assert.IsAssignableFrom<ISchema>(schema);
        Assert.NotNull(schema.FindNode(typeof(Author).GetNeo4jName()));
        Assert.NotNull(schema.FindNode(typeof(Note).GetNeo4jName()));
        Assert.NotNull(schema.FindRelationship("AUTHORS"));
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
