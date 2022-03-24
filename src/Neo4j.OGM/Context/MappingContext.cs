using Neo4j.OGM.Exceptions;
using Neo4j.OGM.Internals.Extensions;
using Neo4j.OGM.Metadata;
using Neo4j.OGM.Utils;

namespace Neo4j.OGM.Context;

public class MappingContext
{
    private readonly Dictionary<object, long> primaryIdToNativeId = new();
    private readonly MetaData _metaData;
    private readonly IEnumerable<MappedRelationship> _relationshipRegister = new List<MappedRelationship>();
    private IndentityMap _indentityMap = new();

    public MappingContext(MetaData metaData)
    {
        _metaData = metaData;
    }

    public long NativeId(object entity)
    {
        GenerateIdIfNotPresent(entity);

        if (entity.GetType().HasIdentityProperty())
        {
            return EntityUtils.Identity(entity);
        }
        else
        {
            var keyValue = entity.GetType().GetKeyValue(entity);
            if (keyValue == null)
            {
                throw new MappingException($"Entity {entity.GetType().Name} has no key value.");
            }

            if (!primaryIdToNativeId.ContainsKey(keyValue))
            {
                var graphId = EntityUtils.NextRef();
                primaryIdToNativeId.Add(keyValue, graphId);
            }

            return primaryIdToNativeId[keyValue];
        }
    }

    public bool HasChanges(object entity)
    {
        var id = OptionalNativeId(entity);
        return _indentityMap.CompareHash(id, entity);
    }

    internal IEnumerable<MappedRelationship> GetRelationships()
    {
        return _relationshipRegister;
    }

    public long? OptionalNativeId(object entity)
    {
        var id = entity.GetType().HasIdentityProperty() ? EntityUtils.Identity(entity) : entity.GetType().GetKeyValue(entity);
        if (id == null)
        {
            return null;
        }

        if (!primaryIdToNativeId.ContainsKey(id))
        {
            return null;
        }

        return primaryIdToNativeId[id];
    }

    private void GenerateIdIfNotPresent(object entity)
    {
        var id = entity.GetType().GetKeyValue(entity);

        if (id == null)
        {
            id = (entity.GetType().GetMemberInfoOfKeyAttribute()?.GetType().Name) switch
            {
                "Int32" => EntityUtils.NextRef(),
                "Guid" => Guid.NewGuid(),
                "String" => Guid.NewGuid().ToString(),
                _ => throw new NotSupportedException($"Id type {entity.GetType().GetMemberInfoOfKeyAttribute()?.GetType().Name} has no value and cannot be generated."),
            };
            entity.GetType().SetKeyValue(entity, id);
        }
    }

    internal bool ContainsRelationship(MappedRelationship relationship)
    {
        return _relationshipRegister.Contains(relationship);
    }
}
