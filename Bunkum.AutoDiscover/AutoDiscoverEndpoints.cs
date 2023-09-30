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
    public AutoDiscoverResponse AutoDiscover(RequestContext context, BunkumConfig bunkumConfig, AutoDiscoverConfig discoverConfig) => new()
        {
            ServerBrand = discoverConfig.ServerBrand,
            Url = bunkumConfig.ExternalUrl + discoverConfig.BaseEndpoint,
            UsesCustomDigestKey = discoverConfig.UsesCustomDigestKey
        };
}