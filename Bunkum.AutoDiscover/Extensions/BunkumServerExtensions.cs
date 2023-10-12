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
    /// <param name="serverDescription">An optional friendly description of the server. You can put anything you like here.</param>
    /// <param name="bannerImageUrl">
    /// An optional image that the client may retrieve.
    /// You can put any URL here as long as the link points to an image, however PNG is recommended.
    /// </param>
    public static void AddAutoDiscover(this BunkumServer server, string serverBrand, string baseEndpoint,
        bool usesCustomDigestKey = false, string serverDescription = "", string? bannerImageUrl = null)
    {
        AutoDiscoverConfig config = new(serverBrand, baseEndpoint, usesCustomDigestKey, serverDescription, bannerImageUrl);
        server.AddService<AutoDiscoverService>(config);
        server.AddEndpointGroup<AutoDiscoverEndpoints>();
    }
}