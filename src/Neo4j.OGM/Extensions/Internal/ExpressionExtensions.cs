using System.Linq.Expressions;
using System.Reflection;
using Neo4j.OGM.Internals.Extensions;

namespace Neo4j.OGM.Extensions.Internals;

internal static class ExpressionExtensions
{
    internal static Expression BuildPredicate(
        PropertyInfo[] keyProperties,
        object[] keyValues,
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
        ) => Expression.Equal(
            Expression.Call(
                property.GetGetMethod()!,
                entityParameterExpression,
                Expression.Constant(property.Name, typeof(string))
            ),
            Expression.Convert(
                Expression.Call(
                    keyValuesConstantExpression,
                    typeof(Array).GetMethod(nameof(Array.GetValue)),
                    Expression.Constant(i)
                ), property.PropertyType
            ));
    }
}
