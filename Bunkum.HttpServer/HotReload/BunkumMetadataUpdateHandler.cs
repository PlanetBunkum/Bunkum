using System.Reflection.Metadata;
using Bunkum.HttpServer.HotReload;
using JetBrains.Annotations;

[assembly: MetadataUpdateHandler(typeof(BunkumMetadataUpdateHandler))]

namespace Bunkum.HttpServer.HotReload;

/// <summary>
/// A receiver for updates of type metadata.
/// For more information, see https://learn.microsoft.com/en-us/dotnet/api/system.reflection.metadata.metadataupdatehandlerattribute
/// </summary>
internal class BunkumMetadataUpdateHandler
{
    /// <summary>
    /// Called when a hot reload is triggered and the program's metadata has updated.
    /// </summary>
    /// <param name="updatedTypes">The types affected by the metadata update. If null, any type may have been updated.</param>
    [UsedImplicitly]
    public static void UpdateApplication(Type[]? updatedTypes)
    {
        BunkumHotReloadableRegistry.ProcessHotReload();
    } 
}