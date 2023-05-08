using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Bunkum.CustomHttpListener.Parsing;

namespace Bunkum.CustomHttpListener.Request;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public abstract class ListenerContext
{
    public abstract bool CanSendData { get; }
    
    public MemoryStream InputStream { get; internal set; } = null!;

    public Method Method { get; internal set; }
    public Uri Uri { get; internal set; } = null!;

    public readonly NameValueCollection RequestHeaders = new();
    public readonly Dictionary<string, string> ResponseHeaders = new();

    public readonly NameValueCollection Cookies = new();
    public NameValueCollection Query { get; internal set; } = null!;

    public IPEndPoint RemoteEndpoint = null!;

    public long ContentLength
    {
        get
        {
            string? lengthStr = this.RequestHeaders["Content-Length"];
            long.TryParse(lengthStr, out long length);
            return length;
        }
    }

    public bool HasBody
    {
        get
        {
            string? lengthStr = this.RequestHeaders["Content-Length"];
            if (lengthStr is null or "0") return false;

            return true;
        }
    }

    // Response
    public HttpStatusCode ResponseCode = HttpStatusCode.OK;
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

        await this.SendResponse(this.ResponseCode, this.ResponseStream.GetBuffer());
    }

    internal virtual async Task SendResponse(HttpStatusCode code, byte[]? data = null)
    {
        List<string> response = new() { $"HTTP/1.1 {(int)code} {code.ToString()}" };
        foreach ((string? key, string? value) in this.ResponseHeaders)
        {
            Debug.Assert(key != null);
            Debug.Assert(value != null);
            
            response.Add($"{key}: {value}");
        }
        response.Add("\r\n");

        await this.SendBufferSafe(string.Join("\r\n", response));
        if (data != null) await this.SendBufferSafe(data);
        
        this.CloseConnection();
    }

    public abstract void CloseConnection();

    private Task SendBufferSafe(string str) => this.SendBufferSafe(Encoding.UTF8.GetBytes(str));
    protected abstract Task SendBufferSafe(byte[] buffer);
}