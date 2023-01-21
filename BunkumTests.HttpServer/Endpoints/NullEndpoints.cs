using System.Net;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;

namespace BunkumTests.HttpServer.Endpoints;

public class NullEndpoints : EndpointGroup
{
    [Endpoint("/null", ContentType.Json)]
    [NullStatusCode(HttpStatusCode.BadRequest)]
    public object? Null(RequestContext context)
    {
        if (context.Request.QueryString["null"] == "true") return null;
        return new object();
    }
}