using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Configuration;
using Bunkum.HttpServer.Endpoints;

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