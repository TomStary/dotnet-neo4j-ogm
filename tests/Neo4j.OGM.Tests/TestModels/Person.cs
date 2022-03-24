using System.ComponentModel.DataAnnotations;

namespace Neo4j.OGM.Tests.TestModels;

public class Person
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
