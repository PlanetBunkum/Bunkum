namespace Bunkum.HttpServer.HotReload;

/// <summary>
/// A registry containing objects that can be hot reloaded.
/// </summary>
internal static class BunkumHotReloadableRegistry
{
    private static readonly List<IHotReloadable> HotReloadableObjects = new(1);

    internal static void ProcessHotReload()
    {
        Console.WriteLine("Hot reload!");
        Console.WriteLine($"- Processing {HotReloadableObjects.Count} reloadable objects");
        int i = 0;
        foreach (IHotReloadable reloadable in HotReloadableObjects)
        {
            i++;
            Console.WriteLine($"  ({i}/{HotReloadableObjects.Count}) Reloading {reloadable.GetType().Name}...");
            reloadable.ProcessHotReload();
        }
        
        Console.WriteLine("Done hot reloading.");
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