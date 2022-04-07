using System.ComponentModel.DataAnnotations;
using Neo4j.OGM.Annotations;

namespace Neo4j.OGM.Tests.TestModels;

[Node]
public class MissingKeyEntity
{
    public string Title { get; set; } = string.Empty;
}
