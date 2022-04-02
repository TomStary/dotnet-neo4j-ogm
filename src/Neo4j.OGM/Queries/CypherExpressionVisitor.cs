using System.Linq.Expressions;
using System.Text;
using Neo4j.OGM.Internals.Extensions;
using Neo4j.OGM.Requests;

namespace Neo4j.OGM.Queries;

internal class CypherExpressionVisitor : ExpressionVisitor
{
    private readonly StringBuilder _cypherBuilder = new();

    private readonly IDictionary<ExpressionType, string> _operatorMap = new Dictionary<ExpressionType, string>
        {
            // Logical
            { ExpressionType.AndAlso, " AND " },
            { ExpressionType.OrElse, " OR " },

            // Comparison
            { ExpressionType.Equal, " = " },
            { ExpressionType.NotEqual, " != " },
            { ExpressionType.GreaterThan, " > " },
            { ExpressionType.GreaterThanOrEqual, " >= " },
            { ExpressionType.LessThan, " < " },
            { ExpressionType.LessThanOrEqual, " <= " },
        };

    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case ShapedQueryExpression shapedQueryExpression:
                return shapedQueryExpression.Update(
                    Visit(shapedQueryExpression.QueryExpression), shapedQueryExpression.ShaperExpression);

            case MatchExpression matchExpression:
                return VisitMatch(matchExpression);

            case EntityProjectionExpression entityProjectionExpression:
                return VisitEntityProjection(entityProjectionExpression);

            case ReturnExpression returnExpression:
                return VisitReturn(returnExpression);

            case KeyAccessExpression keyAccessExpression:
                return VisitKeyAccess(keyAccessExpression);

            case CypherBinaryExpression cypherBinaryExpression:
                return VisitCypherBinary(cypherBinaryExpression);

            case CypherConstantExpression cypherConstantExpression:
                return VisitCypherConstant(cypherConstantExpression);

            case CypherUnaryExpression cypherUnaryExpression:
                return VisitCypherUnary(cypherUnaryExpression);

            case CypherParameterExpression cypherParameterExpression:
                return VisitCypherParameter(cypherParameterExpression);
        }

        return base.VisitExtension(extensionExpression);
    }

    internal IStatement GenerateCypherQuery(MatchExpression matchExpression)
    {
        Visit(matchExpression);
        return new QueryStatement(_cypherBuilder.ToString(), new Dictionary<string, object?>());
    }

    protected Expression VisitMatch(MatchExpression matchExpression)
    {
        _cypherBuilder.Append("MATCH ");

        var entityName = matchExpression.EntityType.GetNeo4jName();

        if (matchExpression.EntityType.HasRelationshipEntityAttribute())
        {
            // if entity is relationship we are using different brackets
            _cypherBuilder.AppendFormat("[{0}:{1}]", matchExpression.ReturnExpression.Alias, matchExpression.EntityType.GetNeo4jName());
        }
        else
        {
            _cypherBuilder.AppendFormat("({0}:{1})", matchExpression.ReturnExpression.Alias, matchExpression.EntityType.GetNeo4jName());
        }

        if (matchExpression.Predicate != null)
        {
            _cypherBuilder.Append(" WHERE ");
            Visit(matchExpression.Predicate);
        }

        if (matchExpression.ReturnExpression != null)
        {
            _cypherBuilder.Append(" RETURN ");
            Visit(matchExpression.ReturnExpression);
        }

        if (matchExpression.Limit != null)
        {
            _cypherBuilder.Append(" LIMIT ");
            Visit(matchExpression.Limit);
        }


        return matchExpression;
    }

    protected virtual Expression VisitReturn(ReturnExpression returnCypherExpression)
    {
        _cypherBuilder.Append(returnCypherExpression.ToString());
        return returnCypherExpression;
    }

    protected Expression VisitCypherParameter(CypherParameterExpression CypherParameterExpression)
    {
        throw new NotImplementedException();
    }

    protected Expression VisitCypherUnary(CypherUnaryExpression CypherUnaryExpression)
    {
        throw new NotImplementedException();
    }

    protected Expression VisitCypherConstant(CypherConstantExpression cypherConstantExpression)
    {
        _cypherBuilder.Append(cypherConstantExpression.Value);
        return cypherConstantExpression;
    }

    protected Expression VisitCypherBinary(CypherBinaryExpression cypherBinaryExpression)
    {
        var op = _operatorMap[cypherBinaryExpression.OperatorType];

        _cypherBuilder.Append("(");
        Visit(cypherBinaryExpression.Left);
        _cypherBuilder.Append(op);
        Visit(cypherBinaryExpression.Right);
        _cypherBuilder.Append(")");

        return cypherBinaryExpression;
    }

    protected Expression VisitKeyAccess(KeyAccessExpression keyAccessExpression)
    {
        _cypherBuilder.Append(keyAccessExpression.ToString());
        return keyAccessExpression;
    }

    protected Expression VisitEntityProjection(EntityProjectionExpression entityProjectionExpression)
    {
        throw new NotImplementedException();
    }
}
