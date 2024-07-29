using System.Net;
using Bunkum.Core;
using Bunkum.Core.Configuration;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Gemini;
using Bunkum.Protocols.Gemini.Responses;

namespace BunkumGeminiSampleApplication.Endpoints;

public class RootEndpoints : EndpointGroup
{
    [GeminiEndpoint("/")]
    public string ManuallyAddedRootEndpoint(RequestContext context, GeminiBunkumConfig config)
    {
        UriBuilder titanUri = new(config.ExternalUrl)
        {
            Scheme = "titan",
            Path = "titan",
        };
        
        return $@"# Weather test

=> /api/v1/weather V1 Weather API
=> /api/v2/weather V2 Weather API
=> /api/v3/weather V3 Weather API

=> /requires_cert Endpoint that requires a client cert
=> {titanUri} Test titan file upload
=> /input Query input test
";
    }

    [GeminiEndpoint("/requires_cert")]
    public Response RequiresAuthentication(RequestContext context)
    {
        if (context.RemoteCertificate == null) 
            return new Response("This endpoint requires authentication!", ContentType.Plaintext, HttpStatusCode.UpgradeRequired);
        
        return new Response($@"# Hello {context.RemoteCertificate.Issuer}!

We have authenticated you!", GeminiContentTypes.Gemtext);
    }

    [GeminiEndpoint("/titan")]
    public Response TitanUpload(RequestContext context, byte[] body)
    {
        return new Response($@"# Titan upload test

Token: {context.QueryString["token"]}
Mime: {context.QueryString["mime"]}
Body: {body.Length}", GeminiContentTypes.Gemtext);
    }

    [GeminiEndpoint("/input")]
    public Response Input(RequestContext context)
    {
        string? input = context.QueryString.Get("input");
        if (input != null)
        {
            return new Response($"# You have input the text \"{input}\"", GeminiContentTypes.Gemtext);
        }
        
        return new Response("TEST INPUT FIELD", ContentType.Plaintext, HttpStatusCode.Continue);
    }
}