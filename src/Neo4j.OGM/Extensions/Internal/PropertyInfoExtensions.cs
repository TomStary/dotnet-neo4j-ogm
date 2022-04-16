using System.Reflection;
using Neo4j.OGM.Annotations;

namespace Neo4j.OGM.Extensions.Internals;

internal static class PropertyInfoExtensions
{
    internal static bool HasRelationshipAttribute(this PropertyInfo propertyInfo)
    {
        return propertyInfo.GetCustomAttribute<RelationshipAttribute>() != null;
    }
}
