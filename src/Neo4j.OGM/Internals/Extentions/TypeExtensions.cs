using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Neo4j.OGM.Annotations;
using Neo4j.OGM.Exceptions;

namespace Neo4j.OGM.Internals.Extensions;

internal static class TypeExtensions
{
    /// <summary>
    /// Check if given <see cref="Type" does have the <see cref="NodeAttribute"> applied as custom attribute.
    /// </summary>
    internal static bool HasNodeAttribute(this Type type)
        => type.GetCustomAttributes().Any(attribute => attribute is NodeAttribute);

    /// <summary>
    /// Check if given <see cref="Type" does have the <see cref="RelationshipEntityAttribute"> applied as custom attribute.
    /// </summary>
    internal static bool HasRelationshipEntityAttribute(this Type type)
        => type.GetCustomAttributes().Any(attribute => attribute is RelationshipEntityAttribute);

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

        if (nodeAttribute != null
            && !string.IsNullOrEmpty(nodeAttribute.Label))
        {
            return nodeAttribute.Label;
        }

        var relationshipAttribute = attributes.OfType<RelationshipEntityAttribute>().FirstOrDefault();

        if (relationshipAttribute != null
            && !string.IsNullOrEmpty(relationshipAttribute.Type))
        {
            return relationshipAttribute.Type;
        }

        return type.Name;
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

    internal static bool HasKeyAttribute(this PropertyInfo property)
    {
        return property.GetCustomAttributes().OfType<KeyAttribute>().Any();
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

    internal static IEnumerable<string> GetNeo4jLabels(this Type type, object entity)
    {
        var attributes = type.GetCustomAttributes();
        var nodeAttribute = attributes.OfType<NodeAttribute>().FirstOrDefault();
        var labels = new List<string>()
        {
            type.GetNeo4jName(),
        };

        if (nodeAttribute != null)
        {
            labels.Add(nodeAttribute.Label);
        }

        return labels;
    }

    /// <summary>
    /// Return list of <see cref="PropertyInfo" /> for all properties that are not <see cref="RelationshipAttribute" /> or <see cref="KeyAttribute" />.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    internal static IEnumerable<PropertyInfo> GetNeo4jProperties(this Type type)
    {
        var properties = type.GetProperties();

        return properties.Where(property =>
            !property.GetCustomAttributes().OfType<RelationshipAttribute>().Any()
            && !property.GetCustomAttributes().OfType<KeyAttribute>().Any());
    }

    internal static IEnumerable<MemberInfo> GetRelationshipsMembers(this Type type)
    {
        var members = type.GetMembers();

        return members.Where(member => member.GetCustomAttributes().OfType<RelationshipAttribute>().Any());
    }

    //cc: https://stackoverflow.com/a/17713382 changed to extension and changed code style
    public static Type GetAnyElementType(this Type type)
    {
        // Type is Array
        // short-circuit if you expect lots of arrays
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        // type is IEnumerable<T>;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return type.GetGenericArguments()[0];
        }

        // type implements/extends IEnumerable<T>;
        var enumType = type.GetInterfaces()
                                .Where(t => t.IsGenericType &&
                                       t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                .Select(t => t.GenericTypeArguments[0]).FirstOrDefault();
        return enumType ?? type;
    }

    internal static string GetRelationshipType(this MemberInfo member)
    {
        var attributes = member.GetCustomAttributes();
        var relationshipAttribute = attributes.OfType<RelationshipAttribute>().FirstOrDefault();

        return relationshipAttribute != null ? relationshipAttribute.Type : member.Name;
    }

    internal static RelationshipAttribute.DirectionEnum GetRelationshipDirection(this MemberInfo member)
    {
        var attributes = member.GetCustomAttributes();
        var relationshipAttribute = attributes.OfType<RelationshipAttribute>().FirstOrDefault();

        return relationshipAttribute != null ? relationshipAttribute.Direction : RelationshipAttribute.DirectionEnum.Outgoing;
    }

    internal static Type GetEndNodeType(this MemberInfo member)
    {
        var type = member.DeclaringType;

        return type switch
        {
            null => throw new MappingException($"{nameof(member)} does not have a DeclaringType."),
            _ => type.HasRelationshipEntityAttribute()
            ? type.GetEndNode().DeclaringType ?? throw new MappingException($"{nameof(type)} does not have a EndNode.")
            : type
        };
    }

    internal static bool HasIdentityProperty(this Type type)
    {
        return type.GetProperties().Any(property => property.GetCustomAttributes().OfType<KeyAttribute>().Any() && property.PropertyType == typeof(long?));
    }

    internal static MemberInfo? GetMemberInfoOfKeyAttribute(this Type type)
    {
        return type.GetProperties().FirstOrDefault(property => property.GetCustomAttributes().OfType<KeyAttribute>().Any());
    }

    internal static bool HasPrimaryIndexAttribute(this Type type)
    {
        return type.GetProperties().Any(property => property.GetCustomAttributes().OfType<KeyAttribute>().Any());
    }

    internal static PropertyInfo? FindProperty(this Type type, string propertyName)
    {
        return type.GetProperties().FirstOrDefault(property => property.Name == propertyName);
    }

    internal static PropertyInfo? FindProperty(this Type type, MemberInfo property)
        => type.FindProperty(property.Name);

    internal static PropertyInfo GetRequiredProperty(this Type type, string name)
       => type.GetTypeInfo().GetProperty(name)
           ?? throw new InvalidOperationException($"Could not find property '{name}' on type '{type}'");

    internal static object? GetKeyValue(this Type type, object entity)
    {
        var keyAttribute = type.GetMemberInfoOfKeyAttribute();

        if (keyAttribute == null)
        {
            throw new MappingException($"Could not find key attribute on type: {type.Name}");
        }

        return keyAttribute.GetValue(entity);
    }

    // source https://github.com/dotnet/efcore
    public static Type GetSequenceType(this Type type)
    {
        var sequenceType = TryGetSequenceType(type);
        if (sequenceType == null)
        {
            throw new ArgumentException($"The type {type.Name} does not represent a sequence");
        }

        return sequenceType;
    }

    // source https://github.com/dotnet/efcore
    public static Type? TryGetSequenceType(this Type type)
            => type.TryGetElementType(typeof(IEnumerable<>))
                ?? type.TryGetElementType(typeof(IAsyncEnumerable<>));

    // source https://github.com/dotnet/efcore
    public static Type? TryGetElementType(this Type type, Type interfaceOrBaseType)
    {
        if (type.IsGenericTypeDefinition)
        {
            return null;
        }

        var types = GetGenericTypeImplementations(type, interfaceOrBaseType);

        Type? singleImplementation = null;
        foreach (var implementation in types)
        {
            if (singleImplementation == null)
            {
                singleImplementation = implementation;
            }
            else
            {
                singleImplementation = null;
                break;
            }
        }

        return singleImplementation?.GenericTypeArguments.FirstOrDefault();
    }

    // source https://github.com/dotnet/efcore
    public static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
    {
        var typeInfo = type.GetTypeInfo();
        if (!typeInfo.IsGenericTypeDefinition)
        {
            var baseTypes = interfaceOrBaseType.GetTypeInfo().IsInterface
                ? typeInfo.ImplementedInterfaces
                : type.GetBaseTypes();
            foreach (var baseType in baseTypes)
            {
                if (baseType.IsGenericType
                    && baseType.GetGenericTypeDefinition() == interfaceOrBaseType)
                {
                    yield return baseType;
                }
            }

            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == interfaceOrBaseType)
            {
                yield return type;
            }
        }
    }

