using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;

namespace BunkumTests.HttpServer.Endpoints;

public class MultipleEndpoints : EndpointGroup
{
    [HttpEndpoint("/a")]
    [HttpEndpoint("/b")]
    public string Test(RequestContext context)
    {
        return "works";
    }
}