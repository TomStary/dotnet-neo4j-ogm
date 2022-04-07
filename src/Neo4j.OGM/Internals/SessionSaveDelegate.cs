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

    /// <summary>
    /// Internal save method, reponsible for saving entity or entities with defined depth.
    /// Depth defines how many levels of relationships should be traversed.
    /// </summary>
    internal async Task SaveAsync<TEntity>(TEntity entity, int depth = -1) where TEntity : class
    {
        // create instance of EntityGraphMapper
        var entityGraphMapper = new EntityGraphMapper(_session.Metadata, _session.MappingContext);

        // tranform entity/entities into array
        IEnumerable<TEntity> objects;
        if (typeof(IEnumerable).IsAssignableFrom(entity.GetType()))
        {
            objects = (IEnumerable<TEntity>)entity;
        }
        else
        {
            objects = new[] { entity };
        }

        // map objects into graph and create statements
        foreach (var item in objects)
        {
            entityGraphMapper.Map(item, depth);
        }

        // execute statements
        await _requestExecutor.ExecuteSave(entityGraphMapper.CompilerContext());
    }
}
