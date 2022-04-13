using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests;

/// <summary>
/// This class will contain all tests with relation to the infrastructure of the library.
/// It will test things like depency injection extensions, and the configuration of the library.
/// </summary>
public class InfrastructureTest
{
    [Fact]
    public void TestSessionFactoryRegistration()
    {

        var assembly = new Moq.Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(SimplePerson) });
        var collection = new ServiceCollection();

        collection.AddNeo4jOGMFactory("bolt://localhost:11007", AuthTokens.Basic("neo4j", "rootroot"), assembly.Object);

        var provider = collection.BuildServiceProvider();
        var factory = provider.GetRequiredService<SessionFactory>();

        Assert.NotNull(factory);
    }
}
