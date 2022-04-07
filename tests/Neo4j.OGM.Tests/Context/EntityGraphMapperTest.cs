using System.Reflection;
using Moq;
using Neo4j.OGM.Context;
using Neo4j.OGM.Cypher.Compilers;
using Neo4j.OGM.Metadata;
using Neo4j.OGM.Requests;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests.Context;

public class EntityGraphMapperTest
{
    [Fact]
    public void MapTestEntityWithInfiniteDepth()
    {
        var assembly = new Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(SimplePerson) });

        var metadata = new MetaData(assembly.Object);
        var mappingContext = new MappingContext(metadata);
        var entityGraphMapper = new EntityGraphMapper(metadata, mappingContext);

        var model = new SimplePerson()
        {
            Name = "John Doe"
        };

        var result = entityGraphMapper.Map(model, -1);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<ICompilerContext>(result);
        result.Compiler.UseStatementFactory(new RowStatementFactory());
        Assert.NotNull(result.Compiler.GetAllStatements());
    }

    [Fact]
    public void MapTestRelationshipWithInfiniteDepth()
    {
        var assembly = new Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(Person), typeof(Post) });

        var metadata = new MetaData(assembly.Object);
        var mappingContext = new MappingContext(metadata);
        var entityGraphMapper = new EntityGraphMapper(metadata, mappingContext);

        var model = new Person()
        {
            Name = "John Doe",
            Posts = new List<Post> { new Post() { Title = "Hello World" } },
        };

        var result = entityGraphMapper.Map(model, -1);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<ICompilerContext>(result);
        result.Compiler.UseStatementFactory(new RowStatementFactory());
        Assert.NotNull(result.Compiler.GetAllStatements());
    }

    [Fact(Skip = "Not implemented.")]
    public void MapTestRelationshipEntityWithInfiniteDepth()
    {
        var assembly = new Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(Author), typeof(AuthorsRelationship), typeof(Note) });

        var metadata = new MetaData(assembly.Object);
        var mappingContext = new MappingContext(metadata);
        var entityGraphMapper = new EntityGraphMapper(metadata, mappingContext);

        var model = new AuthorsRelationship
        {
            Author = new Author() { Name = "John Doe" },
            Note = new Note() { Title = "Hello World" }
        };

        var result = entityGraphMapper.Map(model, -1);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<ICompilerContext>(result);
        result.Compiler.UseStatementFactory(new RowStatementFactory());
        Assert.NotNull(result.Compiler.GetAllStatements());
    }

    [Fact]
    public void MapTestNullReferenceException()
    {
        var assembly = new Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(SimplePerson) });

        var metadata = new MetaData(assembly.Object);
        var mappingContext = new MappingContext(metadata);
        var entityGraphMapper = new EntityGraphMapper(metadata, mappingContext);

        Assert.Throws<ArgumentNullException>(() => entityGraphMapper.Map(default(SimplePerson), -1));
    }
}
