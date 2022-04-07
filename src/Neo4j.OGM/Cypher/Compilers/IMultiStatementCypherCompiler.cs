using Neo4j.OGM.Requests;

namespace Neo4j.OGM.Cypher.Compilers;

public interface IMultiStatementCypherCompiler
{
    CompilerContext Context { get; }
    NodeBuilder CreateNode(long id);
    IEnumerable<IStatement> CreateNodesStatements();
    NodeBuilder ExistingNode(long id);
    bool HasStatementDependentOnNewNode();
    void UseStatementFactory(IStatementFactory statementFactory);
    RelationshipBuilder NewRelationship(string type);
    RelationshipBuilder ExistingRelationship(long relId, string type);
    RelationshipBuilder NewRelationship(string type, bool mapBothDirections);
    IEnumerable<IStatement> GetAllStatements();
}
