using System.Text;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Protocols.Gemini;
using Bunkum.Protocols.Gemini.Responses;

namespace BunkumGeminiSampleApplication.Endpoints;

public class RootEndpoints : EndpointGroup
{
    [GeminiEndpoint("/")]
    public Response Root(RequestContext context)
    {
        return new Response("# THIS IS A TEST\r\n", GeminiContentTypes.Gemtext);
    }
}