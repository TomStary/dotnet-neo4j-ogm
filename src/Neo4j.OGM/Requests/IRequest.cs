using Neo4j.OGM.Response;
using Neo4j.OGM.Response.Model;

namespace Neo4j.OGM.Requests;

public interface IRequest
{
    IResponse<IRowModel> Execute(DefaulterRequest request);
}
