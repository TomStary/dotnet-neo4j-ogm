using Neo4j.OGM.Internals.Extensions;
using Neo4j.OGM.Metadata;
using Neo4j.OGM.Utils;

namespace Neo4j.OGM.Context;

public class MappingContext
{
    private readonly Dictionary<object, long> primaryIdToNativeId = new();
    private readonly MetaData _metaData;
    private readonly IEnumerable<MappedRelationship> _relationshipRegister = new List<MappedRelationship>();
    private IdentityMap _indentityMap = new();

    public MappingContext(MetaData metaData)
    {
        _metaData = metaData;
    }

    public long NativeId(object entity)
    {
        GenerateIdIfNotPresent(entity);

        return EntityUtils.Identity(entity);
    }

    public bool HasChanges(object entity)
    {
        var id = OptionalNativeId(entity);
        return _indentityMap.CompareHash(id, entity);
    }

    public IEnumerable<MappedRelationship> GetRelationships()
    {
        return _relationshipRegister;
    }

    public bool ContainsRelationship(MappedRelationship relationship)
    {
        return _relationshipRegister.Contains(relationship);
    }

    public long? OptionalNativeId(object entity)
    {
        var id = entity.GetType().HasIdentityProperty() ? EntityUtils.Identity(entity) : entity.GetType().GetKeyValue(entity);
        return (long?)id ?? null;

    }

    private void GenerateIdIfNotPresent(object entity)
    {
        var id = (long?)entity.GetType().GetKeyValue(entity);

        if (id == null)
        {
            id = EntityUtils.NextRef();
            entity.GetType().SetKeyValue(entity, id);
        }
    }
}
