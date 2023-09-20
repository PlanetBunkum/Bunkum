using Bunkum.HttpServer;

namespace Bunkum.ProfanityFilter;

public static class BunkumServerExtensions
{
    public static void AddProfanityService(this BunkumHttpServer server, string[]? allowList = null, string[]? extraDenyList = null)
    {
        allowList ??= Array.Empty<string>();
        extraDenyList ??= Array.Empty<string>();
        
        server.AddService<ProfanityService>(allowList, extraDenyList);
    }
}