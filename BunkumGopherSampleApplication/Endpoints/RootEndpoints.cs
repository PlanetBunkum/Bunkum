using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Gopher;
using Bunkum.Protocols.Gopher.Responses;

namespace BunkumGopherSampleApplication.Endpoints;

public class RootEndpoints : EndpointGroup
{
    [GopherEndpoint("/")]
    public Gophermap GetRoot(RequestContext context)
    {
        return new Gophermap
        {
            Items = new List<GophermapItem>
            {
                new()
                {
                    ItemType = GopherItemType.Message,
                    DisplayString = "Yo man",
                },
            },
        };
    }
}