    // source https://github.com/dotnet/efcore
    public static IEnumerable<Type> GetBaseTypes(this Type type)
    {
        var currentType = type.BaseType;

        while (currentType != null)
        {
            yield return currentType;

            currentType = currentType.BaseType;
        }
    }


    internal static void SetKeyValue(this Type type, object entity, object id)
    {
        var keyAttribute = type.GetMemberInfoOfKeyAttribute();

        if (keyAttribute == null)
        {
            throw new MappingException($"Could not find key attribute on type: {type.Name}");
        }

        keyAttribute.SetValue(entity, id);
    }

    // https://stackoverflow.com/a/33446914
    internal static object? GetValue(this MemberInfo member, object entity)
    {
        return member.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)member).GetValue(entity),
            MemberTypes.Property => ((PropertyInfo)member).GetValue(entity),
            _ => throw new NotImplementedException(),
        };
    }

    internal static void SetValue(this MemberInfo member, object entity, object value)
    {
        switch (member.MemberType)
        {
            case MemberTypes.Field:
                ((FieldInfo)member).SetValue(entity, value);
                break;
            case MemberTypes.Property:
                ((PropertyInfo)member).SetValue(entity, value);
                break;
            default:
                throw new NotImplementedException();
        };
    }

    // source https://github.com/dotnet/efcore
    public static bool IsNumeric(this Type type)
    {
        type = type.UnwrapNullableType();

        return type.IsInteger()
            || type == typeof(decimal)
            || type == typeof(float)
            || type == typeof(double);
    }

    // source https://github.com/dotnet/efcore
    public static bool IsInteger(this Type type)
    {
        type = type.UnwrapNullableType();

        return type == typeof(int)
            || type == typeof(long)
            || type == typeof(short)
            || type == typeof(byte)
            || type == typeof(uint)
            || type == typeof(ulong)
            || type == typeof(ushort)
            || type == typeof(sbyte)
            || type == typeof(char);
    }

    // source https://github.com/dotnet/efcore
    internal static Type UnwrapNullableType(this Type type)
        => Nullable.GetUnderlyingType(type) ?? type;

    internal static MethodInfo GetRequiredRuntimeMethod(this Type type, string name, params Type[] parameters)
            => type.GetTypeInfo().GetRuntimeMethod(name, parameters)
                ?? throw new InvalidOperationException($"Could not find method '{name}' on type '{type}'");
}
