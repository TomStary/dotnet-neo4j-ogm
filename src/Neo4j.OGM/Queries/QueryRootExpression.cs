using System.Linq.Expressions;

namespace Neo4j.OGM.Queries;

public class QueryRootExpression : Expression
{
    public virtual IAsyncQueryProvider? QueryProvider { get; }

    public override Type Type { get; }

    public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

    public override bool CanReduce
        => false;

    public Type EntityType { get; internal set; }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
            => this;

    public QueryRootExpression(IAsyncQueryProvider asyncQueryProvider, Type entityType)
    {
        QueryProvider = asyncQueryProvider;
        Type = typeof(IQueryable<>).MakeGenericType(entityType);
        EntityType = entityType;
    }
}
