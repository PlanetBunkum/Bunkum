using System.Diagnostics;

namespace Bunkum.HttpServer.HotReload;

/// <summary>
/// A registry containing objects that can be hot reloaded.
/// </summary>
internal static class BunkumHotReloadableRegistry
{
    private static readonly List<IHotReloadable> HotReloadableObjects = new(1);

    internal static void ProcessHotReload()
    {
        Debug.WriteLine("Hot reload!");
        Debug.WriteLine($"- Processing {HotReloadableObjects.Count} reloadable objects");
        int i = 0;
        foreach (IHotReloadable reloadable in HotReloadableObjects)
        {
            i++;
            Debug.WriteLine($"  ({i}/{HotReloadableObjects.Count}) Reloading {reloadable.GetType().Name}...");
            reloadable.ProcessHotReload();
        }
        
        Debug.WriteLine("Done hot reloading.");
    }

    /// <summary>
    /// Register a reloadable object, signing it up to receive an event when a hot reload occurs.
    /// </summary>
    internal static void RegisterReloadable(IHotReloadable reloadable)
    {
        HotReloadableObjects.Add(reloadable);
    }

    /// <summary>
    /// Remove a reloadable object from the registry. Call this when the object is disposed or collected.
    /// </summary>
    internal static void UnregisterReloadable(IHotReloadable reloadable)
    {
        HotReloadableObjects.Remove(reloadable);
    }
}