using System.Reflection;
using Neo4j.OGM.Utils;

namespace Neo4j.OGM.Tests.Utils;

public class EnumerableMethodsTest
{
    [Fact]
    public void TestStaticProperties()
    {
        var singleWithoutPredicate = EnumerableMethods.SingleWithoutPredicate;
        var singleOrDefaultWithPredicate = EnumerableMethods.SingleOrDefaultWithoutPredicate;

        Assert.NotNull(singleWithoutPredicate);
        Assert.NotNull(singleOrDefaultWithPredicate);

        Assert.IsAssignableFrom<MemberInfo>(singleWithoutPredicate);
        Assert.IsAssignableFrom<MemberInfo>(singleOrDefaultWithPredicate);
    }
}
