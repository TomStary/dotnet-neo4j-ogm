using System.Collections.Generic;
using System.Runtime;
using Neo4j.OGM.Annotations;
using Neo4j.OGM.Cypher.Compilers;
using Neo4j.OGM.Exceptions;
using Neo4j.OGM.Internals.Extensions;
using Neo4j.OGM.Metadata;

namespace Neo4j.OGM.Context;

public class EntityGraphMapper : IEntityMapper
{
    private MetaData _metadata;
    private readonly MappingContext _mappingContext;
    private readonly MultiStatementCypherCompiler _compiler;
    private int _currentDepth = 0;

    public EntityGraphMapper(
        MetaData metadata,
        MappingContext mappingContext
    )
    {
        _metadata = metadata;
        _mappingContext = mappingContext;
        _compiler = new MultiStatementCypherCompiler();
    }

    public ICompilerContext Map(object entity, int depth)
    {
        _currentDepth = 0;

        if (entity == null)
        {
            throw new NullReferenceException("Missing entity to map.");
        }

        if (entity.GetType().HasRelationshipEntityAttribute())
        {
            var startNode = entity.GetType().GetStartNode().GetValue(entity);
            if (startNode == null)
            {
                throw new NullReferenceException("Missing value of start node for relationship.");
            }

            var endNode = entity.GetType().GetEndNode().GetValue(entity);
            if (endNode == null)
            {
                throw new NullReferenceException("Missing value of end node for relationship.");
            }

            var startNodeBuilder = MapEntity(startNode, depth);
            var endNodeBuilder = MapEntity(endNode, depth);

            if (!_compiler.Context.VisitedRelationshipEntity(_mappingContext.NativeId(entity)))
            {
                throw new NotImplementedException();
            }
        }
        else
        {
            MapEntity(entity, depth);
        }

        return _compiler.Context;
    }

    public ICompilerContext CompilerContext()
    {
        return _compiler.Context;
    }

    private NodeBuilder? MapEntity(object entity, int horizon)
    {
        if (entity.GetType().IsAbstract || entity.GetType().IsInterface)
        {
            return null;
        }

        var context = _compiler.Context;
        var nodeBuilder = context.VisitedNode(entity);

        if (context.Visited(entity, horizon))
        {
            return nodeBuilder;
        }

        if (nodeBuilder == null)
        {
            nodeBuilder = NewNodeBuilder(entity, horizon);
            UpdateNode(entity, context, nodeBuilder);
        }

        if (horizon != 0)
        {
            MapEntityReferences(entity, nodeBuilder, horizon - 1);
        }

        return nodeBuilder;
    }

    private void MapEntityReferences(object entity, NodeBuilder nodeBuilder, int horizon)
    {
        _ = Interlocked.Increment(ref _currentDepth);

        _ = _mappingContext.NativeId(entity);

        foreach (var relationship in entity.GetType().GetRelationshipsMembers())
        {
            var type = relationship.GetRelationshipType();
            var direction = relationship.GetRelationshipDirection();
            var startNodeType = relationship.DeclaringType;
            var endNodeType = relationship.GetEndNodeType();

            var directedRelationship = new DirectedRelationship(type, direction);

            var relatedObject = startNodeType?.GetValue(entity);
            if (relatedObject != null)
            {
                if (relationship?.DeclaringType?.HasRelationshipEntityAttribute() ?? false)
                {
                    if (relationship.DeclaringType.GetNeo4jName() != type)
                    {
                        directedRelationship = new DirectedRelationship(relationship.DeclaringType.GetNeo4jName(), direction);
                    }
                }

                var relNodes = new RelationshipNodes(entity, relatedObject, startNodeType, endNodeType);
                Link(directedRelationship, nodeBuilder, horizon, entity.Equals(relatedObject), relNodes);
            }
        }
        _ = Interlocked.Decrement(ref _currentDepth);
    }

