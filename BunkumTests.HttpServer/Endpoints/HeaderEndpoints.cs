using System.Net;
using Bunkum.Core;
using Bunkum.Core.Endpoints;

namespace BunkumTests.HttpServer.Endpoints;

public class HeaderEndpoints : EndpointGroup
{
    [HttpEndpoint("/header/{name}")]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public string? GetHeader(RequestContext context, string name)
    {
        return context.RequestHeaders[name];
    }
    
    [HttpEndpoint("/cookie/{name}")]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public string? GetCookie(RequestContext context, string name)
    {
        return context.Cookies[name];
    }
}