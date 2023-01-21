using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using JetBrains.Annotations;

namespace BunkumTests.HttpServer.Endpoints;

[NoReorder]
public class RouteStartsWithEndpoints : EndpointGroup
{
    [Endpoint("/sw/a")]
    public string A(RequestContext context)
    {
        return "a";
    }
    
    [Endpoint("/sw/asdf")]
    public string Asdf(RequestContext context)
    {
        return "asdf";
    }
}