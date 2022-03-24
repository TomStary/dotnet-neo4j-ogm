using Neo4j.OGM.Annotations;
using Neo4j.OGM.Cypher.Compilers.Statements;
using Neo4j.OGM.Requests;

namespace Neo4j.OGM.Cypher.Compilers;

public class MultiStatementCypherCompiler
{
    private readonly CompilerContext _compilerContext;
    private readonly List<NodeBuilder> _newNodeBuilders = new();
    private readonly List<RelationshipBuilder> _newRelationshipBuilders = new();
    private readonly List<RelationshipBuilder> _existingRelationshipBuilders = new();
    private readonly List<NodeBuilder> _existingNodeBuilders = new();
    private IStatementFactory _statementFactory;

    public CompilerContext Context => _compilerContext;

    public MultiStatementCypherCompiler()
    {
        _compilerContext = new CompilerContext(this);
    }

    internal IEnumerable<IStatement> GetAllStatements()
    {
        var statements = new List<IStatement>();
        statements.AddRange(CreateNodesStatements());
        return statements;
    }

    public void UseStatementFactory(IStatementFactory statementFactory)
    {
        _statementFactory = statementFactory;
    }

    public NodeBuilder CreateNode(long id)
    {
        NodeBuilder builder = new(id);
        _newNodeBuilders.Add(builder);
        return builder;
    }

    public NodeBuilder ExistingNode(long id)
    {
        var node = new NodeBuilder(id);
        _existingNodeBuilders.Add(node);
        return node;
    }

    internal IEnumerable<IStatement> CreateNodesStatements()
    {
        CheckIfNotNull(_statementFactory, "StatementFactory");

        var newNodeByLabels = _newNodeBuilders.Select(x => x.Node).GroupBy(x => x.Label).ToDictionary(x => x.Key, x => x.ToList());
        var statements = new List<IStatement>(newNodeByLabels.Count);
        foreach (var nodeModels in newNodeByLabels.Values)
        {
            var newNodeStatementBuilder = new NewNodeStatementBuilder(nodeModels, _statementFactory);
            statements.Add(newNodeStatementBuilder.Build());
        }

        return statements;
    }

    internal bool HasStatementDependentOnNewNode()
    {
        foreach (var builder in _newRelationshipBuilders)
        {
            if (builder.StartNode() != null
                && builder.EndNode() != null
                && builder.StartNode() < 0
                && builder.EndNode() < 0
            )
            {
                return true;
            }
        }
        return false;
    }

    private static void CheckIfNotNull(IStatementFactory statementFactory, string name)
    {
        if (statementFactory == null)
        {
            throw new ArgumentNullException(name);
        }
    }

    internal RelationshipBuilder NewRelationship(string type)
    {
        return NewRelationship(type, false);
    }

    internal RelationshipBuilder ExistingRelationship(long relId, string type)
    {
        var relationshipBuilder = new RelationshipBuilder(type, relId);
        _existingRelationshipBuilders.Add(relationshipBuilder);
        return relationshipBuilder;
    }

    internal RelationshipBuilder NewRelationship(string type, bool mapBothDirections)
    {
        var relationshipBuilder = new RelationshipBuilder(type, mapBothDirections);
        _newRelationshipBuilders.Add(relationshipBuilder);
        return relationshipBuilder;
    }
}
