namespace Neo4j.OGM.Response;

public interface IResponse<T>
{
    T Next();
}
