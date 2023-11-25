using System.Net.Sockets;

namespace Bunkum.Listener.Request;

/// <summary>
/// A <see cref="ListenerContext"/> with <see cref="Socket"/> support.
/// </summary>
public abstract class SocketListenerContext : ListenerContext
{
    private readonly Socket _socket;
    private readonly Stream _stream;
    
    private bool _socketClosed;
    private bool SocketClosed => this._socketClosed || !this._socket.Connected;
    
    /// <summary>
    /// Initializes the <see cref="ListenerContext"/> with socket support.
    /// </summary>
    /// <param name="socket">The socket to use for this context.</param>
    protected SocketListenerContext(Socket socket, Stream stream)
    {
        this._socket = socket;
        this._stream = stream;
    }

    /// <inheritdoc/>
    protected internal override bool CanSendData => !this.SocketClosed || !this._stream.CanWrite;
    
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
        await this._stream.WriteAsync(buffer);
    }
}