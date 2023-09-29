using Bunkum.Core;
using Bunkum.Core.Endpoints;

namespace BunkumSampleApplication.Endpoints;

public class ManualEndpoints : EndpointGroup
{
    public string ManuallyAddedEndpoint(RequestContext context) =>
        "This endpoint was added manually by BunkumServer.AddEndpointGroup";
}