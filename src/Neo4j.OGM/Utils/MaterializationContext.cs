using System.Reflection;
using System.Runtime.CompilerServices;

namespace Neo4j.OGM.Utils;

// source from: http://github.com/dotnet/efcore
internal readonly struct MaterializationContext
{
    public static readonly MethodInfo GetValueBufferMethod
        = typeof(MaterializationContext).GetProperty(nameof(ValueBuffer))!.GetMethod!;

    internal static readonly PropertyInfo ContextProperty
        = typeof(MaterializationContext).GetProperty(nameof(Context))!;

    public MaterializationContext(
        in ValueBuffer valueBuffer,
        ISession context)
    {
        ValueBuffer = valueBuffer;
        Context = context;
    }

    /// <summary>
    ///     The <see cref="ValueBuffer" /> to use to materialize an entity.
    /// </summary>
    public ValueBuffer ValueBuffer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    /// <summary>
    ///     The current <see cref="DbContext" /> instance being used.
    /// </summary>
    public ISession Context
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }
}
