using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Neo4j.OGM.Extensions.Internals;
using Neo4j.OGM.Internals.Extensions;
using Neo4j.OGM.Metadata;
using Neo4j.OGM.Queries.CypherExpressions;
using Neo4j.OGM.Queries.Internal;
using Neo4j.OGM.Utils;

namespace Neo4j.OGM.Queries;

internal class CypherTranslatingExpressionVisitor : ExpressionVisitor
{
    private static readonly List<MethodInfo> SingleResultMethodInfos = new()
    {
        QueryableMethods.FirstOrDefaultWithPredicate,
    };

    private readonly CypherExpressionFactory _cypherExpressionFactory;
    private readonly MethodCallTranslator _methodCallTranslator;

    public CypherTranslatingExpressionVisitor(CypherExpressionFactory cypherExpressionFactory)
    {
        _cypherExpressionFactory = cypherExpressionFactory;
        _methodCallTranslator = new MethodCallTranslator(_cypherExpressionFactory);
    }

    internal CypherExpression? Translate(Expression expression)
    {
        var result = Visit(expression);

        if (result is CypherExpression translation)
        {
            translation = ApplyDefaultMapping(translation);

            // if (translation?.TypeMapping == null)
            // {
            //     return null;
            // }

            return translation;
        }

        return null;
    }

    protected override Expression? VisitBinary(BinaryExpression binaryExpression)
    {
        if (binaryExpression.NodeType == ExpressionType.Coalesce)
        {
            var ifTrue = binaryExpression.Left;
            var ifFalse = binaryExpression.Right;
            if (ifTrue.Type != ifFalse.Type)
            {
                ifFalse = Expression.Convert(ifFalse, ifTrue.Type);
            }

            return Visit(
                Expression.Condition(
                    Expression.NotEqual(ifTrue, Expression.Constant(null, ifTrue.Type)),
                    ifTrue,
                    ifFalse));
        }

        var left = TryRemoveImplicitConvert(binaryExpression.Left);
        var right = TryRemoveImplicitConvert(binaryExpression.Right);

        // Remove convert-to-object nodes if both sides have them, or if the other side is null constant
        var isLeftConvertToObject = TryUnwrapConvertToObject(left, out var leftOperand);
        var isRightConvertToObject = TryUnwrapConvertToObject(right, out var rightOperand);
        if (isLeftConvertToObject && isRightConvertToObject)
        {
            left = leftOperand;
            right = rightOperand;
        }
        else if (isLeftConvertToObject && right.IsNullConstantExpression())
        {
            left = leftOperand;
        }
        else if (isRightConvertToObject && left.IsNullConstantExpression())
        {
            right = rightOperand;
        }

        var visitedLeft = Visit(left);
        var visitedRight = Visit(right);

        if ((binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
            // Visited expression could be null, We need to pass MemberInitExpression
            && TryRewriteEntityEquality(
                binaryExpression.NodeType, visitedLeft ?? left, visitedRight ?? right, out var result))
        {
            return result;
        }

        var uncheckedNodeTypeVariant = binaryExpression.NodeType switch
        {
            ExpressionType.AddChecked => ExpressionType.Add,
            ExpressionType.SubtractChecked => ExpressionType.Subtract,
            ExpressionType.MultiplyChecked => ExpressionType.Multiply,
            _ => binaryExpression.NodeType
        };

        return TranslationFailed(binaryExpression.Left, visitedLeft, out var cypherLeft)
            || TranslationFailed(binaryExpression.Right, visitedRight, out var cypherRight)
                ? null
                : _cypherExpressionFactory.MakeBinary(
                    uncheckedNodeTypeVariant,
                    cypherLeft,
                    cypherRight,
                    null);

        static bool TryUnwrapConvertToObject(Expression expression, out Expression operand)
        {
            if (expression is UnaryExpression convertExpression
                && (convertExpression.NodeType == ExpressionType.Convert
                    || convertExpression.NodeType == ExpressionType.ConvertChecked)
                && expression.Type == typeof(object))
            {
                operand = convertExpression.Operand;
                return true;
            }

            operand = null;
            return false;
        }
    }

    protected override Expression VisitConstant(ConstantExpression constantExpression)
            => new CypherConstantExpression(constantExpression, null);

    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression == null)
        {
            throw new ArgumentNullException(nameof(extensionExpression));
        }

