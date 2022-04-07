using System.Reflection;
using Neo4j.Driver;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests;

public class DbSetTest
{
    [Fact]
    public async Task FindAsync_ObjectFound()
    {
        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(SimplePerson) });

        var sessionFactory = new SessionFactory("bolt://localhost:11007", AuthTokens.Basic("neo4j", "rootroot"), assembly.Object);

        var session = sessionFactory.Create();

        Assert.NotNull(session);

        var model = new Person()
        {
            Id = 1,
            Name = "John Doe"
        };

        await session.SaveAsync(model);

        var person = await session.Set<SimplePerson>().FindAsync(1);

        Assert.NotNull(person);
    }

    [Fact]
    public async Task FindAsync_NullOnNotFound()
    {
        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(SimplePerson) });

        var sessionFactory = new SessionFactory("bolt://localhost:11007", AuthTokens.Basic("neo4j", "rootroot"), assembly.Object);

        var session = sessionFactory.Create();

        Assert.NotNull(session);

        var person = await session.Set<SimplePerson>().FindAsync(-1);

        Assert.Null(person);
    }

    [Fact]
    public async Task FindAsync_NullKey_NullResult()
    {
        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(SimplePerson) });

        var sessionFactory = new SessionFactory("bolt://localhost:11007", AuthTokens.Basic("neo4j", "rootroot"), assembly.Object);

        var session = sessionFactory.Create();

        Assert.NotNull(session);

        var person = await session.Set<SimplePerson>().FindAsync(null);

        Assert.Null(person);
    }

    [Fact]
    public async Task FindAsync_IncorectKeyCount()
    {
        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(SimplePerson) });

        var sessionFactory = new SessionFactory("bolt://localhost:11007", AuthTokens.Basic("neo4j", "rootroot"), assembly.Object);

        var session = sessionFactory.Create();

        Assert.NotNull(session);

        await Assert.ThrowsAsync<ArgumentException>(() => session.Set<SimplePerson>().FindAsync(1, 2, 3));
    }
}
