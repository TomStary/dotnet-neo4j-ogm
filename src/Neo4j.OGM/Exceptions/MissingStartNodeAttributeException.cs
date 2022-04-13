namespace Neo4j.OGM.Exceptions;

public class MissingStartNodeAttributeException : Exception
{
    public MissingStartNodeAttributeException(string? message) : base(message)
    {
    }
}
