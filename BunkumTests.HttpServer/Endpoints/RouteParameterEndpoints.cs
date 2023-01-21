using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;

namespace BunkumTests.HttpServer.Endpoints;


public class RouteParameterEndpoints : EndpointGroup
{
    [Endpoint("/param/{input}")]
    public string Parameter(RequestContext context, string input)
    {
        return input;
    }
    
    [Endpoint("/params/{input}/{inputOther}")]
    public string Parameters(RequestContext context, string input, string inputOther)
    {
        return input + "," + inputOther;
    }
}