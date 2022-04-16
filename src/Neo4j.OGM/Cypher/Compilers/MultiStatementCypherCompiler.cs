using Neo4j.OGM.Cypher.Compilers.Statements;
using Neo4j.OGM.Requests;
using Neo4j.OGM.Response.Model;

namespace Neo4j.OGM.Cypher.Compilers;

public class MultiStatementCypherCompiler : IMultiStatementCypherCompiler
{
    private readonly CompilerContext _compilerContext;
    private readonly List<NodeBuilder> _newNodeBuilders = new();
    private readonly List<RelationshipBuilder> _newRelationshipBuilders = new();
    private readonly List<RelationshipBuilder> _existingRelationshipBuilders = new();
    private readonly List<NodeBuilder> _existingNodeBuilders = new();
    private IStatementFactory _statementFactory = null!;

    public CompilerContext Context => _compilerContext;

    public MultiStatementCypherCompiler()
    {
        _compilerContext = new CompilerContext(this);
    }

    public IEnumerable<IStatement> GetAllStatements()
    {
        var statements = new List<IStatement>();
        statements.AddRange(CreateNodesStatements());
        statements.AddRange(CreateRelationshipsStatements());
        statements.AddRange(UpdateNodesStatements());
        statements.AddRange(UpdateRelationshipsStatements());
        return statements;
    }

    public IEnumerable<IStatement> UpdateRelationshipsStatements()
    {
        CheckIfNotNull(_statementFactory, "StatementFactory");

        var relationships = new List<RelationshipModel>(_existingRelationshipBuilders.Count);
        var statements = new List<IStatement>();
        if (_existingRelationshipBuilders.Count > 0)
        {
            foreach (var relationshipBuilder in _existingRelationshipBuilders)
            {
                relationships.Add(relationshipBuilder.RelationshipModel());
            }
            var ExistingRelationshipStatementBuilder = new ExistingRelationshipStatementBuilder(relationships, _statementFactory);
            statements.Add(ExistingRelationshipStatementBuilder.Build());
        }

        return statements;
    }

    public IEnumerable<IStatement> UpdateNodesStatements()
    {
        CheckIfNotNull(_statementFactory, "StatementFactory");

        var nodeByLabels = _existingNodeBuilders.Select(x => x.Node).GroupBy(x => x.Label).ToDictionary(x => x.Key, x => x.ToList());
        var statements = new List<IStatement>();
        foreach (var nodeModels in nodeByLabels.Values)
        {
            var existingNodeBuilder = new ExistingNodeStatementBuilder(nodeModels, _statementFactory);
            statements.Add(existingNodeBuilder.Build());
        }

        return statements;
    }

    public IEnumerable<IStatement> CreateRelationshipsStatements()
    {
        CheckIfNotNull(_statementFactory, "StatementFactory");

        Dictionary<string, List<RelationshipModel>> relsByType = new();
        foreach (var newRelationship in _newRelationshipBuilders)
        {
            if (newRelationship.StartNode() == null || newRelationship.EndNode() == null)
            {
                continue;
            }

            var rels = relsByType.GetValueOrDefault(newRelationship.Type)
                ?? new List<RelationshipModel>();

            var model = newRelationship.RelationshipModel();

            rels.Add(model);
            relsByType[newRelationship.Type] = rels;
        }

        List<IStatement> statements = new();
        foreach (var relationships in relsByType.Values)
        {
            var newRelationshipStatementBuilder = new NewRelationshipStatementBuilder(relationships, _statementFactory);
            statements.Add(newRelationshipStatementBuilder.Build());
        }

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

    public IEnumerable<IStatement> CreateNodesStatements()
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

    public bool HasStatementDependentOnNewNode()
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

    public RelationshipBuilder NewRelationship(string type)
    {
        return NewRelationship(type, false);
    }

    public RelationshipBuilder ExistingRelationship(long relId, string type)
    {
        var relationshipBuilder = new RelationshipBuilder(type, relId);
        _existingRelationshipBuilders.Add(relationshipBuilder);
        return relationshipBuilder;
    }

    public RelationshipBuilder NewRelationship(string type, bool mapBothDirections)
    {
        var relationshipBuilder = new RelationshipBuilder(type, mapBothDirections);
        _newRelationshipBuilders.Add(relationshipBuilder);
        return relationshipBuilder;
    }

    private static void CheckIfNotNull(IStatementFactory statementFactory, string name)
    {
        if (statementFactory == null)
        {
            throw new ArgumentNullException(name);
        }
    }
}
