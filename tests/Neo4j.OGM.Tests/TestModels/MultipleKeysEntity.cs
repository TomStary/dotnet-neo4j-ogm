using System.ComponentModel.DataAnnotations;

namespace Neo4j.OGM.Tests.TestModels;

public class MultipleKeysEntity
{
    [Key]
    public long Id { get; set; }

    [Key]
    public long SecondId { get; set; }

    [Key]
    public string Name { get; set; }
}
