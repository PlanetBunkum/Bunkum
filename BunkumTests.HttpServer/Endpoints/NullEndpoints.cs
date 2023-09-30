using System.Net;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;

namespace BunkumTests.HttpServer.Endpoints;

public class NullEndpoints : EndpointGroup
{
    [HttpEndpoint("/null", ContentType.Json)]
    [NullStatusCode(HttpStatusCode.BadRequest)]
    public object? Null(RequestContext context)
    {
        if (context.QueryString["null"] == "true") return null;
        return new object();
    }
}