using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using JetBrains.Annotations;

namespace BunkumTests.HttpServer.Endpoints;

[NoReorder]
public class RouteStartsWithEndpoints : EndpointGroup
{
    [HttpEndpoint("/sw/a")]
    public string A(RequestContext context)
    {
        return "a";
    }
    
    [HttpEndpoint("/sw/asdf")]
    public string Asdf(RequestContext context)
    {
        return "asdf";
    }
}