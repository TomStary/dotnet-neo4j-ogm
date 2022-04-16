using System.Text;
using Neo4j.OGM.Requests;
using Neo4j.OGM.Response.Model;

namespace Neo4j.OGM.Cypher.Compilers.Statements;

public class NewRelationshipStatementBuilder
{
    private List<RelationshipModel> _relationships;
    private IStatementFactory _statementFactory;

    public NewRelationshipStatementBuilder(List<RelationshipModel> relationships, IStatementFactory statementFactory)
    {
        _relationships = relationships;
        _statementFactory = statementFactory;
    }

    public IStatement Build()
    {
        var parameters = new Dictionary<string, object?>();
        StringBuilder stringBuilder = new();

        bool hasProps = false;
        if (_relationships.Count > 0)
        {
            var fisrtRel = _relationships.First();
            var relType = fisrtRel.Type;
            var hasProperty = fisrtRel.Properties.Count > 0;

            stringBuilder.Append("UNWIND $rows AS row ")
                .Append("Match (startNode) WHERE ID(startNode) = row.startNodeId WITH row,startNode ")
                .Append(" Match (endNode) WHERE ID(endNode) = row.endNodeId ");

            if (hasProperty)
            {
                stringBuilder.Append("CRETE ");
            }
            else
            {
                stringBuilder.Append("MERGE ");
            }

            stringBuilder.Append("(startNode)-[rel:`")
                .Append(relType)
                .Append("`]->(endNode) ");

            if (hasProperty)
            {
                stringBuilder.Append("SET rel += row.properties ");
            }
            stringBuilder.Append("RETURN row.relRef as ref, ID(rel) as id, $type as type");

            var rows = new List<object>();
            foreach (var relationship in _relationships)
            {
                var rowMap = new Dictionary<string, object?>();
                rowMap.Add("startNodeId", relationship.StartNodeId);
                rowMap.Add("endNodeId", relationship.EndNodeId);
                rowMap.Add("relRef", relationship.Id);
                if (hasProperty)
                {
                    var props = new Dictionary<string, object?>();
                    foreach (var property in relationship.Properties)
                    {
                        props.Add(property.Key, property.Value);
                    }
                    rowMap.Add("props", props);
                }
                else
                {
                    rowMap.Add("props", Array.Empty<object>());
                }
                rows.Add(rowMap);
            }
            parameters.Add("type", "rel");
            parameters.Add("rows", rows);
        }

        return _statementFactory.CreateStatement(stringBuilder.ToString(), parameters);
    }
}
