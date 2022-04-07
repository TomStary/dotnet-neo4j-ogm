using System.Reflection;
using Neo4j.OGM.Metadata.Schemas;

namespace Neo4j.OGM.Metadata;

/// <summary>
/// Metadata for a domain model, contains <see cref="ISchema"> for graph based on supplied assemblies.
/// </summary>
internal class MetaData
{
    /// <summary>
    /// Representation of graph schema obtained from the DomainInfo instance.
    /// </summary>
    public ISchema Schema { get; init; }

    /// <inheritdoc cref="Neo4j.OGM.Metadata.DomainInfo">
    private readonly DomainInfo _domainInfo;

    /// <summary>
    /// MetaData constructor.
    /// </summary>
    /// <param name="assemblies">Assemblies containing domain model.</param>
    internal MetaData(params Assembly[] assemblies)
    {
        _domainInfo = new DomainInfo(assemblies);
        Schema = new SchemaBuilder(_domainInfo).Build();
    }
}
