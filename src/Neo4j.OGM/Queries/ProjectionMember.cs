using System.Diagnostics;
using System.Reflection;

namespace Neo4j.OGM.Queries;

public class ProjectionMember
{
    private readonly IList<MemberInfo> _memberChain;

    public ProjectionMember()
    {
        _memberChain = new List<MemberInfo>();
    }

    [DebuggerStepThrough]
    public override bool Equals(object? obj)
            => obj != null
                && (obj is ProjectionMember projectionMember
                    && Equals(projectionMember));

    private bool Equals(ProjectionMember other)
    {
        if (_memberChain.Count != other._memberChain.Count)
        {
            return false;
        }

        for (var i = 0; i < _memberChain.Count; i++)
        {
            if (!Equals(_memberChain[i], other._memberChain[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override string ToString()
            => _memberChain.Any()
                ? string.Join(".", _memberChain.Select(mi => mi.Name))
                : "EmptyProjectionMember";
}
