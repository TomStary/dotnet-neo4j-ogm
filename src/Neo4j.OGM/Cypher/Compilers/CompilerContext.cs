using System.ComponentModel;
using System.Reflection;
using Microsoft.Win32;
using Neo4j.OGM.Context;
using Neo4j.OGM.Internals.Extensions;

namespace Neo4j.OGM.Cypher.Compilers;

public class CompilerContext : ICompilerContext
{
    private readonly Dictionary<object, NodeBuilderHorizonPair> _visitedObjects = new();
    private readonly Dictionary<Tuple<long, long>, IEnumerable<object>> _transientRelationships = new();
    private readonly HashSet<long> _visitedRelationshipEntities = new();
    private readonly IMultiStatementCypherCompiler _compiler;
    private readonly HashSet<object> _registry = new();
    private readonly List<MappedRelationship> _relationshipRegister = new();

    public IMultiStatementCypherCompiler Compiler => _compiler;

    public CompilerContext(IMultiStatementCypherCompiler compiler)
    {
        _compiler = compiler;
    }

    public NodeBuilder? VisitedNode(object? entity)
    {
        if (entity == null)
        {
            return null;
        }

        var pair = _visitedObjects.GetValueOrDefault(GetIdentifier(entity));
        return pair?.NodeBuilder;
    }

    public bool Visited(object? entity, int horizon)
    {
        if (entity == null)
        {
            return false;
        }
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

    internal IEnumerable<object> GetTransientRelationships(Tuple<long, long> tuple)
    {
        return _transientRelationships.GetValueOrDefault(tuple) ?? Array.Empty<object>();
    }

    internal void RegisterTransientRelationship(Tuple<long, long> tuple, TransientRelationship transientRelationship)
    {
        if (!_registry.Contains(transientRelationship))
        {
            _registry.Add(transientRelationship);
            var value = _transientRelationships.GetValueOrDefault(tuple);
            _transientRelationships[tuple] = value != null ? value.Append(transientRelationship) : (IEnumerable<object>)(new[] { transientRelationship });
        }
    }

    internal void RegisterRelationship(MappedRelationship mappedRelationship)
    {
        _relationshipRegister.Add(mappedRelationship);
    }
}
