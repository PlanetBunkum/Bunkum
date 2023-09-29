using Bunkum.Core;
using Bunkum.Core.Endpoints;

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
    
    [Endpoint("/inlineParam/inline{input}")]
    public string InlineParameter(RequestContext context, string input)
    {
        return input;
    }
}