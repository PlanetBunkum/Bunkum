namespace Bunkum.Listener.Request;

public class DirectListenerContext : ListenerContext
{
    private readonly MemoryStream _stream;
    private readonly ManualResetEventSlim? _reset;
    private bool _closed;

    public DirectListenerContext(MemoryStream stream, ManualResetEventSlim reset)
    {
        this._stream = stream;
        this._reset = reset;
    }

    public DirectListenerContext()
    {
        this._stream = new MemoryStream(Array.Empty<byte>(), false);
    }

    protected override bool CanSendData => !this._closed;
    public override long ContentLength => this.InputStream.Length;

    protected override void CloseConnection()
    {
        this._reset?.Set(); // Tell any threads that may be waiting on us that they can read.
        this._closed = true;
    }

    protected override async Task SendBuffer(ArraySegment<byte> buffer)
    {
        await this._stream.WriteAsync(buffer);
    }
}