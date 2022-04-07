using System.ComponentModel.DataAnnotations;
using Neo4j.OGM.Annotations;

namespace Neo4j.OGM.Tests.TestModels;

[Node]
public class Note
{
    [Key]
    public long? Id { get; set; }

    public string Title { get; set; } = string.Empty;
}
