namespace Neo4j.OGM.Response.Model;

public interface IRowModel
{
    object[] GetValues();
    string[] Variables();
}
