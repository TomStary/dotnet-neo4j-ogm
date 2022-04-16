using System.ComponentModel.DataAnnotations;
using Neo4j.OGM.Annotations;
using static Neo4j.OGM.Annotations.RelationshipAttribute;

namespace Neo4j.OGM.Tests.TestModels;

[Node(nameof(Post))]
public class Post
{
    [Key]
    public long? Id { get; set; }
    public string Title { get; set; } = string.Empty;

    [Relationship("AUTHORS", DirectionEnum.Incoming)]
    public Person Author { get; set; } = null!;
}