    private void Link(DirectedRelationship directedRelationship, NodeBuilder nodeBuilder, int horizon, bool mapBothDirections, RelationshipNodes relNodes)
    {
        if (relNodes.Target != null)
        {
            var context = _compiler.Context;

            var relationshipBuilder = GetRelationshipBuilder(_compiler, relNodes.Target, directedRelationship, mapBothDirections);
            if (relNodes.Target.GetType().HasRelationshipEntityAttribute())
            {
                var relId = _mappingContext.NativeId(relNodes.Target);
                if (context.VisitedRelationshipEntity(relId))
                {
                    MapRelationshipEntity(relNodes.Target, relNodes.Source, relationshipBuilder, context, nodeBuilder, horizon, relNodes.SourceType, relNodes.TargetType);
                }
            }
        }
    }

    private void MapRelationshipEntity(
        object relationshipEntity,
        object parent,
        RelationshipBuilder relationshipBuilder,
        CompilerContext context,
        NodeBuilder nodeBuilder,
        int horizon,
        Type startNodeType,
        Type endNodeType
    )
    {
        var startEntity = relationshipEntity.GetType().GetStartNode().GetValue(relationshipEntity);
        var targetEntity = relationshipBuilder.GetType().GetEndNode().GetValue(relationshipBuilder);

        var tgtId = _mappingContext.NativeId(targetEntity!);
        var srcId = _mappingContext.NativeId(startEntity!);

        RelationshipNodes nodes;

        if (parent == targetEntity)
        {
            nodes = new RelationshipNodes(targetEntity, startEntity, startNodeType, endNodeType);
        }
        else
        {
            nodes = new RelationshipNodes(startEntity, targetEntity, startNodeType, endNodeType);
        }

        if (_mappingContext.HasChanges(relationshipEntity))
        {
            context.Register(relationshipEntity);
            if (tgtId >= 0 && srcId >= 0)
            {
                var mappedRelationship = CreateMappedRelationship(relationshipBuilder, nodes);
            }
        }

        var srcNodeBuilder = context.VisitedNode(startEntity);
        var tgtNodeBuilder = context.VisitedNode(startEntity);

        if (parent == targetEntity)
        {
            if (!context.Visited(startEntity, horizon))
            {
                nodes.Source = targetEntity;
                nodes.Target = startEntity;
                MapRelatedEntity(nodeBuilder, relationshipBuilder, _currentDepth, horizon, nodes);
            }
        }
    }

    private void MapRelatedEntity(NodeBuilder nodeBuilder, RelationshipBuilder relationshipBuilder, int level, int horizon, RelationshipNodes nodes)
    {
        var context = _compiler.Context;
        var alreadyVisited = context.Visited(nodes.Target, horizon);
        var selfReferentialUndirectedRel = relationshipBuilder.HasDirection(RelationshipAttribute.DirectionEnum.Undirected)
            && nodes.Source.GetType() == nodes.Target.GetType();
        var relationshipFromExplicitlyMappedObject = level == 1;

        var tgtNodeBuilder = MapEntity(nodes.Target, horizon);

        if (!alreadyVisited || !selfReferentialUndirectedRel || relationshipFromExplicitlyMappedObject)
        {
            nodes.TargetId = _mappingContext.NativeId(nodes.Target);
            UpdateRelationship(context, nodeBuilder, tgtNodeBuilder, relationshipBuilder, nodes);
        }
    }

    private void UpdateRelationship(CompilerContext context, NodeBuilder nodeBuilder, NodeBuilder tgtNodeBuilder, RelationshipBuilder relationshipBuilder, RelationshipNodes nodes)
    {
        throw new NotImplementedException();
    }

    private object CreateMappedRelationship(RelationshipBuilder relationshipBuilder, RelationshipNodes nodes)
    {
        var isRelEntity = relationshipBuilder.RelationshipEntity;
        MappedRelationship mappedRelationshipOutgoing = new MappedRelationship(
            nodes.SourceId,
            relationshipBuilder.Type,
            nodes.TargetId,
            isRelEntity ? relationshipBuilder.Reference : null,
            nodes.SourceType,
            nodes.TargetType);

        MappedRelationship mappedRelationshipIncoming = new MappedRelationship(
            nodes.TargetId,
            relationshipBuilder.Type,
            nodes.SourceId,
            isRelEntity ? relationshipBuilder.Reference : null,
            nodes.TargetType,
            nodes.SourceType);

