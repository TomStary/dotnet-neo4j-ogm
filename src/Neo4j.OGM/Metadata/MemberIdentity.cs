using System.Reflection;
using Neo4j.OGM.Extensions.Internals;

namespace Neo4j.OGM.Metadata;

public readonly struct MemberIdentity : IEquatable<MemberIdentity>
{
    private readonly object? _nameOrMember;

    public static readonly MemberIdentity None = new((object?)null);

    public MemberIdentity(object? nameOrMember)
    {
        _nameOrMember = nameOrMember;
    }

    public MemberInfo? MemberInfo
    {
        get => _nameOrMember as MemberInfo;
    }

    public string? Name
    {
        get => MemberInfo?.GetSimpleMemberName() ?? (string?)_nameOrMember;
    }

    internal static MemberIdentity Create(MemberInfo member) => member == null ? None : new MemberIdentity(member);

    public static MemberIdentity Create(string? name)
            => name == null ? None : new MemberIdentity(name);

    public bool Equals(MemberIdentity other)
    {
        if (MemberInfo != null)
        {
            return MemberInfo == other.MemberInfo;
        }

        return Name == other.Name;
    }
}
