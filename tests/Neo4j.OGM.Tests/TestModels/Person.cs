using System.ComponentModel.DataAnnotations;
using Neo4j.OGM.Annotations;
using static Neo4j.OGM.Annotations.RelationshipAttribute;

namespace Neo4j.OGM.Tests.TestModels;

[Node]
public class Person
{
    [Key]
    public long? Id { get; set; }

    public string Name { get; set; } = string.Empty;

    [Relationship("AUTHORS", DirectionEnum.Outgoing)]
    public IEnumerable<Post> Posts { get; set; } = new List<Post>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
