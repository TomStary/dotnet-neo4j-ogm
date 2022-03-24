using System.Linq.Expressions;

namespace Neo4j.OGM.Query;

internal class MatchExpression : Expression
{
    public virtual Expression QueryExpression { get; }
}
