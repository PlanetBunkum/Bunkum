using Bunkum.Listener.Request;

namespace Bunkum.Protocols.Gopher.Socket;

public class SocketGopherListenerContext : SocketListenerContext
{
    public SocketGopherListenerContext(System.Net.Sockets.Socket socket) : base(socket)
    {}

    public override long ContentLength => 0;

    public override async Task SendResponse(Enum code, ArraySegment<byte>? data = null)
    {
        if (!this.CanSendData) return;
        
        if (data.HasValue) await this.SendBufferSafe(data.Value);
        this.CloseConnection();
    }
}