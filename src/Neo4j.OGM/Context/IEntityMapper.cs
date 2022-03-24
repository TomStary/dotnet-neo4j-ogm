using Neo4j.OGM.Cypher.Compilers;

namespace Neo4j.OGM.Context;

public interface IEntityMapper
{
    ICompilerContext Map(object entity, int depth);
    ICompilerContext CompilerContext();
}

