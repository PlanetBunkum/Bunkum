using System.Diagnostics;
using Bunkum.Listener.Request;

namespace Bunkum.Protocols.Http.Direct;

public class DirectHttpListenerContext : ListenerContext
{
    private readonly MemoryStream _stream;
    private readonly ManualResetEventSlim? _reset;
    private bool _closed;

    public DirectHttpListenerContext(MemoryStream stream, ManualResetEventSlim reset)
    {
        this._stream = stream;
        this._reset = reset;
    }

    public DirectHttpListenerContext()
    {
        this._stream = new MemoryStream(Array.Empty<byte>(), false);
    }

    protected override bool CanSendData => !this._closed;
    public override long ContentLength => this.InputStream.Length;

    protected override async Task SendResponseInternal(Enum code, ArraySegment<byte>? data = null)
    {
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