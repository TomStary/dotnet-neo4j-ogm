using System.Reflection;

namespace Neo4j.OGM.Internals.Extensions;

internal static class PropertyInfoExtensions
{
    internal static Type GetImplementedType(this PropertyInfo propertyInfo)
    {
        return propertyInfo.PropertyType.GetAnyElementType();
    }
}
