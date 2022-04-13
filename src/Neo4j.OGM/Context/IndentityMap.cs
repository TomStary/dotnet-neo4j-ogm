using Neo4j.OGM.Internals.Extensions;

namespace Neo4j.OGM.Context;

internal class IdentityMap
{
    private const long SEED = 0xBAAAAAFL;
    private Dictionary<long, long> _nodeHashes = new();

    internal void AddHash(long id, object entity)
    {
        if (entity.GetType().HasNodeAttribute())
        {
            _nodeHashes.Add(id, CreateHash(entity));
        }
    }

    internal bool CompareHash(long? id, object entity)
    {
        if (id == null || entity == null)
        {
            return false;
        }

        var isRelationship = entity.GetType().HasRelationshipEntityAttribute();
        Dictionary<long, long> hashes = isRelationship ? new() : _nodeHashes;

        if (!hashes.ContainsKey(id.Value))
        {
            return true;
        }

        var actual = CreateHash(entity);
        var expected = hashes[id.Value];

        return actual != expected;
    }

    private long CreateHash(object entity)
    {
        var properties = entity.GetType().GetProperties();

        var hash = SEED;
        foreach (var property in properties)
        {
            var value = property.GetValue(entity);
            if (value != null)
            {
                hash += value.GetHashCode();
            }
        }

        return hash;
    }
}
