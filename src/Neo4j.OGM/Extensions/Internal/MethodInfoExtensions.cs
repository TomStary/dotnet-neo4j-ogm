using System.Reflection;
namespace Neo4j.OGM.Extensions.Internals;

public static class MethodInfoExtensions
{
    private static readonly string _typeName = typeof(ExpressionExtensions).FullName!;

    public static bool IsPropertyMethod(this MethodInfo? methodInfo)
                => Equals(methodInfo, ExpressionExtensions.PropertyMethod)
                    // fallback to string comparison because MethodInfo.Equals is not
                    // always true in .NET Native even if methods are the same
                    || methodInfo?.IsGenericMethod == true
                    && methodInfo.Name == nameof(ExpressionExtensions.Property)
                    && methodInfo.DeclaringType?.FullName == _typeName;


    public static string GetSimpleMemberName(this MemberInfo member)
    {
        var name = member.Name;
        var index = name.LastIndexOf('.');
        return index >= 0 ? name[(index + 1)..] : name;
    }
}
