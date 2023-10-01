using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;

namespace Bunkum.Protocols.Gopher;

public class GopherEndpointAttribute : EndpointAttribute
{
    public GopherEndpointAttribute(string route) : base(route)
    {
        this.Method = Method.Invalid;
    }
}