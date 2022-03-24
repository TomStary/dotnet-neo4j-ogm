using Neo4j.OGM.Cypher.Compilers;
using Neo4j.OGM.Internals;

namespace Neo4j.OGM.Requests;

public class RequestExecutor
{
    private readonly Session _session;

    public RequestExecutor(Session session)
    {
        _session = session;
    }

    internal async Task ExecuteSave(ICompilerContext context)
    {
        var compiler = context.Compiler;
        compiler.UseStatementFactory(new RowStatementFactory());

        if (compiler.HasStatementDependentOnNewNode())
        {
            await ExecuteStatementsAsync(context, compiler.CreateNodesStatements());
        }
        else
        {
            await ExecuteStatementsAsync(context, compiler.GetAllStatements());
        }
    }

    private async Task ExecuteStatementsAsync(ICompilerContext context, IEnumerable<IStatement> statements)
    {
        if (statements.Count() > 0)
        {
            var defualtRequest = new DefaulterRequest(statements);
            try
            {
                var response = await _session.ExecuteAsync(defualtRequest);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
