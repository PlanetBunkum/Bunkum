using Bunkum.Core;
using Bunkum.Core.Configuration;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Listener.Parsing;

namespace Bunkum.AutoDiscover;

internal class AutoDiscoverEndpoints : EndpointGroup
{
    [Endpoint("/autodiscover", ContentType.Json)]
    [Authentication(false)]
    public AutoDiscoverResponse AutoDiscover(RequestContext context, BunkumConfig bunkumConfig, AutoDiscoverConfig discoverConfig) => new()
        {
            ServerBrand = discoverConfig.ServerBrand,
            Url = bunkumConfig.ExternalUrl + discoverConfig.BaseEndpoint,
            UsesCustomDigestKey = discoverConfig.UsesCustomDigestKey
        };
}