using Bunkum.Core;

namespace Bunkum.AutoDiscover.Extensions;

/// <summary>
/// Extension class that facilitates injection of this library's services.
/// </summary>
public static class BunkumServerExtensions
{
    /// <summary>
    /// Adds the AutoDiscover service to the server.
    /// </summary>
    /// <param name="server">The server to inject to.</param>
    /// <param name="serverBrand">A friendly name of the server. You can put anything you like here.</param>
    /// <param name="baseEndpoint">The base url that should be recommended to the client.</param>
    /// <param name="usesCustomDigestKey">
    /// A boolean, when true it represents that the LBP server will only accept the digest key <c>CustomServerDigest</c>.
    /// Otherwise, the server is using the default digest key.
    /// </param>
    public static void AddAutoDiscover(this BunkumHttpServer server, string serverBrand, string baseEndpoint,
        bool usesCustomDigestKey = false)
    {
        AutoDiscoverConfig config = new(serverBrand, baseEndpoint, usesCustomDigestKey);
        server.AddService<AutoDiscoverService>(config);
        server.AddEndpointGroup<AutoDiscoverEndpoints>();
    }
}