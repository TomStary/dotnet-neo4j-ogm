using System.Threading.Tasks;
using Neo4j.Driver;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests;

public class SessionTest
{
    [Fact]
    public async Task AddShallowNodeAsync()
    {
        // TODO: Create fixture as this is singleton
        var sessionFactory = new SessionFactory("neo4j+s://4b101c70.databases.neo4j.io", AuthTokens.Basic("neo4j", "password"), typeof(Person).Assembly);

        var session = sessionFactory.CreateSession();

        Assert.NotNull(session);

        var model = new Person()
        {
            Name = "John Doe"
        };

        await session.SaveAsync(model);
    }
}
