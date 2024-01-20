using Bunkum.Core;
using Bunkum.Core.Configuration;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Gopher;
using Bunkum.Protocols.Gopher.Responses;
using Bunkum.Protocols.Gopher.Responses.Items;

namespace BunkumGopherSampleApplication.Endpoints;

public class RootEndpoints : EndpointGroup
{
    [GopherEndpoint("/")]
    public List<GophermapItem> GetRoot(RequestContext context, BunkumConfig config)
    {
        return new List<GophermapItem>
        {
            new GophermapMessage("Welcome to Bunkum's Gopher sample application."),
            new GophermapLink("Visit an External Destination", new Uri("gopher://gopher.floodgap.com")),
            new GophermapLink("Visit a Local Endpoint", config, "/test"),
            new GophermapLink(GophermapItemType.IndexSearchServer, "Test Input", config, "/input"),
        };
    }

    [GopherEndpoint("/test")]
    public List<GophermapItem> GetLocalEndpoint(RequestContext context, BunkumConfig config)
    {
        return new List<GophermapItem>
        {
            new GophermapMessage("There are no Easter Eggs up here."),
            new GophermapLink("Go away.", config, "/"),
        };
    }
    
    [GopherEndpoint("/input")]
    public List<GophermapItem> GetUserInput(RequestContext context, BunkumConfig config)
    {
        return new List<GophermapItem>
        {
            new GophermapMessage($"You inputted '{context.QueryString["input"]}'."),
            new GophermapLink("Return to root", config, "/"),
        };
    }
}