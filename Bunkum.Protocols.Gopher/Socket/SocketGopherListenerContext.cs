using System.Net.Sockets;
using Bunkum.Listener.Request;

namespace Bunkum.Protocols.Gopher.Socket;

public class SocketGopherListenerContext : ListenerContext
{
    private readonly System.Net.Sockets.Socket _socket;
    
    private bool _socketClosed;
    private bool SocketClosed => this._socketClosed || !this._socket.Connected;
    
    public SocketGopherListenerContext(System.Net.Sockets.Socket socket)
    {
        this._socket = socket;
    }

    protected override bool CanSendData => !this.SocketClosed;
    protected override void CloseConnection()
    {
        if (this.SocketClosed) return;

        this._socketClosed = true;
        try
        {
            this._socket.Shutdown(SocketShutdown.Both);
            this._socket.Disconnect(false);
            this._socket.Close();
            this._socket.Dispose();
        }
        catch
        {
            // ignored
        }
    }

    protected override async Task SendBuffer(ArraySegment<byte> buffer)
    {
        await this._socket.SendAsync(buffer);
    }

    public override long ContentLength => 0;

    public override async Task SendResponse(Enum code, ArraySegment<byte>? data = null)
    {
        if (!this.CanSendData) return;
        
        if (data.HasValue) await this.SendBufferSafe(data.Value);
        this.CloseConnection();
    }
}