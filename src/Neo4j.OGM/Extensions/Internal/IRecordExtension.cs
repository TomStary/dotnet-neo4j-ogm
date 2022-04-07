using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Neo4j.Driver;
using Neo4j.OGM.Internals.Extensions;

namespace Neo4j.OGM.Extensions.Internals;

internal static class IRecordExtension
{
    public static TEntity MapRecordToType<TEntity>(this IRecord record, string alias)
    {
        var entity = Activator.CreateInstance<TEntity>();
        var entityType = typeof(TEntity);
        var properties = entityType.GetProperties();
        foreach (var property in properties)
        {
            var propertyName = property.Name;
            var propertyType = property.PropertyType;
            if (property.GetCustomAttributes().OfType<KeyAttribute>().Any())
            {
                property.SetValue(entity, ((IEntity)record[alias]).Id);
            }
            else if (((IEntity)record[alias]).Properties.ContainsKey(propertyName))
            {
                var propertyValue = ((IEntity)record[alias])[propertyName];
                if (propertyValue != null)
                {
                    property.SetValue(entity, Convert.ChangeType(propertyValue, propertyType));
                }
            }
        }
        return entity;
    }
}
