using Moq;
using Neo4j.OGM.Context;
using Neo4j.OGM.Exceptions;
using Neo4j.OGM.Metadata;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests.Context;

public class MappingContextTest
{
    [Fact]
    public void NativeIdTestNewEntity()
    {
        var metadataMock = new Mock<MetaData>();

        var mappingContext = new MappingContext(metadataMock.Object);

        var entity = new SimplePerson();

        mappingContext.NativeId(entity);

        Assert.NotNull(entity.Id);
        Assert.InRange(entity.Id!.Value, long.MinValue, -1);
    }

    [Fact]
    public void NativeIdTestExistingEntity()
    {
        var metadataMock = new Mock<MetaData>();

        var mappingContext = new MappingContext(metadataMock.Object);

        var entity = new SimplePerson
        {
            Id = 1
        };

        mappingContext.NativeId(entity);
        Assert.NotNull(entity.Id);
        Assert.Equal(1, entity.Id);
    }

    [Fact]
    public void NativeIdTestMissingKeyEntity()
    {
        var metadataMock = new Mock<MetaData>();

        var mappingContext = new MappingContext(metadataMock.Object);

        var entity = new MissingEndNodeAttributeException();

        Assert.Throws<MappingException>(() => mappingContext.NativeId(entity));
    }

    [Fact]
    public void GetRelationshipsTestOk()
    {
        var metadataMock = new Mock<MetaData>();

        var mappingContext = new MappingContext(metadataMock.Object);

        var relationships = mappingContext.GetRelationships();

        Assert.NotNull(relationships);
        Assert.IsAssignableFrom<IEnumerable<MappedRelationship>>(relationships);
    }

    [Fact]
    public void ContainsRelationshipsTestOk()
    {
        var metadataMock = new Mock<MetaData>();

        var mappingContext = new MappingContext(metadataMock.Object);

        var containsRelationship = mappingContext.ContainsRelationship(
            new MappedRelationship(null, string.Empty, null, null, typeof(SimplePerson), typeof(SimplePerson)));

        Assert.False(containsRelationship);
    }
}
