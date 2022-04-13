using Neo4j.OGM.Context;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests.Context;

public class IdentityMapTest
{
    [Fact]
    public void AddHashTest()
    {
        var identityMap = new IdentityMap();

        var entity = new SimplePerson
        {
            Id = 1
        };

        identityMap.AddHash(entity.Id.Value, entity);
        Assert.False(identityMap.CompareHash(entity.Id, entity));
    }

    [Fact]
    public void CompareHashTest()
    {
        var identityMap = new IdentityMap();

        var entity = new SimplePerson
        {
            Id = null
        };

        // Null id should result in false
        Assert.False(identityMap.CompareHash(entity.Id, entity));

        entity = new SimplePerson
        {
            Id = 1
        };

        // Existing id that is not added into internal dictionary should result in true
        Assert.True(identityMap.CompareHash(entity.Id, entity));

        identityMap.AddHash(entity.Id.Value, entity);
        Assert.False(identityMap.CompareHash(entity.Id, entity));
    }
}
