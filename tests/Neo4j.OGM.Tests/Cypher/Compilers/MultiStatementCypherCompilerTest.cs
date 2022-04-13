using Neo4j.OGM.Cypher.Compilers;

namespace Neo4j.OGM.Tests.Cypher.Compilers;

public class MultiStatementCypherCompilerTest
{
    [Fact]
    public void CreateNodeTest()
    {
        var compiler = new MultiStatementCypherCompiler();

        var nodeBuilder = compiler.CreateNode(-1);

        Assert.NotNull(nodeBuilder);
        Assert.Equal(-1, nodeBuilder.Id);
        Assert.IsAssignableFrom<NodeBuilder>(nodeBuilder);
    }

    [Fact]
    public void ExistingNodeTest()
    {
        var compiler = new MultiStatementCypherCompiler();

        var nodeBuilder = compiler.ExistingNode(1);

        Assert.NotNull(nodeBuilder);
        Assert.Equal(1, nodeBuilder.Id);
        Assert.IsAssignableFrom<NodeBuilder>(nodeBuilder);
    }

    [Fact]
    public void NewRelationshipTest()
    {
        var compiler = new MultiStatementCypherCompiler();

        var relationshipBuilder = compiler.NewRelationship("AUTHORS");

        Assert.NotNull(relationshipBuilder);
        Assert.Equal("AUTHORS", relationshipBuilder.Type);
        Assert.IsAssignableFrom<RelationshipBuilder>(relationshipBuilder);
    }

    [Fact]
    public void ExistingRelationshipTest()
    {
        var compiler = new MultiStatementCypherCompiler();

        var relationshipBuilder = compiler.ExistingRelationship(1, "AUTHORS");

        Assert.NotNull(relationshipBuilder);
        Assert.Equal("AUTHORS", relationshipBuilder.Type);
        Assert.IsAssignableFrom<RelationshipBuilder>(relationshipBuilder);
    }

    [Fact]
    public void HasStatementDependentOnNewNodeTest()
    {
        var compiler = new MultiStatementCypherCompiler();

        var hasDependencyOnNewNode = compiler.HasStatementDependentOnNewNode();
        Assert.False(hasDependencyOnNewNode);

        var relationshipBuilder = compiler.NewRelationship("AUTHORS");
        relationshipBuilder.SetStartNode(-1);
        relationshipBuilder.SetEndNode(-2);

        hasDependencyOnNewNode = compiler.HasStatementDependentOnNewNode();
        Assert.True(hasDependencyOnNewNode);

        compiler = new MultiStatementCypherCompiler();

        relationshipBuilder = compiler.NewRelationship("AUTHORS");
        relationshipBuilder.SetStartNode(1);
        relationshipBuilder.SetEndNode(2);

        hasDependencyOnNewNode = compiler.HasStatementDependentOnNewNode();
        Assert.False(hasDependencyOnNewNode);
    }

    [Fact]
    public void GetAllStatementsTest()
    {
        var compiler = new MultiStatementCypherCompiler();

        Assert.Throws<ArgumentNullException>(() => compiler.GetAllStatements());


    }
}
