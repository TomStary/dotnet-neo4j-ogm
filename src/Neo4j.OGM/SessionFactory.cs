using System.Reflection;
using Neo4j.Driver;
using Neo4j.OGM.Internals;
using Neo4j.OGM.Metadata;

namespace Neo4j.OGM;

public class SessionFactory
{
    private readonly MetaData _metadata;
    private readonly IDriver _driver;

    /// <summary>
    ///
    /// </summary>
    /// <param name="assemblies"></param>
    public SessionFactory(string connectionString, params Assembly[] assemblies)
    {
        _driver = GraphDatabase.Driver(connectionString);
        _metadata = new MetaData(assemblies);
    }

    public ISession CreateSession()
    {
        return new Session(_metadata, _driver);
    }
}