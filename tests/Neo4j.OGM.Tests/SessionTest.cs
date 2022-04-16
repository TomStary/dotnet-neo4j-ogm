using System.Runtime.CompilerServices;
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
        var entityGraphMapper = new EntityGraphMapper(mappingContextMock.Object);

        var asyncSessionMock = new Mock<IAsyncSession>();
        var asyncTransactionMock = new Mock<IAsyncTransaction>();

        var dateTime = DateTime.UtcNow;

        var iStatementList = new List<IStatement>
        {
            new RowDataStatement(
                "UNWIND $rows as row CREATE (n:`Person`) SET n=row.props RETURN row.nodeRef as ref, ID(n) as id, $type as type",
                new Dictionary<string, object?>
                {
                    {"type", "node"},
                    {"rows", new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"nodeRef", -1},
                            {"props", new Dictionary<string, object>
                            {
                                {"Label", "Person"},
                                {"Name", "John Doe"},
                                {"Posts", Array.Empty<Post>()},
                                {"CreatedAt", dateTime}
                            }}
                        }
                    }}
                })
        };

        driverMock.Setup(x => x.AsyncSession()).Returns(asyncSessionMock.Object);
        asyncSessionMock.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(asyncTransactionMock.Object);

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapper);

        Assert.NotNull(session);

        var model = new Person()
        {
            Name = "John Doe",
            CreatedAt = dateTime,
        };

        await session.SaveAsync(model);

        asyncTransactionMock.Verify(x => x.RunAsync(It.Is<Query>(x =>
            x.Text == iStatementList.First().Statement
            && x.Parameters.Contains(iStatementList.First().Parameters.First()))), Times.Once);
    }

    [Fact]
    public async Task UpdateShallowNodeAsync()
    {
        var metadataMock = new Mock<MetaData>();
        var driverMock = new Mock<IDriver>();
        var mappingContextMock = new Mock<MappingContext>(metadataMock.Object);
        var entityGraphMapper = new EntityGraphMapper(mappingContextMock.Object);

        var asyncSessionMock = new Mock<IAsyncSession>();
        var asyncTransactionMock = new Mock<IAsyncTransaction>();

        var dateTime = DateTime.UtcNow;

        var iStatementList = new List<IStatement>
        {
            new RowDataStatement(
                "UNWIND $rows as row MATCH (n) WHERE ID(n)=row.nodeId SET n:`Person` SET n += row.props RETURN row.nodeId as ref, ID(n) as id, $type as type",
                new Dictionary<string, object?>
                {
                    {"type", "node"},
                    {"rows", new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"nodeId", 1},
                            {"props", new Dictionary<string, object>
                            {
                                {"Label", "Person"},
                                {"Name", "John Doe"},
                                {"Posts", Array.Empty<Post>()},
                                {"CreatedAt", dateTime}
                            }}
                        }
                    }}
                })
        };

        driverMock.Setup(x => x.AsyncSession()).Returns(asyncSessionMock.Object);
        asyncSessionMock.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(asyncTransactionMock.Object);

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapper);

        Assert.NotNull(session);

        var model = new Person()
        {
            Id = 1,
            Name = "John Doe",
            CreatedAt = dateTime,
        };

        await session.SaveAsync(model);

        asyncTransactionMock.Verify(x => x.RunAsync(It.Is<Query>(x =>
            x.Text == iStatementList.First().Statement
            && x.Parameters.Contains(iStatementList.First().Parameters.First()))), Times.Once);
    }

    [Fact]
    public async Task AddListOfShallowNodesAsync()
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
            new RowDataStatement("CREATE (n:Person {name: 'John Doe'}) RETURN n", new Dictionary<string, object?>()),
            new RowDataStatement("CREATE (n:Person {name: 'Jane Doe'}) RETURN n", new Dictionary<string, object?>()),
        };

        entityGraphMapperMock.Setup(x => x.CompilerContext()).Returns(compilerContextMock.Object);
        compilerContextMock.Setup(x => x.Compiler).Returns(multistatementCompilerMock.Object);
        multistatementCompilerMock.Setup(x => x.HasStatementDependentOnNewNode()).Returns(true);
        multistatementCompilerMock.Setup(x => x.CreateNodesStatements()).Returns(iStatementList);
        driverMock.Setup(x => x.AsyncSession()).Returns(asyncSessionMock.Object);
        asyncSessionMock.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(asyncTransactionMock.Object);

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        Assert.NotNull(session);

        var model = new List<Person>
        {
            new Person()
            {
                Name = "John Doe"
            },
            new Person()
            {
                Name = "Jane Doe"
            }
        };

        await session.SaveAsync(model);

        entityGraphMapperMock.Verify(x => x.Map(It.IsAny<Person>(), It.IsAny<int>()), Times.Exactly(2));
        compilerContextMock.Verify(x => x.Compiler, Times.Once);
        multistatementCompilerMock.Verify(x => x.UseStatementFactory(It.IsAny<RowStatementFactory>()), Times.Once);
        asyncTransactionMock.Verify(x => x.RunAsync(It.Is<Query>(x => x.Text == iStatementList.First().Statement)), Times.Once);
    }

    [Fact]
    public async Task CreateRelationship()
    {
        var metadataMock = new Mock<MetaData>();
        var driverMock = new Mock<IDriver>();
        var mappingContextMock = new Mock<MappingContext>(metadataMock.Object);
        var entityGraphMapper = new EntityGraphMapper(mappingContextMock.Object);

        var asyncSessionMock = new Mock<IAsyncSession>();
        var asyncTransactionMock = new Mock<IAsyncTransaction>();

        var dateTime = DateTime.UtcNow;

        var iStatementList = new List<IStatement>
        {
            new RowDataStatement(
                "UNWIND $rows as row CREATE (n:`Person`) SET n=row.props RETURN row.nodeRef as ref, ID(n) as id, $type as type",
                new Dictionary<string, object?>
                {
                    {"type", "node"},
                    {"rows", new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"nodeRef", -1},
                            {"props", new Dictionary<string, object>
                            {
                                {"Label", "Person"},
                                {"Name", "John Doe"},
                                {"Posts", Array.Empty<Post>()},
                                {"CreatedAt", dateTime}
                            }}
                        }
                    }}
                })
        };

        driverMock.Setup(x => x.AsyncSession()).Returns(asyncSessionMock.Object);
        asyncSessionMock.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(asyncTransactionMock.Object);

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapper);

        Assert.NotNull(session);

        var model = new Person()
        {
            Name = "John Doe",
            CreatedAt = dateTime,
            Posts = new List<Post>
            {
                new Post()
                {
                    Title = "Hello World",
                }
            }
        };

        await session.SaveAsync(model);

        asyncTransactionMock.Verify(x => x.RunAsync(It.Is<Query>(x =>
            x.Text == iStatementList.First().Statement
            && x.Parameters.Contains(iStatementList.First().Parameters.First()))), Times.Once);
        asyncTransactionMock.Verify(x => x.RunAsync(It.IsAny<Query>()), Times.Exactly(3));
    }

    [Fact]
    public async Task UpdateRelationship()
    {
        var metadataMock = new Mock<MetaData>();
        var driverMock = new Mock<IDriver>();
        var mappingContextMock = new Mock<MappingContext>(metadataMock.Object);
        var entityGraphMapper = new EntityGraphMapper(mappingContextMock.Object);

        var asyncSessionMock = new Mock<IAsyncSession>();
        var asyncTransactionMock = new Mock<IAsyncTransaction>();

        var dateTime = DateTime.UtcNow;

        var iStatementList = new List<IStatement>
        {
            new RowDataStatement(
                "UNWIND $rows as row MATCH (n) WHERE ID(n)=row.nodeId SET n:`Person` SET n += row.props RETURN row.nodeId as ref, ID(n) as id, $type as type",
                new Dictionary<string, object?>
                {
                    {"type", "node"},
                    {"rows", new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"nodeId", 1},
                            {"props", new Dictionary<string, object>
                            {
                                {"Label", "Person"},
                                {"Name", "John Doe"},
                                {"Posts", Array.Empty<Post>()},
                                {"CreatedAt", dateTime}
                            }}
                        }
                    }}
                })
        };

        driverMock.Setup(x => x.AsyncSession()).Returns(asyncSessionMock.Object);
        asyncSessionMock.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(asyncTransactionMock.Object);

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapper);

        Assert.NotNull(session);

        var model = new Person()
        {
            Id = 1,
            Name = "John Doe",
            CreatedAt = dateTime,
            Posts = new List<Post>
            {
                new Post()
                {
                    Title = "Hello World",
                },
                new Post()
                {
                    Title = "Hello World 2",
                }
            }
        };

        await session.SaveAsync(model);

        asyncTransactionMock.Verify(x => x.RunAsync(It.Is<Query>(x =>
            x.Text == iStatementList.First().Statement
            && x.Parameters.Contains(iStatementList.First().Parameters.First()))), Times.Once);
        asyncTransactionMock.Verify(x => x.RunAsync(It.IsAny<Query>()), Times.Exactly(3));
    }

    [Fact]
    public async Task CreateRelationshipEntity()
    {
        var metadataMock = new Mock<MetaData>();
        var driverMock = new Mock<IDriver>();
        var mappingContextMock = new Mock<MappingContext>(metadataMock.Object);
        var entityGraphMapper = new EntityGraphMapper(mappingContextMock.Object);

        var asyncSessionMock = new Mock<IAsyncSession>();
        var asyncTransactionMock = new Mock<IAsyncTransaction>();

        var dateTime = DateTime.UtcNow;

        driverMock.Setup(x => x.AsyncSession()).Returns(asyncSessionMock.Object);
        asyncSessionMock.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(asyncTransactionMock.Object);

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapper);

        Assert.NotNull(session);

        var model = new AuthorsRelationship
        {
            Author = new Author
            {
                Name = "John Doe",
            },
            Note = new Note
            {
                Title = "Hello World",
            }
        };

        await session.SaveAsync(model);

        asyncTransactionMock.Verify(x => x.RunAsync(It.IsAny<Query>()), Times.Exactly(3));
    }

    [Fact(Skip = "Not implemented")]
    public async Task CreateRelationshipEntityFromNode()
    {
        var metadataMock = new Mock<MetaData>();
        var driverMock = new Mock<IDriver>();
        var mappingContextMock = new Mock<MappingContext>(metadataMock.Object);
        var entityGraphMapper = new EntityGraphMapper(mappingContextMock.Object);

        var asyncSessionMock = new Mock<IAsyncSession>();
        var asyncTransactionMock = new Mock<IAsyncTransaction>();

        var dateTime = DateTime.UtcNow;

        driverMock.Setup(x => x.AsyncSession()).Returns(asyncSessionMock.Object);
        asyncSessionMock.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(asyncTransactionMock.Object);

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapper);

        Assert.NotNull(session);

        var model = new Author
        {
            Name = "John Doe",
            AuthorsRelationships = new List<AuthorsRelationship>
           {
               new AuthorsRelationship
               {
                   Note = new Note
                   {
                       Title = "Hello World",
                   }
               }
           }
        };

        await session.SaveAsync(model);

        asyncTransactionMock.Verify(x => x.RunAsync(It.IsAny<Query>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SaveAsyncCallRollbackTest()
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

        asyncTransactionMock.Setup(x => x.CommitAsync()).ThrowsAsync(new Exception());

        var session = new Session(metadataMock.Object, driverMock.Object, entityGraphMapperMock.Object);

        Assert.NotNull(session);

        var model = new Person()
        {
            Name = "John Doe"
        };

        await Assert.ThrowsAsync<Exception>(() => session.SaveAsync(model));

        entityGraphMapperMock.Verify(x => x.Map(It.IsAny<Person>(), It.IsAny<int>()), Times.Once);
        compilerContextMock.Verify(x => x.Compiler, Times.Once);
        multistatementCompilerMock.Verify(x => x.UseStatementFactory(It.IsAny<RowStatementFactory>()), Times.Once);
        asyncTransactionMock.Verify(x => x.RunAsync(It.Is<Query>(x => x.Text == iStatementList.First().Statement)), Times.Once);
        asyncTransactionMock.Verify(x => x.CommitAsync(), Times.Once);
        asyncTransactionMock.Verify(x => x.RollbackAsync(), Times.Once);
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

        Assert.NotNull(session.Metadata);

        session.Dispose();

        Assert.Throws<ObjectDisposedException>(() => session.GetDatabaseSession());
        Assert.Throws<ObjectDisposedException>(() => session.Metadata);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => session.SaveAsync<object>(new object { }));
        Assert.Throws<ObjectDisposedException>(() => session.Set<SimplePerson>());

        // second call
        session.Dispose();
    }
}
