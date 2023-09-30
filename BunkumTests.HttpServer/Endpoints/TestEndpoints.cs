using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;

namespace BunkumTests.HttpServer.Endpoints;

public class TestEndpoints : EndpointGroup
{
    public const string TestString = "Test";

    [HttpEndpoint("/")]
    public string Test(RequestContext context)
    {
        return TestString;
    }

    [HttpEndpoint("/echoString")]
    public string Echo(RequestContext context, string body)
    {
        return body;
    }
}