using System.ComponentModel.DataAnnotations;
using Neo4j.OGM.Annotations;
using static Neo4j.OGM.Annotations.RelationshipAttribute;

namespace Neo4j.OGM.Tests.TestModels;

[Node]
public class Author
{
    [Key]
    public long? Id { get; set; }

    public string Name { get; set; } = string.Empty;

    [Relationship("AUTHORS", DirectionEnum.Outgoing)]
    public IEnumerable<AuthorsRelationship> AuthorsRelationships { get; set; } = new List<AuthorsRelationship>();

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
