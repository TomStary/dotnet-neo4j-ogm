using System.Reflection;
using Neo4j.OGM.Internals.Extensions;

namespace Neo4j.OGM.Cypher.Compilers;

public class CompilerContext : ICompilerContext
{
    private Dictionary<object, NodeBuilderHorizonPair> _visitedObjects = new();
    private HashSet<long> _visitedRelationshipEntities = new();
    private Dictionary<long, object> _createdOnjectsWithId = new();
    private HashSet<object> _registry = new();
    private readonly IMultiStatementCypherCompiler _compiler;

    public IMultiStatementCypherCompiler Compiler => _compiler;

    public CompilerContext(IMultiStatementCypherCompiler compiler)
    {
        _compiler = compiler;
    }

    public NodeBuilder? VisitedNode(object entity)
    {
        var pair = _visitedObjects.GetValueOrDefault(GetIdentifier(entity));
        return pair?.NodeBuilder;
    }

    public bool Visited(object entity, int horizon)
    {
        var pair = _visitedObjects.GetValueOrDefault(GetIdentifier(entity));
        return pair != null && (horizon < 0 || pair.Horizon > horizon);
    }

    public void VisitRelationshipEntity(long id)
    {
        _visitedRelationshipEntities.Add(id);
    }

    public bool VisitedRelationshipEntity(long id)
    {
        return _visitedRelationshipEntities.Contains(id);
    }

    public void RegisterNewObject(long id, object entity)
    {
        _createdOnjectsWithId.Add(id, entity);
        Register(entity);
    }

    public void Register(object entity)
    {
        if (!_registry.Contains(entity))
        {
            _registry.Add(entity);
        }
    }

    public void Unregister(object entity)
    {
        _registry.Remove(entity);
    }

    private object GetIdentifier(object entity)
    {
        var type = entity.GetType();
        return type.GetMemberInfoOfKeyAttribute()?.GetValue(entity) ?? entity;
    }

    private class NodeBuilderHorizonPair
    {
        private readonly NodeBuilder _nodeBuilder;
        private readonly int _horizon;

        private NodeBuilderHorizonPair(NodeBuilder nodeBuilder, int horizon)
        {
            _nodeBuilder = nodeBuilder;
            _horizon = horizon;
        }

        internal NodeBuilder NodeBuilder => _nodeBuilder;

        internal int Horizon => _horizon;
    }
}
