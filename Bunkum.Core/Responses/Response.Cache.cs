using System.Net;

namespace Bunkum.Core.Responses;

public partial struct Response // Cache
{
    private static readonly Dictionary<HttpStatusCode, Response> CodeResponseCache = new();

    private static partial void SetupResponseCache()
    {
        foreach (HttpStatusCode code in Enum.GetValues<HttpStatusCode>())
        {
            Response response = new(code);
            CodeResponseCache.TryAdd(code, response);
        }
    }
    
    public static implicit operator Response(HttpStatusCode code) => CodeResponseCache[code];
}