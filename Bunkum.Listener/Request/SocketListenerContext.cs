using System.Net.Sockets;

namespace Bunkum.Listener.Request;

/// <summary>
/// A <see cref="ListenerContext"/> with <see cref="Socket"/> support.
/// </summary>
public abstract class SocketListenerContext : ListenerContext
{
    private readonly Socket _socket;
    /// <summary>
    /// The stream associated with this SocketListenerContext
    /// </summary>
    protected readonly Stream Stream;
    
    private bool _socketClosed;
    /// <summary>
    /// Whether or not the socket has been closed
    /// </summary>
    protected bool SocketClosed => this._socketClosed || !this._socket.Connected;
    
    /// <summary>
    /// Initializes the <see cref="ListenerContext"/> with socket support.
    /// </summary>
    /// <param name="socket">The socket to use for this context.</param>
    protected SocketListenerContext(Socket socket, Stream stream)
    {
        this._socket = socket;
        this.Stream = stream;
    }

    /// <inheritdoc/>
    protected internal override bool CanSendData => !this.SocketClosed || !this.Stream.CanWrite;
    
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
        await this.Stream.WriteAsync(buffer);
    }
}