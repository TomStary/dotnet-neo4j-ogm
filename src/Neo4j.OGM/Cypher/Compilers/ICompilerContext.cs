namespace Neo4j.OGM.Cypher.Compilers;

public interface ICompilerContext
{
    IMultiStatementCypherCompiler Compiler { get; }
    bool Visited(object entity, int horizon);
    NodeBuilder? VisitedNode(object entity);
    void VisitRelationshipEntity(long id);
    bool VisitedRelationshipEntity(long id);
}
