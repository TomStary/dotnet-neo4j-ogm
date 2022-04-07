using System.Reflection;
using Neo4j.OGM.Internals.Extensions;

namespace Neo4j.OGM.Metadata;

/// <summary>
/// This class contains two Dictionaries of types, one for all node classes and one for replationship classes.
/// These can be used to further scan doman model and create <see cref="MetaData">.
/// </summary>
public class DomainInfo
{
    public readonly Dictionary<string, Type> NodeClasses;

    public readonly Dictionary<string, Type> RelationshipClasses;

    public DomainInfo(params Assembly[] assemblies)
    {
        NodeClasses = new Dictionary<string, Type>();
        RelationshipClasses = new Dictionary<string, Type>();

        FindAllNodeClasses(assemblies);
        FindAllRelationshipClasses(assemblies);
    }

    private void FindAllNodeClasses(Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract && type.HasNodeAttribute())
                {
                    NodeClasses.Add(type.Name, type);
                }
            }
        }
    }

    private void FindAllRelationshipClasses(Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract && type.HasRelationshipEntityAttribute())
                {
                    RelationshipClasses.Add(type.Name, type);
                }
            }
        }
    }
}
