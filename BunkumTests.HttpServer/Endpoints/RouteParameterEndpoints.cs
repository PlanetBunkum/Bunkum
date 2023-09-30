using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;

namespace BunkumTests.HttpServer.Endpoints;


public class RouteParameterEndpoints : EndpointGroup
{
    [HttpEndpoint("/param/{input}")]
    public string Parameter(RequestContext context, string input)
    {
        return input;
    }
    
    [HttpEndpoint("/params/{input}/{inputOther}")]
    public string Parameters(RequestContext context, string input, string inputOther)
    {
        return input + "," + inputOther;
    }
    
    [HttpEndpoint("/inlineParam/inline{input}")]
    public string InlineParameter(RequestContext context, string input)
    {
        return input;
    }
}