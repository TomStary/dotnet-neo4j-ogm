using Neo4j.Driver;

namespace Neo4j.OGM;

public static class SessionFactory
{
    public static Session CreateSession(IDriver _driver)
    {
        return new Session(_driver);
    }
}