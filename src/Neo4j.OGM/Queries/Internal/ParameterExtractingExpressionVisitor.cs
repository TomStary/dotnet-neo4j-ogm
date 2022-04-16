using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Neo4j.OGM.Internals.Extensions;

namespace Neo4j.OGM.Queries.Internal;

// parts of this class are sourced from: https://github.com/dotnet/efcore
public class ParameterExtractingExpressionVisitor : ExpressionVisitor
{
    private const string QueryFilterPrefix = "cypher_filter";
    private readonly IParameterValues _parameterValues;
    private readonly bool _parameterize;
    private readonly EvaluatableExpressionFindingExpressionVisitor _evaluatableExpressionFindingExpressionVisitor;
    private IDictionary<Expression, bool> _evaluatableExpressions = null!;
    private readonly Dictionary<Expression, EvaluatedValues> _evaluatedValues = new(ExpressionEqualityComparer.Instance);

    public ParameterExtractingExpressionVisitor(
        IParameterValues parameterValues,
        bool parameterize)
    {
        _parameterValues = parameterValues;
        _parameterize = parameterize;
        _evaluatableExpressionFindingExpressionVisitor
            = new EvaluatableExpressionFindingExpressionVisitor(new EvaluatableExpressionFilter(), parameterize);
    }

    public virtual Expression ExtractParameters(Expression expression, bool clearEvaluatedValues)
    {
        _evaluatableExpressions = _evaluatableExpressionFindingExpressionVisitor.Find(expression);

        try
        {
            return Visit(expression);
        }
        finally
        {
            if (clearEvaluatedValues)
            {
                _evaluatedValues.Clear();
            }
        }
    }

    [return: NotNullIfNotNull("expression")]
    public override Expression? Visit(Expression? expression)
    {
        if (expression == null)
        {
            return null;
        }

        if (_evaluatableExpressions.TryGetValue(expression, out var isEvaluatable))
        {
            return Evaluate(expression, isEvaluatable && _parameterize);
        }

        return base.Visit(expression);
    }

    private Expression Evaluate(Expression expression, bool generateParameter)
    {
        object? parameterValue;
        string? parameterName;
        if (_evaluatedValues.TryGetValue(expression, out var cachedValue))
        {
            var existingExpression = generateParameter ? cachedValue.Parameter : cachedValue.Constant;
            if (existingExpression != null)
            {
                return existingExpression;
            }

            parameterValue = cachedValue.Value;
            parameterName = cachedValue.CandidateParameterName;
        }
        else
        {
            parameterValue = GetValue(expression, out parameterName);
            cachedValue = new EvaluatedValues { CandidateParameterName = parameterName, Value = parameterValue };
            _evaluatedValues[expression] = cachedValue;
        }

        if (parameterValue is IQueryable innerQueryable)
        {
            return ExtractParameters(innerQueryable.Expression, clearEvaluatedValues: false);
        }

        if (parameterName?.StartsWith(QueryFilterPrefix, StringComparison.Ordinal) != true)
        {
            if (parameterValue is Expression innerExpression)
            {
                return ExtractParameters(innerExpression, clearEvaluatedValues: false);
            }

            if (!generateParameter)
            {
                var constantValue = GenerateConstantExpression(parameterValue, expression.Type);

                cachedValue.Constant = constantValue;

                return constantValue;
            }
        }

        parameterName ??= "p";

        if (string.Equals(QueryFilterPrefix, parameterName, StringComparison.Ordinal))
        {
            parameterName = QueryFilterPrefix + "__p";
        }

        var compilerPrefixIndex
            = parameterName.LastIndexOf(">", StringComparison.Ordinal);

        if (compilerPrefixIndex != -1)
        {
            parameterName = parameterName[(compilerPrefixIndex + 1)..];
        }

        parameterName
            = QueryCompilationContext.QueryParameterPrefix
            + parameterName
            + "_"
            + _parameterValues.ParameterValues.Count;

        _parameterValues.AddParameter(parameterName, parameterValue);

        var parameter = Expression.Parameter(expression.Type, parameterName);

        cachedValue.Parameter = parameter;

        return parameter;
    }

    private Expression GenerateConstantExpression(object? value, Type returnType)
    {
        var constantExpression = Expression.Constant(value, value?.GetType() ?? returnType);

        return constantExpression.Type != returnType
            ? Expression.Convert(constantExpression, returnType)
            : (Expression)constantExpression;
    }

    private object? GetValue(Expression? expression, out string? parameterName)
    {
        parameterName = null;

        if (expression == null)
        {
            return null;
        }

        switch (expression)
        {
            case MemberExpression memberExpression:
                var instanceValue = GetValue(memberExpression.Expression, out parameterName);
                try
                {
                    switch (memberExpression.Member)
                    {
                        case FieldInfo fieldInfo:
                            parameterName = (parameterName != null ? parameterName + "_" : "") + fieldInfo.Name;
                            return fieldInfo.GetValue(instanceValue);

                        case PropertyInfo propertyInfo:
                            parameterName = (parameterName != null ? parameterName + "_" : "") + propertyInfo.Name;
                            return propertyInfo.GetValue(instanceValue);
                    }
                }
                catch
                {
                    // ignored
                }
                break;

            case ConstantExpression constantExpression:
                return constantExpression.Value;

            case MethodCallExpression methodCallExpression:
                parameterName = methodCallExpression.Method.Name;
                break;

            case UnaryExpression unaryExpression
                when (unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                && (unaryExpression.Type.UnwrapNullableType() == unaryExpression.Operand.Type):
                return GetValue(unaryExpression.Operand, out parameterName);
        }

        try
        {
            return Expression.Lambda<Func<object>>(
                    Expression.Convert(expression, typeof(object)))
                .Compile()
                .Invoke();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Unable to extract value from expression '{expression}'",
                e);
        }
    }

