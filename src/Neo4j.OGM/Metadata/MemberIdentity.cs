using System.Diagnostics.Tracing;
using System.Reflection;

namespace Neo4j.OGM.Metadata;

public readonly struct MemberIdentity : IEquatable<MemberIdentity>
{
    private readonly object? _nameOrMember;
    public static readonly MemberIdentity None = new((object?)null);

    public MemberIdentity(object? nameOrMember)
    {
        _nameOrMember = nameOrMember;
    }

    public bool Equals(MemberIdentity other)
    {
        throw new NotImplementedException();
    }

    internal static MemberIdentity Create(MemberInfo member) => member == null ? None : new MemberIdentity(member);
}
