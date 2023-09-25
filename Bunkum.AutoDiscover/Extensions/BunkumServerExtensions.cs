using Bunkum.HttpServer;

namespace Bunkum.AutoDiscover.Extensions;

public static class BunkumServerExtensions
{
    public static void AddAutoDiscover(this BunkumHttpServer server, string serverBrand, string baseEndpoint,
        bool usesCustomDigestKey = false)
    {
        AutoDiscoverConfig config = new(serverBrand, baseEndpoint, usesCustomDigestKey);
        server.AddService<AutoDiscoverService>(config);
        server.AddEndpointGroup<AutoDiscoverEndpoints>();
    }
}