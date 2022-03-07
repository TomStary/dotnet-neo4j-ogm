using System.Reflection;
using Neo4j.OGM.Metadata.Schema;

namespace Neo4j.OGM.Metadata;

public class MetaData
{
    public ISchema Schema { get; init; }

    private readonly DomainInfo _domainInfo;

    /// <summary>
    /// MetaData constructor.
    /// </summary>
    /// <param name="assemblies">Assemblies containing domain model.</param>
    public MetaData(params Assembly[] assemblies)
    {
        _domainInfo = new DomainInfo(assemblies);
        Schema = new SchemaBuilder(_domainInfo).Build();
    }
}
