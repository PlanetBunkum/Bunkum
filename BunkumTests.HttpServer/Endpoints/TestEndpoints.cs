using Bunkum.Core;
using Bunkum.Core.Endpoints;

namespace BunkumTests.HttpServer.Endpoints;

public class TestEndpoints : EndpointGroup
{
    public const string TestString = "Test";

    [Endpoint("/")]
    public string Test(RequestContext context)
    {
        return TestString;
    }

    [Endpoint("/echoString")]
    public string Echo(RequestContext context, string body)
    {
        return body;
    }
}