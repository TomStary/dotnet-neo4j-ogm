using System.Runtime.Serialization;

namespace Neo4j.OGM.Exceptions;

public class MissingEndNodeAttributeException : Exception
{
    public MissingEndNodeAttributeException()
    {
    }

    public MissingEndNodeAttributeException(string? message) : base(message)
    {
    }

    public MissingEndNodeAttributeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected MissingEndNodeAttributeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}