using System.Net;
using Bunkum.Listener.Request;

namespace Bunkum.Protocols.Gopher.Socket;

public class SocketGopherListenerContext : SocketListenerContext
{
    public SocketGopherListenerContext(System.Net.Sockets.Socket socket) : base(socket)
    {}

    public override long ContentLength => 0;

    protected override async Task SendResponseInternal(HttpStatusCode code, ArraySegment<byte>? data = null)
    {
        if (data.HasValue) await this.SendBufferSafe(data.Value);
    }
}