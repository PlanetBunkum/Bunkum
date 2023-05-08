using System.Net.Sockets;

namespace Bunkum.CustomHttpListener.Request;

public class SocketListenerContext : ListenerContext
{
    private readonly Socket _socket;
    
    private bool _socketClosed;
    internal bool SocketClosed => this._socketClosed || !this._socket.Connected;
    
    internal SocketListenerContext(Socket socket)
    {
        this._socket = socket;
    }

    public override bool CanSendData => !this.SocketClosed;

    public override void CloseConnection()
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
    protected override async Task SendBufferSafe(byte[] buffer)
    {
        if (this.SocketClosed) return;
        
        try
        {
            await this._socket.SendAsync(buffer);
        }
        catch
        {
            // ignored, log warning in the future?
        }
    }
}