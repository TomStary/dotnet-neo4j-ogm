using Neo4j.OGM.Annotations;

namespace Neo4j.OGM.Tests.TestModels;

[RelationshipEntity("TEST")]
internal class MissingEndNodeAttribute
{
    [StartNode]
    public Person Person { get; set; } = null!;
}
