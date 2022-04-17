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

    [Fact]
    public async Task ToListAsyncWithoutPredicateTest()
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
        resultCursorMock.SetupSequence(x => x.FetchAsync())
                        .ReturnsAsync(true)
                        .ReturnsAsync(true)
                        .ReturnsAsync(true)
                        .ReturnsAsync(false);

        var personEntity = new PersonEntity(1, new Dictionary<string, object> { { "name", "Test" } });
        var personEntity2 = new PersonEntity(2, new Dictionary<string, object> { { "name", "Test1" } });
        var personEntity3 = new PersonEntity(3, new Dictionary<string, object> { { "name", "Test2" } });
        var presonRecord = new PersonRecord(new Dictionary<string, object> { { "c", personEntity } });
        var presonRecord2 = new PersonRecord(new Dictionary<string, object> { { "c", personEntity } });
        var presonRecord3 = new PersonRecord(new Dictionary<string, object> { { "c", personEntity } });
        resultCursorMock.SetupSequence(x => x.Current)
                        .Returns(presonRecord)
                        .Returns(presonRecord2)
                        .Returns(presonRecord3);

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        var dbSet = session.Set<SimplePerson>();

        var result = await dbSet.ToListAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task ToListAsyncEmptyTest()
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
        resultCursorMock.SetupSequence(x => x.FetchAsync())
                        .ReturnsAsync(false);


        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        var dbSet = session.Set<SimplePerson>();

        var result = await dbSet.ToListAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task WhereTest()
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
        resultCursorMock.SetupSequence(x => x.FetchAsync())
                        .ReturnsAsync(true)
                        .ReturnsAsync(false);

        var personEntity = new PersonEntity(1, new Dictionary<string, object> { { "name", "Test" } });
        var presonRecord = new PersonRecord(new Dictionary<string, object> { { "c", personEntity } });

        resultCursorMock.SetupSequence(x => x.Current)
                        .Returns(presonRecord);

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        var dbSet = session.Set<SimplePerson>();

        var result = await dbSet.Where(x => x.Name == "Test").ToListAsync();

        Assert.NotNull(result);
        Assert.Single(result);
    }
}
