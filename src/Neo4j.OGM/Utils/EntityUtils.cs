using Neo4j.OGM.Internals.Extensions;

namespace Neo4j.OGM.Utils;

internal class EntityUtils
{
    static long _id = 0;

    internal static long NextRef()
    {
        return Interlocked.Decrement(ref _id);
    }

    internal static long Identity(object entity)
    {
        var id = entity.GetType().GetKeyValue(entity);

        if (id == null)
        {
            id = NextRef();
            entity.GetType().SetKeyValue(entity, id);
        }

        return (long)id;
    }
}
