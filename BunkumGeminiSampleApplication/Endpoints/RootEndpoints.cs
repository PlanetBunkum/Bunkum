using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Gemini;

namespace BunkumGeminiSampleApplication.Endpoints;

public class RootEndpoints : EndpointGroup
{
    [GeminiEndpoint("/")]
    public string ManuallyAddedRootEndpoint(RequestContext context) =>
        @"# Weather test

=> /api/v1/weather V1 Weather API
=> /api/v2/weather V2 Weather API
=> /api/v3/weather V3 Weather API
";
}