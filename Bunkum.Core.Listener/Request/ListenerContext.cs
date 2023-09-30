using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Bunkum.Core.Listener.Parsing;

namespace Bunkum.Core.Listener.Request;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public abstract class ListenerContext<TStatusCode, TProtocolVersion, TProtocolMethod>
    where TStatusCode : Enum
    where TProtocolVersion : Enum
    where TProtocolMethod : Enum
{
    public MemoryStream InputStream { get; set; } = null!;

    public TProtocolVersion Version { get; set; }
    public TProtocolMethod Method { get; set; }
    public Uri Uri { get; set; } = null!;

    public readonly NameValueCollection RequestHeaders = new();
    public readonly Dictionary<string, string> ResponseHeaders = new();

    public readonly NameValueCollection Cookies = new();
    public NameValueCollection Query { get; set; } = null!;

    /// <summary>
    /// The actual endpoint this request originated from, ignoring BunkumConfig.UseForwardedIp.
    /// In most cases you should use <see cref="RemoteEndpoint"/>.
    /// </summary>
    public IPEndPoint RealRemoteEndpoint = null!;
    /// <summary>
    /// The endpoint this request originated from.
    /// If BunkumConfig.UseForwardedIp is enabled, this will use the endpoint forwarded via the reverse proxy. 
    /// </summary>
    public IPEndPoint RemoteEndpoint = null!;

    public virtual long ContentLength
    {
        get
        {
            string? lengthStr = this.RequestHeaders["Content-Length"];
            _ = long.TryParse(lengthStr, out long length);
            return length;
        }
    }

    public bool HasBody => this.ContentLength > 0;
    
    protected abstract bool CanSendData { get; }

    // Response
    public TStatusCode ResponseCode;
    public ContentType? ResponseType;

    private int _responseLength;

    // ReSharper disable once MemberCanBePrivate.Global
    public MemoryStream ResponseStream { get; } = new();

    public void Write(string str) => this.Write(Encoding.Default.GetBytes(str));
    
    public void Write(byte[] buffer)
    {
        this.ResponseStream.Write(buffer);
        this._responseLength += buffer.Length;
    }

    public async Task FlushResponseAndClose()
    {
        if (this.ResponseType != null)
            this.ResponseHeaders.Add("Content-Type", this.ResponseType.Value.GetName());
        
        if(this._responseLength != 0)
            this.ResponseHeaders.Add("Content-Length", this._responseLength.ToString());

        ArraySegment<byte> dataSlice = new(this.ResponseStream.GetBuffer(), 0, this._responseLength);

        await this.SendResponse(this.ResponseCode, dataSlice);
    }

    public abstract Task SendResponse(TStatusCode code, ArraySegment<byte>? data = null);

    protected abstract void CloseConnection();

    protected Task SendBufferSafe(string str) => this.SendBufferSafe(Encoding.UTF8.GetBytes(str));
    protected async Task SendBufferSafe(ArraySegment<byte> buffer)
    {
        if (!this.CanSendData) return;
        
        try
        {
            await this.SendBuffer(buffer);
        }
        catch
        {
            // ignored, log warning in the future?
        }
    }

    protected abstract Task SendBuffer(ArraySegment<byte> buffer);

    public abstract Task HandleNoEndpoint();
    public abstract Task HandleInvalidRequest();
}