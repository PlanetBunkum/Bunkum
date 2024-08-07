using System.Diagnostics;
using System.Net;
using System.Net.Security;
using Bunkum.Listener.Request;

namespace Bunkum.Protocols.Http.Socket;

public class SocketHttpListenerContext : SocketListenerContext
{
    public SocketHttpListenerContext(System.Net.Sockets.Socket socket, Stream stream) : base(socket, stream)
    {}

    protected override async Task SendResponseInternal(HttpStatusCode code, ArraySegment<byte>? data = null)
    {
        // These are "TryAdd" and not "Add" since if you are proxying requests,
        // both of these should be set to the headers sent by the proxied server, and not be generated by Bunkum directly
        this.ResponseHeaders.TryAdd("Server", "Bunkum");
        this.ResponseHeaders.TryAdd("Date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"));
        // This is an unconditional set because Bunkum does not support keep-alive connections,
        // theres currently no reason for anything other than Bunkum to set this value
        this.ResponseHeaders["Connection"] = "close";
        
        List<string> response = new() { $"HTTP/1.1 {code.GetHashCode()} {code.ToString()}" }; // TODO: spaced code names ("Not Found" instead of "NotFound")
        foreach ((string? key, string? value) in this.ResponseHeaders)
        {
            Debug.Assert(key != null);
            Debug.Assert(value != null);
            
            response.Add($"{key}: {value}");
        }
        response.Add("\r\n");

        await this.SendBufferSafe(string.Join("\r\n", response));
        if (data.HasValue) await this.SendBufferSafe(data.Value);
    }

    protected override void CloseConnection()
    {
        if (this.SocketClosed) return;
        
        // Make sure to safely shutdown the SSL connection, if we are using SSL
        if (this.Stream is SslStream stream) 
            stream.ShutdownAsync().Wait();
        
        base.CloseConnection();
    }
}