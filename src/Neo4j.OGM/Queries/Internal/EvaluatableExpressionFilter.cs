using System.Linq.Expressions;
using System.Reflection;

namespace Neo4j.OGM.Queries.Internal;

// source: https://github.com/dotnet/efcore
public class EvaluatableExpressionFilter : IEvaluatableExpressionFilter
{
#pragma warning disable IDE1006
    private static readonly PropertyInfo DateTimeNow
    = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.Now))!;

    private static readonly PropertyInfo DateTimeUtcNow
        = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.UtcNow))!;

    private static readonly PropertyInfo DateTimeToday
        = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.Today))!;

    private static readonly PropertyInfo DateTimeOffsetNow
        = typeof(DateTimeOffset).GetTypeInfo().GetDeclaredProperty(nameof(DateTimeOffset.Now))!;

    private static readonly PropertyInfo DateTimeOffsetUtcNow
        = typeof(DateTimeOffset).GetTypeInfo().GetDeclaredProperty(nameof(DateTimeOffset.UtcNow))!;

    private static readonly MethodInfo GuidNewGuid
        = typeof(Guid).GetTypeInfo().GetDeclaredMethod(nameof(Guid.NewGuid))!;

    private static readonly MethodInfo RandomNextNoArgs
        = typeof(Random).GetRuntimeMethod(nameof(Random.Next), Type.EmptyTypes)!;

    private static readonly MethodInfo RandomNextOneArg
        = typeof(Random).GetRuntimeMethod(nameof(Random.Next), new[] { typeof(int) })!;

    private static readonly MethodInfo RandomNextTwoArgs
        = typeof(Random).GetRuntimeMethod(nameof(Random.Next), new[] { typeof(int), typeof(int) })!;
#pragma warning restore IDE1006

    public bool IsEvaluatableExpression(Expression expression)
    {
        switch (expression)
        {
            case MemberExpression memberExpression:
                var member = memberExpression.Member;
                if (Equals(member, DateTimeNow)
                    || Equals(member, DateTimeUtcNow)
                    || Equals(member, DateTimeToday)
                    || Equals(member, DateTimeOffsetNow)
                    || Equals(member, DateTimeOffsetUtcNow))
                {
                    return false;
                }
                break;
            case MethodCallExpression methodCallExpression:
                var method = methodCallExpression.Method;

                if (Equals(method, GuidNewGuid)
                    || Equals(method, RandomNextNoArgs)
                    || Equals(method, RandomNextOneArg)
                    || Equals(method, RandomNextTwoArgs))
                {
                    return false;
                }

                break;
        }

        return true;
    }
}
