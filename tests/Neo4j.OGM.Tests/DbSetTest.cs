using Neo4j.Driver;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests;

public class DbSetTest
{
    [Fact]
    public async Task FindAsync_ObjectFound()
    {
        var sessionFactory = new SessionFactory("neo4j+s://4b101c70.databases.neo4j.io", AuthTokens.Basic("neo4j", ""), typeof(Person).Assembly);

        var session = sessionFactory.CreateSession();

        Assert.NotNull(session);

        var model = new Person()
        {
            Id = 1,
            Name = "John Doe"
        };

        var driver = GraphDatabase.Driver("neo4j+s://4b101c70.databases.neo4j.io", AuthTokens.Basic("neo4j", ""));

        using var sess = driver.AsyncSession();
        var cursor = await sess.RunAsync("MATCH (n) RETURN n LIMIT 1");
        var res = await cursor.FetchAsync();
        var item = cursor.Current;

        // await session.SaveAsync(model);

        var person = await session.Set<Person>().FindAsync(1);

        Assert.NotNull(person);
    }
}
