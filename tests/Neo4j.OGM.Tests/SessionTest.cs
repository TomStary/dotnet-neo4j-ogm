using System.Reflection;
using Neo4j.Driver;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests;

public class SessionTest
{
    [Fact]
    public async Task AddShallowNodeAsync()
    {
        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(Person), typeof(Post) });

        var sessionFactory = new SessionFactory("bolt://localhost:11007", AuthTokens.Basic("neo4j", "rootroot"), assembly.Object);

        var session = sessionFactory.Create();

        Assert.NotNull(session);

        var model = new Person()
        {
            Name = "John Doe"
        };

        await session.SaveAsync(model);
    }
}
