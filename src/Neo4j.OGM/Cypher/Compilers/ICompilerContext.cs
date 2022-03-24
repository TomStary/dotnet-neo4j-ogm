namespace Neo4j.OGM.Cypher.Compilers;

public interface ICompilerContext
{
    MultiStatementCypherCompiler Compiler { get; }
    void Register(object entity);
    void RegisterNewObject(long id, object entity);
    void Unregister(object entity);
    bool Visited(object entity, int horizon);
    NodeBuilder? VisitedNode(object entity);
    bool VisitedRelationshipEntity(long id);
}
