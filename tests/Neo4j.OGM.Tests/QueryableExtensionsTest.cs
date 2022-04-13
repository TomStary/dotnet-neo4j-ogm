using Moq;
using Neo4j.Driver;
using Neo4j.OGM.Context;
using Neo4j.OGM.Internals;
using Neo4j.OGM.Metadata;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests;

public class QueryableExtensionsTest
{
    [Fact]
    public async Task FirstOrDefaultWithPredicateAsyncTest()
    {
        var metadataMock = new Mock<MetaData>();
        var driverMock = new Mock<IDriver>();
        var entityGraphMapperMock = new Mock<IEntityMapper>();

        var asyncSessionMock = new Mock<IAsyncSession>();
        var asyncTransactionMock = new Mock<IAsyncTransaction>();
        var resultCursorMock = new Mock<IResultCursor>();

        driverMock.Setup(x => x.AsyncSession()).Returns(asyncSessionMock.Object);
        asyncSessionMock.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(asyncTransactionMock.Object);
        asyncTransactionMock.Setup(x => x.RunAsync(It.IsAny<Query>())).ReturnsAsync(resultCursorMock.Object);
        resultCursorMock.SetupSequence(x => x.FetchAsync()).ReturnsAsync(true).ReturnsAsync(false);

        var personEntity = new PersonEntity(1, new Dictionary<string, object> { { "name", "Test" } });
        var presonRecord = new PersonRecord(new Dictionary<string, object> { { "c", personEntity } });
        resultCursorMock.Setup(x => x.Current).Returns(presonRecord);

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        var dbSet = session.Set<SimplePerson>();

        var result = await dbSet.FirstOrDefaultAsync(x => x.Name == "Test");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task FirstOrDefaultWithPredicateAsyncInvalidQueryableTest()
    {
        var persons = new List<SimplePerson>
        {
            new SimplePerson { Name = "John Doe"},
            new SimplePerson { Name = "Jane Doe"},
        }.AsQueryable();

        await Assert.ThrowsAsync<InvalidOperationException>(() => persons.FirstOrDefaultAsync(x => x.Name == "John Doe"));
    }

    [Fact]
    public async Task FirstOrDefaultWithPredicateAsyncNullPredicateTest()
    {
        var metadataMock = new Mock<MetaData>();
        var driverMock = new Mock<IDriver>();
        var entityGraphMapperMock = new Mock<IEntityMapper>();

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        var dbSet = session.Set<SimplePerson>();

        await Assert.ThrowsAsync<ArgumentNullException>(() => dbSet.FirstOrDefaultAsync(null));
    }

    private class PersonRecord : IRecord
    {
        public object this[int index] => throw new System.NotImplementedException();

        public object this[string key] => Values[key];

        public IReadOnlyDictionary<string, object> Values { get; }

        public IReadOnlyList<string> Keys { get; }

        public PersonRecord(IReadOnlyDictionary<string, object> values)
        {
            Values = values;
            Keys = values.Keys.ToList();
        }
    }

    private class PersonEntity : IEntity
    {
        public object this[string key] => Properties[key];

        public IReadOnlyDictionary<string, object> Properties { get; }

        public long Id { get; }

        public PersonEntity(long key, IReadOnlyDictionary<string, object> properties)
        {
            Id = key;
            Properties = properties;
        }
    }
}
