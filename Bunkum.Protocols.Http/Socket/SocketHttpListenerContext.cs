using System.Diagnostics;
using Bunkum.Listener.Request;

namespace Bunkum.Protocols.Http.Socket;

public class SocketHttpListenerContext : SocketListenerContext
{
    public SocketHttpListenerContext(System.Net.Sockets.Socket socket) : base(socket)
    {}

    protected override async Task SendResponseInternal(Enum code, ArraySegment<byte>? data = null)
    {
        // this is dumb and stupid
        this.ResponseHeaders.Add("Server", "Bunkum");
        this.ResponseHeaders.Add("Connection", "close");
        this.ResponseHeaders.Add("Date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"));
        
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
}