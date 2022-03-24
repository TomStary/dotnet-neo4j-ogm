using System.Linq.Expressions;
using System.Reflection;

namespace Neo4j.OGM.Query;

/// <summary>
/// Reflection metadata for translatable LINQ methods.
/// This code is borrowed from Entity Framework Core: https://github.com/dotnet/efcore.
/// /// </summary>
public static class QueryableMethods
{
    public static MethodInfo FirstOrDefaultWithPredicate { get; }

    static QueryableMethods()
    {
        var queryableMethodGroups = typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .GroupBy(mi => mi.Name)
            .ToDictionary(e => e.Key, l => l.ToList());

        FirstOrDefaultWithPredicate = GetMethod(
            nameof(Queryable.FirstOrDefault), 1,
            types => new[]
            {
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
            });

        MethodInfo GetMethod(string name, int genericParameterCount, Func<Type[], Type[]> parameterGenerator)
            => queryableMethodGroups[name].Single(
                mi => ((genericParameterCount == 0 && !mi.IsGenericMethod)
                        || (mi.IsGenericMethod && mi.GetGenericArguments().Length == genericParameterCount))
                    && mi.GetParameters().Select(e => e.ParameterType).SequenceEqual(
                        parameterGenerator(mi.IsGenericMethod ? mi.GetGenericArguments() : Array.Empty<Type>())));
    }
}
