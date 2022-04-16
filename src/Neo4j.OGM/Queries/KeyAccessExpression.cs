using System.Linq.Expressions;
using System.Reflection;
using Neo4j.OGM.Extensions.Internals;
using Neo4j.OGM.Queries.CypherExpressions;

namespace Neo4j.OGM.Queries;

internal class KeyAccessExpression : CypherExpression, IAccessExpression
{
    public KeyAccessExpression(PropertyInfo property, Expression accessExpression)
        : base(property.PropertyType)
    {
        Property = property;
        AccessExpression = accessExpression;
        Name = property.GetSimpleMemberName();
    }

    public new PropertyInfo Property { get; }
    public Expression AccessExpression { get; }

    public string Name { get; }

    public override string ToString()
    {
        return $"ID({AccessExpression})";
    }
}

