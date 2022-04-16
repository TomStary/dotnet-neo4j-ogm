using Neo4j.OGM.Requests;

namespace Neo4j.OGM.Cypher.Compilers;

public interface IMultiStatementCypherCompiler
{
    CompilerContext Context { get; }
    NodeBuilder CreateNode(long id);
    IEnumerable<IStatement> CreateNodesStatements();
    IEnumerable<IStatement> CreateRelationshipsStatements();
    IEnumerable<IStatement> UpdateRelationshipsStatements();
    IEnumerable<IStatement> UpdateNodesStatements();
    NodeBuilder ExistingNode(long id);
    RelationshipBuilder ExistingRelationship(long relId, string type);
    IEnumerable<IStatement> GetAllStatements();
    bool HasStatementDependentOnNewNode();
    RelationshipBuilder NewRelationship(string type);
    RelationshipBuilder NewRelationship(string type, bool mapBothDirections);
    void UseStatementFactory(IStatementFactory statementFactory);
}
