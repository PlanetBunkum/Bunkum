using Bunkum.Core;
using Bunkum.Core.Endpoints;

namespace BunkumTests.HttpServer.Endpoints;

public class MultipleEndpoints : EndpointGroup
{
    [Endpoint("/a")]
    [Endpoint("/b")]
    public string Test(RequestContext context)
    {
        return "works";
    }
}