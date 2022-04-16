using Neo4j.OGM.Extensions.Internals;
using Neo4j.OGM.Tests.TestModels;

namespace Neo4j.OGM.Tests.Extensions.Internal;

public class IRecordExtensionTest
{
    [Fact]
    public void MapRecordToTypeTest()
    {
        var personEntity = new PersonEntity(1, new Dictionary<string, object> { { "Name", "Test" } });
        var presonRecord = new PersonRecord(new Dictionary<string, object> { { "c", personEntity } });

        var person = presonRecord.MapRecordToType<SimplePerson>("c");

        Assert.NotNull(person);
        Assert.Equal("Test", person.Name);
        Assert.Equal(1, person.Id);
    }
}
