using System.Net.Sockets;

namespace Bunkum.Listener.Request;

/// <summary>
/// A <see cref="ListenerContext"/> with <see cref="Socket"/> support.
/// </summary>
public abstract class SocketListenerContext : ListenerContext
{
    private readonly Socket _socket;
    
    private bool _socketClosed;
    private bool SocketClosed => this._socketClosed || !this._socket.Connected;
    
    /// <summary>
    /// Initializes the <see cref="ListenerContext"/> with socket support.
    /// </summary>
    /// <param name="socket">The socket to use for this context.</param>
    protected SocketListenerContext(Socket socket)
    {
        this._socket = socket;
    }

    /// <inheritdoc/>
    protected override bool CanSendData => !this.SocketClosed;
    
    /// <inheritdoc/>
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
    
    /// <inheritdoc/>
    protected override async Task SendBuffer(ArraySegment<byte> buffer)
    {
        await this._socket.SendAsync(buffer);
    }
}