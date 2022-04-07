namespace Neo4j.OGM.Cypher.Compilers;

public interface ICompilerContext
{
    IMultiStatementCypherCompiler Compiler { get; }
    void Register(object entity);
    void RegisterNewObject(long id, object entity);
    void Unregister(object entity);
    bool Visited(object entity, int horizon);
    NodeBuilder? VisitedNode(object entity);
    void VisitRelationshipEntity(long id);
    bool VisitedRelationshipEntity(long id);
}
