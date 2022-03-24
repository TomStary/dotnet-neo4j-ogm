using System.Collections;
using Neo4j.OGM.Context;
using Neo4j.OGM.Requests;

namespace Neo4j.OGM.Internals;

internal class SessionSaveDelegate
{
    private readonly Session _session;
    private readonly RequestExecutor _requestExecutor;

    internal SessionSaveDelegate(Session session)
    {
        _session = session;
        _requestExecutor = new RequestExecutor(session);
    }

    internal async Task SaveAsync<TEntity>(TEntity entity, int depth = -1) where TEntity : class
    {
        // first we need to map relationships
        var entityGraphMapper = new EntityGraphMapper(_session.Metadata, _session.MappingContext);

        IEnumerable<TEntity> objects;
        if (typeof(IEnumerable).IsAssignableFrom(entity.GetType()))
        {
            objects = (IEnumerable<TEntity>)entity;
        }
        else
        {
            objects = new[] { entity };
        }

        foreach (var item in objects)
        {
            entityGraphMapper.Map(item, depth);
        }

        await _requestExecutor.ExecuteSave(entityGraphMapper.CompilerContext());
    }
}
