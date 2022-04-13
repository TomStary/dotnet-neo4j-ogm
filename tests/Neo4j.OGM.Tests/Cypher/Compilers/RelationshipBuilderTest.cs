using Neo4j.OGM.Cypher.Compilers;
using Neo4j.OGM.Exceptions;
using static Neo4j.OGM.Annotations.RelationshipAttribute;

namespace Neo4j.OGM.Tests.Cypher.Compilers;

public class RelationshipBuilderTest
{
    [Fact]
    public void HasDirectionTest()
    {
        var relationshipBuilder = new RelationshipBuilder("AUTHROS");

        relationshipBuilder.Direction(DirectionEnum.Outgoing);

        Assert.True(relationshipBuilder.HasDirection(DirectionEnum.Outgoing));
        Assert.False(relationshipBuilder.HasDirection(DirectionEnum.Incoming));

        relationshipBuilder.Direction(DirectionEnum.Undirected);
        Assert.False(relationshipBuilder.HasDirection(DirectionEnum.Incoming));
        Assert.False(relationshipBuilder.HasDirection(DirectionEnum.Outgoing));
        Assert.True(relationshipBuilder.HasDirection(DirectionEnum.Undirected));
    }


    [Fact]
    public void AddPropertyTest()
    {
        var relationshipBuilder = new RelationshipBuilder("AUTHROS");

        relationshipBuilder.AddProperty("name", "John");

        Assert.Throws<MappingException>(() => relationshipBuilder.AddProperty("name", "John"));
    }
}
