using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Bunkum.Listener.Protocol;

namespace Bunkum.Listener.Request;

/// <summary>
/// The listener's context for a request. Controls the request and its data.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public abstract class ListenerContext
{
    /// <summary>
    /// The body of the request as a stream.
    /// </summary>
    public MemoryStream InputStream { get; set; } = null!;

    /// <summary>
    /// The protocol version the request is using.
    /// </summary>
    public ProtocolInformation Protocol { get; set; }
    
    /// <summary>
    /// The protocol method the request is using.
    /// </summary>
    public Method Method { get; set; }
    
    /// <summary>
    /// The location/endpoint the client is going to.
    /// </summary>
    public Uri Uri { get; set; } = null!;

    /// <summary>
    /// The request's headers
    /// </summary>
    public readonly NameValueCollection RequestHeaders = new();
    
    /// <summary>
    /// The response's headers
    /// </summary>
    public readonly Dictionary<string, string> ResponseHeaders = new();

    /// <summary>
    /// Cookies shared between the server and client
    /// </summary>
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

    /// <summary>
    /// The length of the request body.
    /// </summary>
    public virtual long ContentLength
    {
        get
        {
            string? lengthStr = this.RequestHeaders["Content-Length"];
            _ = long.TryParse(lengthStr, out long length);
            return length;
        }
    }

    /// <summary>
    /// Whether or not the request has a body.
    /// </summary>
    public bool HasBody => this.ContentLength > 0;
    
    /// <summary>
    /// Can we currently send data back to the client?
    /// </summary>
    protected abstract bool CanSendData { get; }

    // Response
    
    /// <summary>
    /// The status code to respond with.
    /// </summary>
    public Enum ResponseCode = HttpStatusCode.OK;
    
    /// <summary>
    /// The type of content we are responding with.
    /// </summary>
    public ContentType? ResponseType;

    private int _responseLength;

    // ReSharper disable once MemberCanBePrivate.Global
    
    /// <summary>
    /// The response we are about to send.
    /// </summary>
    public MemoryStream ResponseStream { get; } = new();

    /// <summary>
    /// Writes a string to the response stream.
    /// </summary>
    /// <param name="str">The string to write.</param>
    public void Write(string str) => this.Write(Encoding.Default.GetBytes(str));
    
    /// <summary>
    /// Writes a byte array to the response stream.
    /// </summary>
    /// <param name="buffer">The byte array to write.</param>
    public void Write(byte[] buffer)
    {
        this.ResponseStream.Write(buffer);
        this._responseLength += buffer.Length;
    }

    /// <summary>
    /// Finish up the response, and close the connection.
    /// </summary>
    public virtual async Task FlushResponseAndClose()
    {
        if (this.ResponseType != null)
            this.ResponseHeaders.Add("Content-Type", this.ResponseType.Value.ToString());
        
        if(this._responseLength != 0)
            this.ResponseHeaders.Add("Content-Length", this._responseLength.ToString());

        ArraySegment<byte> dataSlice = new(this.ResponseStream.GetBuffer(), 0, this._responseLength);

        await this.SendResponse(this.ResponseCode, dataSlice);
    }

    /// <summary>
    /// Send our response headers, status code, and close the connection.  
    /// </summary>
    /// <param name="code"></param>
    /// <param name="data"></param>
    public virtual async Task SendResponse(Enum code, ArraySegment<byte>? data = null)
    {
        if (!this.CanSendData) return;
        
        // this is dumb and stupid
        this.ResponseHeaders.Add("Server", "Bunkum");
        this.ResponseHeaders.Add("Connection", "close");
        this.ResponseHeaders.Add("Date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"));
        
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
        
        this.CloseConnection();
    }

    /// <summary>
    /// Closes the connection we made. Called after a request has been processed, and after the response has been sent.
    /// </summary>
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

    /// <summary>
    /// Sends data directly to the client.
    /// </summary>
    /// <param name="buffer">The data to send.</param>
    protected abstract Task SendBuffer(ArraySegment<byte> buffer);
}