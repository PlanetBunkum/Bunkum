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
    public AutoDiscoverResponse AutoDiscover(RequestContext context, BunkumConfig bunkumConfig, AutoDiscoverConfig config)
    {
        string url = bunkumConfig.ExternalUrl;
        
        if (config.BaseEndpoint != null)
            url = bunkumConfig.ExternalUrl + config.BaseEndpoint;

        if (config.UrlDelegate != null)
            url = config.UrlDelegate(context, context.QueryString["game"]);
        
        return new AutoDiscoverResponse
        {
            ServerBrand = config.ServerBrand,
            Url = url,
            UsesCustomDigestKey = config.UsesCustomDigestKey,
            ServerDescription = config.ServerDescription,
            BannerImageUrl = config.BannerImageUrl,
        };
    }
}