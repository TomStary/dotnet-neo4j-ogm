using System.Reflection;
using Neo4j.Driver;
using Neo4j.OGM.Exceptions;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests;

public class SessionFactoryTest
{
    [Fact]
    public void CreateTestResultOk()
    {
        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(Person), typeof(Post) });

        var sessionFactory = new SessionFactory("bolt://localhost:11007", AuthTokens.Basic("neo4j", "rootroot"), assembly.Object);

        var session = sessionFactory.Create();

        Assert.NotNull(session);
        Assert.IsAssignableFrom<ISession>(session);
    }

    [Fact]
    public void CreateTestInvalidRelationshipEntity()
    {
        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(InvalidRelationshipEntity) });

        Assert.Throws<MissingStartNodeAttributeException>(
            () => new SessionFactory("bolt://localhost:11007", AuthTokens.Basic("neo4j", "rootroot"), assembly.Object));
    }

    [Fact]
    public void CreateTestMissingEndNodeAttribute()
    {
        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(MissingEndNodeAttribute), typeof(Person) });

        Assert.Throws<MissingEndNodeAttributeException>(
            () => new SessionFactory("bolt://localhost:11007", AuthTokens.Basic("neo4j", "rootroot"), assembly.Object));
    }
}
