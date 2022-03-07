using System.Reflection;
using Neo4j.OGM.Annotations;

namespace Neo4j.OGM.Internals.Extensions;

internal static class DictionaryExtensions
{
    internal static Dictionary<string, Type> GetClassesWithRelationshipAttribute(this Dictionary<string, Type> nodeClasses)
    {
        return nodeClasses.Where(pair => pair.Value.GetMembers().Any(member => member.GetCustomAttributes().OfType<RelationshipAttribute>().Any()))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}