using System.Net;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;

namespace Bunkum.ProfanityFilter.Test;

public class FilterEndpoints : EndpointGroup
{
    [HttpEndpoint("/censorSentence")]
    [NullStatusCode(HttpStatusCode.BadRequest)]
    public string? CensorSentence(RequestContext context, ProfanityService service)
    {
        string? input = context.QueryString.Get("input");
        if (input == null) return null;

        return service.CensorSentence(input);
    }
    
    [HttpEndpoint("/sentenceContainsProfanity", ContentType.Json)]
    [NullStatusCode(HttpStatusCode.BadRequest)]
    public bool? SentenceContainsProfanity(RequestContext context, ProfanityService service)
    {
        string? input = context.QueryString.Get("input");
        if (input == null) return null;

        return service.SentenceContainsProfanity(input);
    }
}