using System.Reflection;
using Neo4j.OGM.Annotations;
using Neo4j.OGM.Internals.Extensions;
using static Neo4j.OGM.Annotations.RelationshipAttribute;

namespace Neo4j.OGM.Metadata.Schema;

public class SchemaBuilder
{
    private readonly DomainInfo _domainInfo;

    private readonly SchemaObj _schema;

    public SchemaBuilder(DomainInfo domainInfo)
    {
        _domainInfo = domainInfo;
        _schema = new SchemaObj();
    }

    public ISchema Build()
    {
        BuildNodes();
        BuildRelationships();
        return _schema;
    }

    private void BuildNodes()
    {
        foreach (var nodeClass in _domainInfo.NodeClasses)
        {
            var label = nodeClass.Value.GetNeo4jName();
            _schema.AddNode(label, new Node(label));
        }
    }

    private void BuildRelationships()
    {
        // Process classes with RelationshipEntityAttribute
        foreach (var relationshipClass in _domainInfo.RelationshipClasses)
        {
            var type = relationshipClass.Value.GetNeo4jName();

            var start = GetNodeByField(relationshipClass.Value.GetStartNode());
            var end = GetNodeByField(relationshipClass.Value.GetEndNode());
            _schema.AddRelationship(type, new Relationship(type, DirectionEnum.Outgoing, start, end));
        }

        // Process classes with RelationshipAttribute
        foreach (var nodesWithRelationships in _domainInfo.NodeClasses.GetClassesWithRelationshipAttribute())
        {
            FindAndCreateRelationshipEntities(nodesWithRelationships.Value);
        }
    }

    private void FindAndCreateRelationshipEntities(Type type)
    {
        foreach (var member in type.GetMembers())
        {
            if (member.GetCustomAttributes().OfType<RelationshipAttribute>().Any())
            {
                var relationshipAttribute = member.GetCustomAttributes().OfType<RelationshipAttribute>().First();
                var relationshipType = relationshipAttribute.Type;
                var direction = relationshipAttribute.Direction;
                var fromNode = _schema.FindNode(type.GetNeo4jName());
                INode toNode;

                if (member.GetType().HasRelationshipEntityAttribute())
                {
                    if (direction == DirectionEnum.Outgoing)
                    {
                        toNode = _schema.FindNode(member.GetType().GetEndNode().GetType().GetNeo4jName());
                    }
                    else
                    {
                        toNode = _schema.FindNode(member.GetType().GetStartNode().GetType().GetNeo4jName());
                    }
                }
                else
                {
                    toNode = _schema.FindNode(member.GetType().GetNeo4jName());
                }

                var relationship = new Relationship(relationshipType, direction, fromNode, toNode);

                fromNode.AddRelationship(relationship.Type, relationship);
            }
        }
    }

    private INode GetNodeByField(MemberInfo member)
    {
        var classType = member.GetType();

        if (classType != null)
        {
            return _schema.FindNode(classType.GetNeo4jName());
        }

        throw new Exception("Could not find node for relationship class."); // TODO: custom exception
    }
}