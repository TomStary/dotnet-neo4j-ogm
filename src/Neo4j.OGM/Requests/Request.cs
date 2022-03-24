using Neo4j.OGM.Response;
using Neo4j.OGM.Response.Model;

namespace Neo4j.OGM.Requests;

public class Request : IRequest
{
    public IResponse<IRowModel> Execute(DefaulterRequest request)
    {
        var rowModel = new List<IRowModel>();
        var columns = new List<string>();

        foreach (var statement in request.Statements)
        {
            var result = ExecuteRequest(statement);
        }
        return null;
    }

    private object ExecuteRequest(IStatement statement)
    {
        throw new NotImplementedException("Missing TransactionManager.");
    }
}
