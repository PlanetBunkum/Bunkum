using System.Net;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;

namespace BunkumTests.HttpServer.Endpoints;

public class HeaderEndpoints : EndpointGroup
{
    [Endpoint("/header/{name}")]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public string? GetHeader(RequestContext context, string name)
    {
        return context.RequestHeaders[name];
    }
    
    [Endpoint("/cookie/{name}")]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public string? GetCookie(RequestContext context, string name)
    {
        return context.Cookies[name];
    }
}