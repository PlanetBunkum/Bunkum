using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Gopher;

namespace BunkumGopherSampleApplication.Endpoints;

public class RootEndpoints : EndpointGroup
{
    [GopherEndpoint("/")]
    public string GetRoot(RequestContext context)
    {
        return "iYo man\n";
    }
}