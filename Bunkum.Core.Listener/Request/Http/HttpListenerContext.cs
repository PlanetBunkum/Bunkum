using System.Diagnostics;
using System.Net;
using HttpMethod = Bunkum.Core.Listener.Parsing.HttpMethod;

namespace Bunkum.Core.Listener.Request.Http;

public abstract class HttpListenerContext : ListenerContext<HttpStatusCode, HttpVersion, HttpMethod>
{
    public override async Task SendResponse(HttpStatusCode code, ArraySegment<byte>? data = null)
    {
        if (!this.CanSendData) return;
        
        // this is dumb and stupid
        this.ResponseHeaders.Add("Server", "Bunkum");
        this.ResponseHeaders.Add("Connection", "close");
        this.ResponseHeaders.Add("Date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"));
        
        List<string> response = new() { $"HTTP/1.1 {(int)code} {code.ToString()}" }; // TODO: spaced code names ("Not Found" instead of "NotFound")
        foreach ((string? key, string? value) in this.ResponseHeaders)
        {
            Debug.Assert(key != null);
            Debug.Assert(value != null);
            
            response.Add($"{key}: {value}");
        }
        response.Add("\r\n");

        await this.SendBufferSafe(string.Join("\r\n", response));
        if (data.HasValue) await this.SendBufferSafe(data.Value);
        
        this.CloseConnection();
    }
}