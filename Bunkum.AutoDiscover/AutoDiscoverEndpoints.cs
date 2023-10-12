using Bunkum.Core;
using Bunkum.Core.Configuration;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;

namespace Bunkum.AutoDiscover;

internal class AutoDiscoverEndpoints : EndpointGroup
{
    [HttpEndpoint("/autodiscover", ContentType.Json)]
    [Authentication(false)]
    public AutoDiscoverResponse AutoDiscover(RequestContext context, BunkumConfig bunkumConfig, AutoDiscoverConfig config) => new()
        {
            ServerBrand = config.ServerBrand,
            Url = bunkumConfig.ExternalUrl + config.BaseEndpoint,
            UsesCustomDigestKey = config.UsesCustomDigestKey,
            ServerDescription = config.ServerDescription,
            BannerImageUrl = config.BannerImageUrl,
        };
}