        if (relationshipBuilder.HasDirection(RelationshipAttribute.DirectionEnum.Undirected))
        {
            if (_mappingContext.ContainsRelationship(mappedRelationshipIncoming))
            {
                return mappedRelationshipIncoming;
            }
            return mappedRelationshipOutgoing;
        }

        if (relationshipBuilder.HasDirection(RelationshipAttribute.DirectionEnum.Incoming))
        {
            return mappedRelationshipIncoming;
        }

        return mappedRelationshipOutgoing;
    }

    private RelationshipBuilder GetRelationshipBuilder(MultiStatementCypherCompiler compiler, object entity, DirectedRelationship directedRelationship, bool mapBothDirections)
    {
        RelationshipBuilder relationshipBuilder;

        if (entity.GetType().HasRelationshipEntityAttribute())
        {
            var relId = _mappingContext.NativeId(entity);

            var newRelationship = relId < 0;
            var hasChanges = HaveRelationEndsChanged(entity, relId);

            if (newRelationship || hasChanges)
            {
                relationshipBuilder = compiler.NewRelationship(directedRelationship.Type);
            }
            else
            {
                relationshipBuilder = compiler.ExistingRelationship(relId, directedRelationship.Type);
            }
        }
        else
        {
            relationshipBuilder = compiler.NewRelationship(directedRelationship.Type, mapBothDirections);
        }

        relationshipBuilder.Direction(directedRelationship.Direction);

        if (entity.GetType().HasRelationshipEntityAttribute())
        {
            relationshipBuilder.Singleton = false;
            relationshipBuilder.Reference = _mappingContext.NativeId(entity);
            relationshipBuilder.RelationshipEntity = true;
        }

        return relationshipBuilder;
    }

    private bool HaveRelationEndsChanged(object entity, long relId)
    {
        var startNode = entity.GetType().GetStartNode().GetValue(entity);
        var endNode = entity.GetType().GetEndNode().GetValue(entity);

        if (startNode == null || endNode == null)
        {
            throw new MappingException("Missing value of start node or end node for relationship.");
        }

        var startNodeId = _mappingContext.NativeId(startNode);
        var endNodeId = _mappingContext.NativeId(endNode);

        var relChanged = false;

        foreach (var mappedRelationship in _mappingContext.GetRelationships())
        {
            if (mappedRelationship.GetRelationshipId() == relId)
            {
                if (mappedRelationship.GetStartNodeId() != startNodeId || mappedRelationship.GetEndNodeId() != endNodeId)
                {
                    relChanged = true;
                    break;
                }
            }
        }

        return relChanged;
    }

    private void UpdateNode(object entity, CompilerContext context, NodeBuilder nodeBuilder)
    {
        if (_mappingContext.HasChanges(entity))
        {
            context.Register(entity);
            UpdatePropertiesOnBuilder(entity, nodeBuilder);
        }
        else
        {
            context.Unregister(entity);
        }
    }

    private static void UpdatePropertiesOnBuilder(object entity, NodeBuilder nodeBuilder)
    {
        foreach (var property in entity.GetType().GetProperties())
        {
            if (!property.CanRead || property.HasKeyAttribute())
            {
                continue;
            }

            nodeBuilder.AddProperty(property.Name, property.GetValue(entity));
        }
        nodeBuilder.SetProperties();
    }

    private NodeBuilder NewNodeBuilder(object entity, int depth)
    {
        var context = _compiler.Context;

        var id = _mappingContext.NativeId(entity);
        var labels = entity.GetType().GetNeo4jLabels(entity);

        NodeBuilder nodeBuilder;

        if (id < 0)
        {
            nodeBuilder = _compiler.CreateNode(id).AddLabels(labels);
            context.RegisterNewObject(id, entity);
        }
        else
        {
            nodeBuilder = _compiler.ExistingNode(id);
            nodeBuilder.AddLabels(labels);
        }

        return nodeBuilder;
    }

    private class RelationshipNodes
    {
        internal long SourceId;
        internal long TargetId;
        internal object Source;
        internal object Target;
        internal Type SourceType;
        internal Type TargetType;

        public RelationshipNodes(object source, object target, Type startNodeType, Type endNodeType)
        {
            Source = source;
            Target = target;
            SourceType = startNodeType;
            TargetType = endNodeType;
        }
    }
}
