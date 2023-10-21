using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Gemini.Responses;

namespace Bunkum.Protocols.Gemini;

public class GeminiEndpointAttribute : EndpointAttribute
{
    public GeminiEndpointAttribute(string route) : base(route)
    {
        this.Method = Method.Invalid;
        this.ContentType = GeminiContentTypes.Gemtext;
    }

    public GeminiEndpointAttribute(string route, string contentType) : base(route)
    {
        this.Method = Method.Invalid;
        this.ContentType = contentType;
    }
}