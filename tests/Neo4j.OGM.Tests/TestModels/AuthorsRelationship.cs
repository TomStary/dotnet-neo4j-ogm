using System.ComponentModel.DataAnnotations;
using Neo4j.OGM.Annotations;

namespace Neo4j.OGM.Tests.TestModels;

[RelationshipEntity("AUTHORS")]
public class AuthorsRelationship
{
    [Key]
    public long? Id { get; set; }

    [StartNode]
    public Author Author { get; set; } = null!;

    [EndNode]
    public Note Note { get; set; } = null!;
}
