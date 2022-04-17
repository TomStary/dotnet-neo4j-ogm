using System.Linq.Expressions;
using Neo4j.Driver;
using Neo4j.OGM.Extensions.Internals;
using Neo4j.OGM.Queries.CypherExpressions;

namespace Neo4j.OGM.Queries;

internal class QueryableMethodTranslationExpressionVisitor : ExpressionVisitor
{
    private readonly CypherTranslatingExpressionVisitor _cypherTranslator;
    // private readonly SharedTypeEntityExpandingExpressionVisitor _sharedTypeEntityExpandingExpressionVisitor;
    private readonly QueryCompilationContext _context;
    private readonly CypherExpressionFactory _cypherExpressionFactory;

    public QueryableMethodTranslationExpressionVisitor(QueryCompilationContext context)
    {
        // _sharedTypeEntityExpandingExpressionVisitor = new SharedTypeEntityExpandingExpressionVisitor();
        _context = context;
        _cypherExpressionFactory = new CypherExpressionFactory();
        _cypherTranslator = new CypherTranslatingExpressionVisitor(_cypherExpressionFactory);
    }

    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression == null)
        {
            throw new ArgumentNullException("extensionExpression");
        }

        return extensionExpression switch
        {
            ShapedQueryExpression _ => extensionExpression,
            QueryRootExpression queryRootExpression => CreateShapedQueryExpression(queryRootExpression.EntityType),
            _ => base.VisitExtension(extensionExpression),
        };
    }

    private Expression CreateShapedQueryExpression(Type entityType)
    {
        var matchExpression = _cypherExpressionFactory.Match(entityType);
        return new ShapedQueryExpression(matchExpression, new EntityShaperExpression(
                    entityType,
                    new ProjectionBindingExpression(
                        matchExpression,
                        new ProjectionMember(),
                        typeof(IRecord)),
                    false));
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        ShapedQueryExpression CheckTranslated(ShapedQueryExpression? translated)
        {
            return translated
                ?? throw new InvalidOperationException("Translated expression was null");
        }

        var method = node.Method;

        if (node.Method.DeclaringType == typeof(Queryable))
        {
            var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
            var source = Visit(node.Arguments[0]);
            if (source is ShapedQueryExpression shapedQueryExpression)
            {
                switch (node.Method.Name)
                {
                    case nameof(Queryable.FirstOrDefault)
                        when genericMethod == QueryableMethods.FirstOrDefaultWithPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                        return CheckTranslated(TranslateFirstOrDefault(shapedQueryExpression, GetLambdaExpressionFromArgument(1), node.Type, true));

                    case nameof(Queryable.Where)
                        when genericMethod == QueryableMethods.Where:
                        return CheckTranslated(TranslateWhere(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        LambdaExpression GetLambdaExpressionFromArgument(int argumentIndex)
                                    => node.Arguments[argumentIndex].UnwrapLambdaFromQuote();
                }
            }
        }

        return base.VisitMethodCall(node);
    }

    private ShapedQueryExpression? TranslateFirstOrDefault(ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
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

        var matchExpression = (MatchExpression)source.QueryExpression;
        matchExpression.ApplyLimit(TranslateExpression(Expression.Constant(1)));
        matchExpression.ApplyRelationshipPredicates(Array.Empty<CypherExpression>());
        return source.ShaperExpression.Type != returnType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType))
            : source;
    }

    private ShapedQueryExpression? TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
    {
        var translation = TranslateLambdaExpression(source, predicate);
        if (translation == null)
        {
            return null;
        }

        ((MatchExpression)source.QueryExpression).ApplyPredicate(translation);

        return source;
    }

    private CypherExpression? TranslateLambdaExpression(ShapedQueryExpression source, LambdaExpression predicate)
    {
        var lambdaBody = TranslateExpression(RemapLambdaBody(source, predicate));
        return lambdaBody;
    }

    private CypherExpression? TranslateExpression(Expression expression)
    {
        return _cypherTranslator.Translate(expression);
    }

    private Expression RemapLambdaBody(ShapedQueryExpression expression, LambdaExpression lambdaExpression)
    {
        return ReplacingExpressionVisitor.Replace(
            lambdaExpression.Parameters.Single(), expression.ShaperExpression, lambdaExpression.Body)!;
    }
}
