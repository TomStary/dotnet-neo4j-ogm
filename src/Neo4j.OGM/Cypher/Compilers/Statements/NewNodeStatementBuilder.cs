using System.Text;
using Neo4j.OGM.Requests;
using Neo4j.OGM.Response.Model;

namespace Neo4j.OGM.Cypher.Compilers.Statements;

public class NewNodeStatementBuilder
{
    private readonly List<NodeModel> _newNodes;
    private readonly IStatementFactory _statementFactory;

    public NewNodeStatementBuilder(List<NodeModel> newNodes, IStatementFactory statementFactory)
    {
        _newNodes = newNodes;
        _statementFactory = statementFactory;
    }

    public IStatement Build()
    {
        var parameters = new Dictionary<string, object>();
        var queryBuilder = new StringBuilder();

        if (_newNodes.Count > 0)
        {
            var firstNode = _newNodes.First();

            queryBuilder.Append("UNWIND $rows as row ");
            queryBuilder.Append("CREATE (n");

            foreach (var label in firstNode.Labels)
            {
                queryBuilder.AppendFormat(":`{0}`", label);
            }

            queryBuilder.Append(") ");

            queryBuilder.Append("SET n=row.props RETURN row.nodeRef as ref, ID(n) as id, $type as type");
            var rows = _newNodes.Select(x => x.ToRow("nodeRef")).ToList();
            parameters.Add("type", "node");
            parameters.Add("rows", rows);
        }

        return _statementFactory.CreateStatement(queryBuilder.ToString(), parameters!);
    }
}
