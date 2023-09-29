using System.Net;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Listener.Parsing;

namespace Bunkum.ProfanityFilter.Test;

public class FilterEndpoints : EndpointGroup
{
    [Endpoint("/censorSentence")]
    [NullStatusCode(HttpStatusCode.BadRequest)]
    public string? CensorSentence(RequestContext context, ProfanityService service)
    {
        string? input = context.QueryString.Get("input");
        if (input == null) return null;

        return service.CensorSentence(input);
    }
    
    [Endpoint("/sentenceContainsProfanity", ContentType.Json)]
    [NullStatusCode(HttpStatusCode.BadRequest)]
    public bool? SentenceContainsProfanity(RequestContext context, ProfanityService service)
    {
        string? input = context.QueryString.Get("input");
        if (input == null) return null;

        return service.SentenceContainsProfanity(input);
    }
}