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

    internal Expression Update(Expression expression, Expression shaperExpression)
    {
        throw new NotImplementedException();
    }

    public Expression QueryExpression { get; }
    public Expression ShaperExpression { get; }
    public ResultCardinality ResultCardinality { get; }

    public override Type Type
            => ResultCardinality == ResultCardinality.Enumerable
                ? typeof(IQueryable<>).MakeGenericType(ShaperExpression.Type)
                : ShaperExpression.Type;

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