    // Source: github.com/dotnet/efcore
    private sealed class EvaluatableExpressionFindingExpressionVisitor : ExpressionVisitor
    {
        private readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter;
        private readonly ISet<ParameterExpression> _allowedParameters = new HashSet<ParameterExpression>();
        private readonly bool _parameterize;

        private bool _evaluatable;
        private bool _containsClosure;
        private bool _inLambda;
        private IDictionary<Expression, bool> _evaluatableExpressions;

        public EvaluatableExpressionFindingExpressionVisitor(
            IEvaluatableExpressionFilter evaluatableExpressionFilter,
            bool parameterize)
        {
            _evaluatableExpressionFilter = evaluatableExpressionFilter;
            _parameterize = parameterize;
            // The entry method will take care of populating this field always. So accesses should be safe.
            _evaluatableExpressions = null!;
        }

        public IDictionary<Expression, bool> Find(Expression expression)
        {
            _evaluatable = true;
            _containsClosure = false;
            _inLambda = false;
            _evaluatableExpressions = new Dictionary<Expression, bool>();
            _allowedParameters.Clear();

            Visit(expression);

            return _evaluatableExpressions;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression == null)
            {
                return base.Visit(expression);
            }

            var parentEvaluatable = _evaluatable;
            var parentContainsClosure = _containsClosure;

            _evaluatable = IsEvaluatableNodeType(expression)
                // Extension point to disable funcletization
                && _evaluatableExpressionFilter.IsEvaluatableExpression(expression)
                // Don't evaluate QueryableMethods if in compiled query
                && (_parameterize || !IsQueryableMethod(expression));
            _containsClosure = false;

            base.Visit(expression);

            if (_evaluatable)
            {
                // Force parameterization when not in lambda
                _evaluatableExpressions[expression] = _containsClosure || !_inLambda;
            }

            _evaluatable = parentEvaluatable && _evaluatable;
            _containsClosure = parentContainsClosure || _containsClosure;

            return expression;
        }

        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
        {
            var oldInLambda = _inLambda;
            _inLambda = true;

            // Note: Don't skip visiting parameter here.
            // SelectMany does not use parameter in lambda but we should still block it from evaluating
            base.VisitLambda(lambdaExpression);

            _inLambda = oldInLambda;
            return lambdaExpression;
        }

        protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
        {
            Visit(memberInitExpression.Bindings, VisitMemberBinding);

            // Cannot make parameter for NewExpression if Bindings cannot be evaluated
            // but we still need to visit inside of it.
            var bindingsEvaluatable = _evaluatable;
            Visit(memberInitExpression.NewExpression);

            if (!bindingsEvaluatable)
            {
                _evaluatableExpressions.Remove(memberInitExpression.NewExpression);
            }

            return memberInitExpression;
        }

        protected override Expression VisitListInit(ListInitExpression listInitExpression)
        {
            Visit(listInitExpression.Initializers, VisitElementInit);

            // Cannot make parameter for NewExpression if Initializers cannot be evaluated
            // but we still need to visit inside of it.
            var initializersEvaluatable = _evaluatable;
            Visit(listInitExpression.NewExpression);

            if (!initializersEvaluatable)
            {
                _evaluatableExpressions.Remove(listInitExpression.NewExpression);
            }

            return listInitExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Visit(methodCallExpression.Object);
            var parameterInfos = methodCallExpression.Method.GetParameters();
            for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
            {
                if (i == 1
                    && _evaluatableExpressions.ContainsKey(methodCallExpression.Arguments[0])
                    && methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                    && methodCallExpression.Method.Name == nameof(Enumerable.Select)
                    && methodCallExpression.Arguments[1] is LambdaExpression lambdaExpression)
                {
                    // Allow evaluation Enumerable.Select operation
                    foreach (var parameter in lambdaExpression.Parameters)
                    {
                        _allowedParameters.Add(parameter);
                    }
                }

                Visit(methodCallExpression.Arguments[i]);

                if (_evaluatableExpressions.ContainsKey(methodCallExpression.Arguments[i]))
                {
                    _evaluatableExpressions[methodCallExpression.Arguments[i]] = false;
                }
            }

            return methodCallExpression;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            _containsClosure = memberExpression.Expression != null
                || !(memberExpression.Member is FieldInfo fieldInfo && fieldInfo.IsInitOnly);
            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            _evaluatable = _allowedParameters.Contains(parameterExpression);

            return base.VisitParameter(parameterExpression);
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            _evaluatable = !(constantExpression.Value is IQueryable);

#pragma warning disable RCS1096 // Use bitwise operation instead of calling 'HasFlag'.
            _containsClosure
                = (constantExpression.Type.Attributes.HasFlag(TypeAttributes.NestedPrivate)
                    && Attribute.IsDefined(constantExpression.Type, typeof(CompilerGeneratedAttribute), inherit: true)) // Closure
                || constantExpression.Type == typeof(object[]); // Find method
#pragma warning restore RCS1096 // Use bitwise operation instead of calling 'HasFlag'.

            return base.VisitConstant(constantExpression);
        }

        private static bool IsEvaluatableNodeType(Expression expression)
            => expression.NodeType != ExpressionType.Extension
                || expression.CanReduce
                && IsEvaluatableNodeType(expression.ReduceAndCheck());

        private static bool IsQueryableMethod(Expression expression)
            => expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.DeclaringType == typeof(Queryable);
    }

    private sealed class EvaluatedValues
    {
        public string? CandidateParameterName { get; init; }
        public object? Value { get; init; }
        public Expression? Constant { get; set; }
        public Expression? Parameter { get; set; }
    }
}
