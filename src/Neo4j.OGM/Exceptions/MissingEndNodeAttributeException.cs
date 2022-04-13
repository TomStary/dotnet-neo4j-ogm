namespace Neo4j.OGM.Exceptions;

public class MissingEndNodeAttributeException : Exception
{
    public MissingEndNodeAttributeException()
    {
    }

    public MissingEndNodeAttributeException(string? message) : base(message)
    {
    }
}
