using System.Reflection;

namespace Neo4j.OGM.Utils;

internal static class EnumerableMethods
{
    public static MethodInfo SingleWithoutPredicate { get; }
    public static MethodInfo SingleOrDefaultWithoutPredicate { get; }

    static EnumerableMethods()
    {
        var queryableMethodGroups = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .GroupBy(mi => mi.Name)
                .ToDictionary(e => e.Key, l => l.ToList());

        SingleWithoutPredicate = GetMethod(
                    nameof(Enumerable.Single), 1, types => new[] { typeof(IEnumerable<>).MakeGenericType(types[0]) });

        SingleOrDefaultWithoutPredicate = GetMethod(
                nameof(Enumerable.SingleOrDefault), 1,
                types => new[] { typeof(IEnumerable<>).MakeGenericType(types[0]) });

        MethodInfo GetMethod(string name, int genericParameterCount, Func<Type[], Type[]> parameterGenerator)
                => queryableMethodGroups[name].Single(
                    mi => ((genericParameterCount == 0 && !mi.IsGenericMethod)
                            || (mi.IsGenericMethod && mi.GetGenericArguments().Length == genericParameterCount))
                        && mi.GetParameters().Select(e => e.ParameterType).SequenceEqual(
                            parameterGenerator(mi.IsGenericMethod ? mi.GetGenericArguments() : Array.Empty<Type>())));
    }

}
