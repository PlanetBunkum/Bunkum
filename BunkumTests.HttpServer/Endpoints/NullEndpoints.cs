using System.Net;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Listener.Parsing;

namespace BunkumTests.HttpServer.Endpoints;

public class NullEndpoints : EndpointGroup
{
    [Endpoint("/null", ContentType.Json)]
    [NullStatusCode(HttpStatusCode.BadRequest)]
    public object? Null(RequestContext context)
    {
        if (context.QueryString["null"] == "true") return null;
        return new object();
    }
}