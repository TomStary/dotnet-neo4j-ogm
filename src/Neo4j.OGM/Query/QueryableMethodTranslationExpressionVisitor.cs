using System.Linq.Expressions;
using System.Reflection;
using Neo4j.OGM.Internals.Extensions;
using Neo4j.OGM.Metadata;
using Neo4j.OGM.Query.CypherExpressions;

namespace Neo4j.OGM.Query;

internal class QueryableMethodTranslationExpressionVisitor : ExpressionVisitor
{
    private readonly CypherTranslatingExpressionVisitor _cypherTranslator;
    private readonly SharedTypeEntityExpandingExpressionVisitor _sharedTypeEntityExpandingExpressionVisitor;

    public QueryableMethodTranslationExpressionVisitor()
    {
        _cypherTranslator = new CypherTranslatingExpressionVisitor();
        _sharedTypeEntityExpandingExpressionVisitor = new SharedTypeEntityExpandingExpressionVisitor();
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var method = node.Method;
        if (node.Method.DeclaringType == typeof(Queryable))
        {
            var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
            var source = Visit(node.Arguments[0]);
            switch (node.Method.Name)
            {
                case nameof(Queryable.FirstOrDefault)
                    when genericMethod == QueryableMethods.FirstOrDefaultWithPredicate:
                    return TranslateFirstOrDefault(source, GetLambdaExpressionFromArgument(1), node.Type, true);

                    LambdaExpression GetLambdaExpressionFromArgument(int argumentIndex)
                                => node.Arguments[argumentIndex].UnwrapLambdaFromQuote();
            }
        }
    }

    private Expression TranslateFirstOrDefault(Expression source, LambdaExpression predicate, Type returnType, bool returnDefault)
    {
        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        var matchExpression = (MatchExpression)source.QueryExpressions;
        matchExpression.ApplyLimit(TranslateExpression(Expression.Constant(1)));
        return source;
    }

    private object TranslateWhere(Expression source, LambdaExpression predicate)
    {
        var translation = TranslateLambdaExpression(source, predicate);
    }

    private CypherExpression? TranslateLambdaExpression(Expression source, LambdaExpression predicate)
        => TranslateExpression(RemapLambdaBody(source, predicate);

    private CypherExpression? TranslateExpression(Expression expression)
    {
        return _cypherTranslator.Translate(expression);
    }


    private Expression RemapLambdaBody(Expression expression, LambdaExpression lambdaExpression)
    {
        var lambdaBody = ReplacingExpressionVisitor.Replace(
            lambdaExpression.Parameters.Single(), expression, lambdaExpression.Body);

        return ExpandSharedTypeEntities((MatchExpression)expression.QueryExpression, lambdaBody);
    }

    private Expression ExpandSharedTypeEntities(MatchExpression matchExpression, Expression lambdaBody)
        => _sharedTypeEntityExpandingExpressionVisitor.Expand(matchExpression, lambdaBody);

    private sealed class SharedTypeEntityExpandingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo ObjectEqualsMethodInfo
            = typeof(object).GetRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) })!;
        private MatchExpression _matchExpression;

        public SharedTypeEntityExpandingExpressionVisitor()
        {
        }

        public Expression Expand(MatchExpression matchExpression, Expression lambdaBody)
        {
            _matchExpression = matchExpression;

            return Visit(lambdaBody);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);

            return TryExpand(innerExpression, MemberIdentity.Create(memberExpression.Member))
                ?? memberExpression.Update(innerExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitExtension(Expression extensionExpression)
            => throw new NotImplementedException();

        private Expression? TryExpand(Expression? source, MemberIdentity member)
        {
            return null;
        }

        private static Expression AddConvertToObject(Expression expression)
            => expression.Type.IsValueType
                ? Expression.Convert(expression, typeof(object))
                : expression;
    }
}
