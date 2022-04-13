using System.Reflection;
using Neo4j.OGM.Internals.Extensions;

namespace Neo4j.OGM.Cypher.Compilers;

public class CompilerContext : ICompilerContext
{
    private Dictionary<object, NodeBuilderHorizonPair> _visitedObjects = new();
    private HashSet<long> _visitedRelationshipEntities = new();
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

    public void Visit(object entity, NodeBuilder nodeBuilder, int horizon)
    {
        _visitedObjects.Add(GetIdentifier(entity), new NodeBuilderHorizonPair(nodeBuilder, horizon));
    }

    public void VisitRelationshipEntity(long id)
    {
        _visitedRelationshipEntities.Add(id);
    }

    public bool VisitedRelationshipEntity(long id)
    {
        return _visitedRelationshipEntities.Contains(id);
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

        public NodeBuilderHorizonPair(NodeBuilder nodeBuilder, int horizon)
        {
            _nodeBuilder = nodeBuilder;
            _horizon = horizon;
        }

        internal NodeBuilder NodeBuilder => _nodeBuilder;

        internal int Horizon => _horizon;
    }
}
