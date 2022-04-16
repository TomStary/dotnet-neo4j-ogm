using System.Text;
using Neo4j.OGM.Requests;
using Neo4j.OGM.Response.Model;

namespace Neo4j.OGM.Cypher.Compilers.Statements;

public class ExistingRelationshipStatementBuilder
{
    private readonly List<RelationshipModel> _relationships;
    private readonly IStatementFactory _statementFactory;

    public ExistingRelationshipStatementBuilder(List<RelationshipModel> relationships, IStatementFactory statementFactory)
    {
        _relationships = relationships;
        _statementFactory = statementFactory;
    }

    internal IStatement Build()
    {
        var parameters = new Dictionary<string, object>();
        var queryBuilder = new StringBuilder();

        var firstRel = _relationships.First();
        if (_relationships.Count > 0)
        {
            queryBuilder.Append("UNWIND $rows AS row MATCH ()-[r]->() WHERE ID(r) = row.relId ");

            queryBuilder.Append("SET r += row.props ");
            queryBuilder.Append("RETURN ID(r) as ref, ID(r) as id, $type as type");

            var rows = new List<Dictionary<string, object>>();
            foreach (var rel in _relationships)
            {
                var rowMap = new Dictionary<string, object>
                {
                    { "relId", rel.Id }
                };
                var pros = new Dictionary<string, object?>();
                foreach (var property in rel.Properties)
                {
                    pros.Add(property.Key, property.Value);
                }
                rowMap.Add("props", pros);
            }

            parameters.Add("type", "relationship");
            parameters.Add("rows", rows);
        }

        return _statementFactory.CreateStatement(queryBuilder.ToString(), parameters!);
    }
}
