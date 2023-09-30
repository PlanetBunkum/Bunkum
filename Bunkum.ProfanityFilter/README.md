# Bunkum.ProfanityFilter
A wrapper around Profanity.Detector for Bunkum

## Example usage

```csharp
// Program.cs
BunkumHttpServer server = new();
server.AddProfanityService();
```

```csharp
// FilterEndpoints.cs
[HttpEndpoint("/censorSentence")]
[NullStatusCode(HttpStatusCode.BadRequest)]
public string? CensorSentence(RequestContext context, ProfanityService service)
{
    string? input = context.QueryString.Get("input");
    if (input == null) return null;
    
    return service.CensorSentence(input);
}
```