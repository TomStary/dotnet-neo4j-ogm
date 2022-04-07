using System.ComponentModel.DataAnnotations;
using Neo4j.OGM.Annotations;

namespace Neo4j.OGM.Tests.TestModels;

[Node(nameof(Person))]
public class SimplePerson
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
