using System.Linq.Expressions;
using System.Reflection;
using Neo4j.OGM.Internals.Extensions;
using Neo4j.OGM.Metadata;

namespace Neo4j.OGM.Queries;

internal class EntityProjectionExpression : Expression
{
    private readonly Dictionary<PropertyInfo, IAccessExpression> _propertyExpressionsMap = new();

    public EntityProjectionExpression(Type entityType, Expression accessExpression)
    {
        EntityType = entityType;
        AccessExpression = accessExpression;
        Name = (accessExpression as IAccessExpression)?.Name;
    }

    public Type EntityType { get; }
    public Expression AccessExpression { get; }
    public string? Name { get; }

    public sealed override ExpressionType NodeType
            => ExpressionType.Extension;
    public override Type Type
        => EntityType;

    internal Expression BindMember(string name, Type type, bool clientEval, out PropertyInfo _)
        => BindMember(MemberIdentity.Create(name), type, clientEval, out _);

    internal Expression BindMember(MemberInfo memberInfo, Type type, bool clientEval, out PropertyInfo _)
        => BindMember(MemberIdentity.Create(memberInfo), type, clientEval, out _);


    private Expression BindMember(MemberIdentity member, Type type, bool clientEval, out PropertyInfo propertyBase)
    {
        var entityType = EntityType;

        var property = member.MemberInfo == null
            ? entityType.FindProperty(member.Name)
            : entityType.FindProperty(member.MemberInfo);
        if (property != null)
        {
            propertyBase = property;
            return BindProperty(property, clientEval);
        }

        // Entity member not found
        propertyBase = null;
        return null;
    }

    public virtual Expression BindProperty(PropertyInfo property, bool clientEval)
    {
        if (!EntityType.IsAssignableFrom(property.DeclaringType)
            && !property.DeclaringType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException();
        }

        if (!_propertyExpressionsMap.TryGetValue(property, out var expression))
        {
            expression = new KeyAccessExpression(property, AccessExpression);
            _propertyExpressionsMap[property] = expression;
        }

        return (Expression)expression;
    }
}
