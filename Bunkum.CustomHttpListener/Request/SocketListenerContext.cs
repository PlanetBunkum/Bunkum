using System.Net.Sockets;

namespace Bunkum.CustomHttpListener.Request;

public class SocketListenerContext : ListenerContext
{
    private readonly Socket _socket;
    
    private bool _socketClosed;
    private bool SocketClosed => this._socketClosed || !this._socket.Connected;
    
    public SocketListenerContext(Socket socket)
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
}