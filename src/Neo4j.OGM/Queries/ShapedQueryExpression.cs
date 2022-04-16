using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Neo4j.OGM.Queries;

internal class ShapedQueryExpression : Expression
{
    public ShapedQueryExpression(Expression queryExpression, Expression shaperExpression)
            : this(
                queryExpression,
                shaperExpression,
                ResultCardinality.Enumerable)
    {
    }

    private ShapedQueryExpression(
        Expression queryExpression,
        Expression shaperExpression,
        ResultCardinality resultCardinality)
    {
        QueryExpression = queryExpression;
        ShaperExpression = shaperExpression;
        ResultCardinality = resultCardinality;
    }

    internal Expression Update(Expression queryExpression, Expression shaperExpression)
    {
        return queryExpression != QueryExpression || shaperExpression != ShaperExpression
            ? new ShapedQueryExpression(queryExpression, shaperExpression, ResultCardinality)
            : this;
    }

    public Expression QueryExpression { get; }
    public Expression ShaperExpression { get; }
    public ResultCardinality ResultCardinality { get; }

    [ExcludeFromCodeCoverage]
    public override Type Type
            => ResultCardinality == ResultCardinality.Enumerable
                ? typeof(IQueryable<>).MakeGenericType(ShaperExpression.Type)
                : ShaperExpression.Type;

    [ExcludeFromCodeCoverage]
    public sealed override ExpressionType NodeType
           => ExpressionType.Extension;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    internal ShapedQueryExpression UpdateResultCardinality(ResultCardinality resultCardinality)
        => new(QueryExpression, ShaperExpression, resultCardinality);

    internal ShapedQueryExpression UpdateShaperExpression(Expression shaperExpression)
        => shaperExpression != ShaperExpression
            ? new(QueryExpression, shaperExpression, ResultCardinality)
            : this;
}
