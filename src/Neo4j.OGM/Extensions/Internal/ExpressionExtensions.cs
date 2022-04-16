using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Neo4j.OGM.Internals.Extensions;
using Neo4j.OGM.Queries;
using Neo4j.OGM.Utils;

namespace Neo4j.OGM.Extensions.Internals;

[ExcludeFromCodeCoverage]
internal static class ExpressionExtensions
{
    private static readonly MethodInfo ObjectEqualsMethodInfo
        = typeof(object).GetRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) })!;

    internal static Expression BuildPredicate(
        PropertyInfo[] keyProperties,
        ValueBuffer keyValues,
        ParameterExpression entityParameter)
    {
        var keyValuesConnstants = Expression.Constant(keyValues);

        var predicate = GenerateEqualExpression(entityParameter, keyValuesConnstants, keyProperties[0], 0);

        for (var i = 1; i < keyProperties.Length; i++)
        {
            predicate = Expression.AndAlso(
                predicate,
                GenerateEqualExpression(entityParameter, keyValuesConnstants, keyProperties[i], i)
            );
        }

        return predicate;

        static Expression GenerateEqualExpression(
            Expression entityParameterExpression,
            Expression keyValuesConstantExpression,
            PropertyInfo property,
            int i
        ) => property.PropertyType.IsValueType
             && property.PropertyType.UnwrapNullableType() is Type nonNullableType
             && !(nonNullableType == typeof(bool) || nonNullableType.IsNumeric() || nonNullableType.IsEnum)
            ? Expression.Call(
                        ObjectEqualsMethodInfo,
                        Expression.Call(
                            ExpressionExtensions.PropertyMethod.MakeGenericMethod(typeof(object)),
                            entityParameterExpression,
                            Expression.Constant(property.Name, typeof(string))),
                        Expression.Convert(
                            Expression.Call(
                                keyValuesConstantExpression,
                                ValueBuffer.GetValueMethod,
                                Expression.Constant(i)),
                            typeof(object)))
                    : Expression.Equal(
                        Expression.Call(
                            ExpressionExtensions.PropertyMethod.MakeGenericMethod(property.PropertyType),
                            entityParameterExpression,
                            Expression.Constant(property.Name, typeof(string))),
                        Expression.Convert(
                            Expression.Call(
                                keyValuesConstantExpression,
                                ValueBuffer.GetValueMethod,
                                Expression.Constant(i)),
                            property.PropertyType));
    }

    public static bool TryGetPropertyArguments(
            this MethodCallExpression methodCallExpression,
            [NotNullWhen(true)] out Expression? entityExpression,
            [NotNullWhen(true)] out string? propertyName)
    {
        if (methodCallExpression.Method.IsPropertyMethod()
            && methodCallExpression.Arguments[1] is ConstantExpression propertyNameExpression)
        {
            entityExpression = methodCallExpression.Arguments[0];
            propertyName = (string)propertyNameExpression.Value!;
            return true;
        }

        (entityExpression, propertyName) = (null, null);
        return false;
    }

    internal static readonly MethodInfo PropertyMethod
        = typeof(ExpressionExtensions).GetTypeInfo().GetDeclaredMethod(nameof(Property))!;


    public static TProperty Property<TProperty>(
        object entity,
        string propertyName)
        => throw new InvalidOperationException("OOF");

    public static bool IsNullConstantExpression(this Expression expression)
            => RemoveConvert(expression) is ConstantExpression constantExpression
                && constantExpression.Value == null;

    internal static LambdaExpression UnwrapLambdaFromQuote(this Expression expression)
    {
        return (LambdaExpression)(expression is UnaryExpression unary && expression.NodeType == ExpressionType.Quote
                ? unary.Operand
                : expression);
    }

    private static Expression RemoveConvert(Expression expression)
    {
        if (expression is UnaryExpression unaryExpression
            && (expression.NodeType == ExpressionType.Convert
                || expression.NodeType == ExpressionType.ConvertChecked))
        {
            return RemoveConvert(unaryExpression.Operand);
        }

        return expression;
    }
}
