using Moq;
using Neo4j.Driver;
using Neo4j.OGM.Context;
using Neo4j.OGM.Internals;
using Neo4j.OGM.Metadata;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests;

public partial class DbSetTest
{
    [Fact]
    public async Task FindAsync_ObjectFound()
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

        Assert.NotNull(dbSet);
        Assert.IsAssignableFrom<DbSet<SimplePerson>>(dbSet);

        var person = await dbSet.FindAsync((long)1);

        Assert.NotNull(person);
    }

    [Fact]
    public async Task FindAsync_NullOnNotFound()
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
        resultCursorMock.SetupSequence(x => x.FetchAsync()).ReturnsAsync(false);

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        var dbSet = session.Set<SimplePerson>();

        Assert.NotNull(dbSet);
        Assert.IsAssignableFrom<DbSet<SimplePerson>>(dbSet);

        var person = await dbSet.FindAsync((long)-1);

        Assert.Null(person);
    }

    [Fact]
    public async Task FindAsync_NullKey_NullResult()
    {
        var metadataMock = new Mock<MetaData>();
        var driverMock = new Mock<IDriver>();
        var entityGraphMapperMock = new Mock<IEntityMapper>();

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        Assert.NotNull(session);

        var person = await session.Set<SimplePerson>().FindAsync(null);

        Assert.Null(person);
    }

    [Fact]
    public async Task FindAsync_IncorectKeyCount()
    {
        var metadataMock = new Mock<MetaData>();
        var driverMock = new Mock<IDriver>();
        var entityGraphMapperMock = new Mock<IEntityMapper>();

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        Assert.NotNull(session);

        await Assert.ThrowsAsync<ArgumentException>(() => session.Set<SimplePerson>().FindAsync((long)1, (long)2, (long)3));
    }
}
