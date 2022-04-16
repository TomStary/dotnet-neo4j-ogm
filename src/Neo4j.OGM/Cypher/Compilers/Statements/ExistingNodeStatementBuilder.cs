using System.Text;
using Neo4j.OGM.Requests;
using Neo4j.OGM.Response.Model;

namespace Neo4j.OGM.Cypher.Compilers.Statements;

public class ExistingNodeStatementBuilder
{
    private readonly List<NodeModel> _nodeModels;
    private readonly IStatementFactory _statementFactory;

    public ExistingNodeStatementBuilder(List<NodeModel> nodeModels, IStatementFactory statementFactory)
    {
        _nodeModels = nodeModels;
        _statementFactory = statementFactory;
    }

    internal IStatement Build()
    {
        var parameters = new Dictionary<string, object>();
        var queryBuilder = new StringBuilder();

        if (_nodeModels.Count > 0)
        {
            var firstNode = _nodeModels.First();

            queryBuilder.Append("UNWIND $rows as row MATCH (n) WHERE ID(n)=row.nodeId ");

            queryBuilder.Append("SET n");

            foreach (var label in firstNode.Labels)
            {
                queryBuilder.AppendFormat(":`{0}`", label);
            }

            queryBuilder.Append(" SET n += row.props RETURN row.nodeId as ref, ID(n) as id, $type as type");
            var rows = _nodeModels.Select(x => x.ToRow("nodeId")).ToList();

            parameters.Add("type", "node");
            parameters.Add("rows", rows);
        }

        return _statementFactory.CreateStatement(queryBuilder.ToString(), parameters!);
    }
}