        return extensionExpression switch
        {
            CypherExpression _ => extensionExpression,
            EntityProjectionExpression _ => extensionExpression,
            EntityShaperExpression entityShaperExpression => EntityShaperVisit(entityShaperExpression),
            ProjectionBindingExpression projectionBindingExpression => ProjectionBindingVisit(projectionBindingExpression),
            _ => throw new InvalidOperationException("Unsupported operation."),
        };
    }

    private Expression ProjectionBindingVisit(ProjectionBindingExpression projectionBindingExpression)
    {
        return projectionBindingExpression.ProjectionMember != null
                        ? ((MatchExpression)projectionBindingExpression.QueryExpression)
                        .GetMappedProjection()
                        : null;
    }

    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        if (methodCallExpression == null)
        {
            throw new ArgumentNullException(nameof(methodCallExpression));
        }

        if (methodCallExpression.TryGetPropertyArguments(out var source, out var propertyName))
        {
            return TryBindMember(Visit(source), MemberIdentity.Create(propertyName));
        }

        CypherExpression cypherObject = null;
        CypherExpression[] arguments;
        var method = methodCallExpression.Method;

        if (method.Name == nameof(object.Equals)
            && methodCallExpression.Object != null
            && methodCallExpression.Arguments.Count == 1)
        {
            var left = Visit(methodCallExpression.Object);
            var right = Visit(RemoveObjectConvert(methodCallExpression.Arguments[0]));

            if (TryRewriteEntityEquality(
                ExpressionType.Equal,
                left ?? methodCallExpression.Object,
                right ?? methodCallExpression.Arguments[0],
                out var result))
            {
                return result;
            }

            if (left is CypherExpression leftCypher
                && right is CypherExpression rightCypher)
            {
                cypherObject = leftCypher;
                arguments = new CypherExpression[1] { rightCypher };
            }
            else
            {
                return null;
            }
        }
        else
        {
            if (TranslationFailed(methodCallExpression.Object, Visit(methodCallExpression.Object), out cypherObject))
            {
                return null;
            }

            arguments = new CypherExpression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = methodCallExpression.Arguments[i];
                if (TranslationFailed(argument, Visit(argument), out var sqlArgument))
                {
                    return null;
                }

                arguments[i] = sqlArgument;
            }
        }

        var translation = _methodCallTranslator.Translate(cypherObject, methodCallExpression.Method, arguments);

        return translation;

        static Expression RemoveObjectConvert(Expression expression)
            => expression is UnaryExpression unaryExpression
                && (unaryExpression.NodeType == ExpressionType.Convert || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                && unaryExpression.Type == typeof(object)
                    ? unaryExpression.Operand
                    : expression;
    }

    private Expression TryBindMember(Expression source, MemberIdentity member)
    {
        if (!(source is EntityReferenceExpression entityReferenceExpression))
        {
            return null;
        }

        var result = member.MemberInfo != null
                ? entityReferenceExpression.ParameterEntity.BindMember(
                    member.MemberInfo, entityReferenceExpression.Type, clientEval: false, out _)
                : entityReferenceExpression.ParameterEntity.BindMember(
                    member.Name, entityReferenceExpression.Type, clientEval: false, out _);

        return result switch
        {
            EntityProjectionExpression entityProjectionExpression => new EntityReferenceExpression(entityProjectionExpression),
            _ => result
        };
    }

    private static Expression TryRemoveImplicitConvert(Expression expression)
    {
        if (expression is UnaryExpression unaryExpression
            && (unaryExpression.NodeType == ExpressionType.Convert
                || unaryExpression.NodeType == ExpressionType.ConvertChecked))
        {
            var innerType = unaryExpression.Operand.Type.UnwrapNullableType();
            if (innerType.IsEnum)
            {
                innerType = Enum.GetUnderlyingType(innerType);
            }

            var convertedType = unaryExpression.Type.UnwrapNullableType();

            if (innerType == convertedType
                || (convertedType == typeof(int)
                    && (innerType == typeof(byte)
                        || innerType == typeof(sbyte)
                        || innerType == typeof(char)
                        || innerType == typeof(short)
                        || innerType == typeof(ushort)))
                || (convertedType == typeof(double)
                    && (innerType == typeof(float))))
            {
                return TryRemoveImplicitConvert(unaryExpression.Operand);
            }
        }

        return expression;
    }

    private bool TranslationFailed(Expression? original, Expression translation, out CypherExpression castTranslation)
    {
        if (original != null
            && !(translation is CypherExpression))
        {
            castTranslation = null;
            return true;
        }

        castTranslation = translation as CypherExpression;
        return false;
    }

    private bool TryRewriteEntityEquality(
    ExpressionType nodeType,
    Expression left,
    Expression right,
    [NotNullWhen(true)] out Expression? result)
    {
        var leftEntityReference = left as EntityReferenceExpression;
        var rightEntityReference = right as EntityReferenceExpression;

        if (leftEntityReference == null
            && rightEntityReference == null)
        {
            result = null;
            return false;
        }

        var leftEntityType = leftEntityReference?.EntityType;
        var rightEntityType = rightEntityReference?.EntityType;
        var entityType = leftEntityType ?? rightEntityType;

        if (entityType == null)
        {
            throw new NullReferenceException();
        }

        var primaryKeyProperty = (PropertyInfo?)entityType.GetMemberInfoOfKeyAttribute();

        if (primaryKeyProperty == null)
        {
            result = null;
            return false;
        }

        result = Visit(
            Expression.MakeBinary(
                nodeType,
                left,
                right));

        return true;
    }

    private static bool IsNullSqlConstantExpression(Expression expression)
            => expression is CypherConstantExpression cypherConstant && cypherConstant.Value == null;

    private Expression EntityShaperVisit(EntityShaperExpression entityShaperExpression)
    {
        var result = Visit(entityShaperExpression.ValueBufferExpression);

        if (result.NodeType == ExpressionType.Convert
                        && result.Type == typeof(ValueBuffer)
                        && result is UnaryExpression outerUnary
                        && outerUnary.Operand.NodeType == ExpressionType.Convert
                        && outerUnary.Operand.Type == typeof(object))
        {
            result = ((UnaryExpression)outerUnary.Operand).Operand;
        }

        if (result is EntityProjectionExpression entityProjectionExpression)
        {
            return new EntityReferenceExpression(entityProjectionExpression);
        }

        return null;
    }

    private CypherExpression? ApplyDefaultMapping(CypherExpression? cypherExpression)
        => cypherExpression == null
            || cypherExpression.TypeMapping != null
            ? cypherExpression
            : ApplyTypeMapping(cypherExpression);

    private CypherExpression? ApplyTypeMapping(CypherExpression cypherExpression)
    {
        return cypherExpression;
    }

    private sealed class EntityReferenceExpression : Expression
    {
        public EntityReferenceExpression(EntityProjectionExpression parameter)
        {
            ParameterEntity = parameter;
            EntityType = parameter.EntityType;
            Type = EntityType;
        }

        private EntityReferenceExpression(EntityProjectionExpression parameter, Type type)
        {
            ParameterEntity = parameter;
            EntityType = parameter.EntityType;
            Type = type;
        }

        public EntityProjectionExpression ParameterEntity { get; }
        public Type EntityType { get; }

        public override Type Type { get; }

        public override ExpressionType NodeType
            => ExpressionType.Extension;

        public Expression Convert(Type type)
        {
            return type == typeof(object) // Ignore object conversion
                || type.IsAssignableFrom(Type) // Ignore conversion to base/interface
                    ? this
                    : new EntityReferenceExpression(ParameterEntity, type);
        }
    }

    private sealed class CypherTypeMappingVerifyingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is CypherExpression sqlExpression
                && sqlExpression.TypeMapping == null)
            {
                throw new InvalidOperationException();
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
