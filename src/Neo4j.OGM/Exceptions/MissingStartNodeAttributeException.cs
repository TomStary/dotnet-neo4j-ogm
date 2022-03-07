using System.Runtime.Serialization;

namespace Neo4j.OGM.Exceptions;

public class MissingStartNodeAttributeException : Exception
{
    public MissingStartNodeAttributeException()
    {
    }

    public MissingStartNodeAttributeException(string? message) : base(message)
    {
    }

    public MissingStartNodeAttributeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected MissingStartNodeAttributeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}