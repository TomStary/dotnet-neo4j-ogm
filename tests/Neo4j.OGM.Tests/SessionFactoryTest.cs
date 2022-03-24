using Neo4j.Driver;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests;

public class SessionFactoryTest
{
    [Fact]
    public void CreateSessionTest()
    {
        var sessionFactory = new SessionFactory("bolt://localhost:7687", AuthTokens.Basic("neo4j", "rootroot"), typeof(Person).Assembly);

        var session = sessionFactory.CreateSession();

        Assert.NotNull(session);
    }
}
