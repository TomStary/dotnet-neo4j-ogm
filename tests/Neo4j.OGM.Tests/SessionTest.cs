using Moq;
using Neo4j.Driver;
using Neo4j.OGM.Context;
using Neo4j.OGM.Cypher.Compilers;
using Neo4j.OGM.Internals;
using Neo4j.OGM.Metadata;
using Neo4j.OGM.Requests;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests;

public class SessionTest
{
    [Fact]
    public async Task AddShallowNodeAsync()
    {
        var metadataMock = new Mock<MetaData>();
        var driverMock = new Mock<IDriver>();
        var mappingContextMock = new Mock<MappingContext>(metadataMock.Object);
        var entityGraphMapperMock = new Mock<IEntityMapper>();

        var compilerContextMock = new Mock<ICompilerContext>();
        var multistatementCompilerMock = new Mock<IMultiStatementCypherCompiler>();

        var asyncSessionMock = new Mock<IAsyncSession>();
        var asyncTransactionMock = new Mock<IAsyncTransaction>();

        var iStatementList = new List<IStatement>
        {
            new RowDataStatement("CREATE (n:Person {name: 'John Doe'}) RETURN n", new Dictionary<string, object?>())
        };

        entityGraphMapperMock.Setup(x => x.CompilerContext()).Returns(compilerContextMock.Object);
        compilerContextMock.Setup(x => x.Compiler).Returns(multistatementCompilerMock.Object);
        multistatementCompilerMock.Setup(x => x.HasStatementDependentOnNewNode()).Returns(true);
        multistatementCompilerMock.Setup(x => x.CreateNodesStatements()).Returns(iStatementList);
        driverMock.Setup(x => x.AsyncSession()).Returns(asyncSessionMock.Object);
        asyncSessionMock.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(asyncTransactionMock.Object);

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        Assert.NotNull(session);

        var model = new Person()
        {
            Name = "John Doe"
        };

        await session.SaveAsync(model);

        entityGraphMapperMock.Verify(x => x.Map(It.IsAny<Person>(), It.IsAny<int>()), Times.Once);
        compilerContextMock.Verify(x => x.Compiler, Times.Once);
        multistatementCompilerMock.Verify(x => x.UseStatementFactory(It.IsAny<RowStatementFactory>()), Times.Once);
        asyncTransactionMock.Verify(x => x.RunAsync(It.Is<Query>(x => x.Text == iStatementList.First().Statement)), Times.Once);
    }

    [Fact]
    public void SetTestCreateDbSet()
    {
        var metadataMock = new Mock<MetaData>();
        var driverMock = new Mock<IDriver>();
        var entityGraphMapperMock = new Mock<IEntityMapper>();

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        var dbSet = session.Set<SimplePerson>();

        Assert.NotNull(dbSet);
        Assert.IsAssignableFrom<DbSet<SimplePerson>>(dbSet);
    }

    [Fact]
    public void GetDatabaseSessionTest()
    {
        var metadataMock = new Mock<MetaData>();
        var driverMock = new Mock<IDriver>();
        var entityGraphMapperMock = new Mock<IEntityMapper>();
        var asyncSessionMock = new Mock<IAsyncSession>();

        driverMock.Setup(x => x.AsyncSession()).Returns(asyncSessionMock.Object);

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        var databaseSession = session.GetDatabaseSession();

        Assert.NotNull(databaseSession);
        Assert.IsAssignableFrom<IAsyncSession>(databaseSession);
    }

    [Fact]
    public async Task DisposeTest()
    {
        var metadataMock = new Mock<MetaData>();
        var driverMock = new Mock<IDriver>();
        var entityGraphMapperMock = new Mock<IEntityMapper>();

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        session.Dispose();

        Assert.Throws<ObjectDisposedException>(() => session.GetDatabaseSession());
        Assert.Throws<ObjectDisposedException>(() => session.Metadata);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => session.SaveAsync<object>(new object { }));
        Assert.Throws<ObjectDisposedException>(() => session.Set<SimplePerson>());
    }
}
