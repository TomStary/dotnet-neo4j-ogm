using System.Reflection;
using Neo4j.OGM.Annotations;
using Neo4j.OGM.Exceptions;

namespace Neo4j.OGM.Internals.Extensions;

internal static class TypeExtensions
{
    internal static bool HasNodeAttribute(this Type type)
    {
        var attrs = type.GetCustomAttributes();

        return attrs.Any(attribute => attribute is NodeAttribute);
    }

    internal static bool HasRelationshipEntityAttribute(this Type type)
    {
        var attrs = type.GetCustomAttributes();

        return attrs.Any(attribute => attribute is RelationshipEntityAttribute);
    }

    /// <summary>
    /// Returns Neo4j name for Node or Relationship class.
    /// </summary>
    /// <param name="type"></param>
    /// <exception cref="Neo4j.OGM.Exceptions.MissingNodeAttributeException"></exception>
    /// <returns></returns>
    internal static string GetNeo4jName(this Type type)
    {
        var attributes = type.GetCustomAttributes();
        var nodeAttribute = attributes.OfType<NodeAttribute>().FirstOrDefault();

        if (nodeAttribute != null)
        {
            return nodeAttribute.Label;
        }

        var relationshipAttribute = attributes.OfType<RelationshipEntityAttribute>().FirstOrDefault();

        if (relationshipAttribute != null)
        {
            return relationshipAttribute.Type;
        }

        return nameof(type);
    }

    internal static MemberInfo GetStartNode(this Type type)
    {
        var members = type.GetMembers();

        var startNode = members.FirstOrDefault(member => member.GetCustomAttributes().OfType<StartNodeAttribute>().Any());

        if (startNode == null)
        {
            throw new MissingStartNodeAttributeException($"Class: {nameof(type)} does not have a StartNodeAttribute.");
        }

        return startNode;
    }

    internal static MemberInfo GetEndNode(this Type type)
    {
        var members = type.GetMembers();

        var endNode = members.FirstOrDefault(member => member.GetCustomAttributes().OfType<EndNodeAttribute>().Any());

        if (endNode == null)
        {
            throw new MissingEndNodeAttributeException($"Class: {nameof(type)} does not have a EndNodeAttribute.");
        }

        return endNode;
    }
}