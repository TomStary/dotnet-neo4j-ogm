using Moq;
using Neo4j.OGM.Cypher.Compilers;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests.Cypher.Compilers;

public class CompilerContextTest
{
    [Fact]
    public void VisitedNodeTest()
    {
        var multistatementCompilerMock = new Mock<IMultiStatementCypherCompiler>();

        var compilerContext = new CompilerContext(multistatementCompilerMock.Object);

        var node = new SimplePerson
        {
            Id = -1,
        };

        // node has not been visited, thus not added into internal cache
        var nodeBuilder = compilerContext.VisitedNode(node);

        Assert.Null(nodeBuilder);

        compilerContext.Visit(node, new NodeBuilder(-1), -1);

        nodeBuilder = compilerContext.VisitedNode(node);
        Assert.NotNull(nodeBuilder);
    }

    [Fact]
    public void VisitedTest()
    {
        var multistatementCompilerMock = new Mock<IMultiStatementCypherCompiler>();

        var compilerContext = new CompilerContext(multistatementCompilerMock.Object);

        var node = new SimplePerson
        {
            Id = -1,
        };

        // node has not been visited, thus not added into internal cache
        var visited = compilerContext.Visited(node, 0);

        Assert.False(visited);

        compilerContext.Visit(node, new NodeBuilder(-1), 3);

        visited = compilerContext.Visited(node, 3);
        Assert.False(visited);

        visited = compilerContext.Visited(node, 1);
        Assert.True(visited);
    }

    [Fact]
    public void VisitRelationshipEntityTest()
    {
        var multistatementCompilerMock = new Mock<IMultiStatementCypherCompiler>();

        var compilerContext = new CompilerContext(multistatementCompilerMock.Object);

        var id = -1;

        // relationship entity has not been visited, thus not added into internal cache
        var visited = compilerContext.VisitedRelationshipEntity(id);

        Assert.False(visited);

        compilerContext.VisitRelationshipEntity(id);

        visited = compilerContext.VisitedRelationshipEntity(id);
        Assert.True(visited);
    }
}
