using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Web;
using Bunkum.Listener;
using Bunkum.Listener.Extensions;
using Bunkum.Listener.Protocol;
using Bunkum.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.Protocols.Http.Socket;

public partial class SocketHttpListener : BunkumHttpListener
{
    private System.Net.Sockets.Socket? _socket;
    private readonly Uri _listenEndpoint;
    private readonly bool _useForwardedIp;
    
    [GeneratedRegex("^[a-zA-Z]+$")]
    private static partial Regex LettersRegex();

    public SocketHttpListener(Uri listenEndpoint, bool useForwardedIp, Logger logger) : base(logger)
    {
        this._listenEndpoint = listenEndpoint;
        this._useForwardedIp = useForwardedIp;
        
        this.Logger.LogInfo(ListenerCategory.Startup, "Internal HTTP server is listening at URL " + listenEndpoint);
    }
    
    public override void StartListening()
    {
        if (this._socket != null) throw new InvalidOperationException("Cannot start listening when we are already doing so");

        IPAddress host;
        
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (LettersRegex().IsMatch(this._listenEndpoint.Host)) // If host contains letters
            host = Dns.GetHostEntry(this._listenEndpoint.Host, AddressFamily.InterNetwork).AddressList[0];
        else
            host = IPAddress.Parse(this._listenEndpoint.Host);

        IPEndPoint listenEndpoint = new(host, this._listenEndpoint.Port);

        this._socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        this._socket.Bind(listenEndpoint);
        this._socket.Listen(128);
        
        this.Logger.LogInfo(ListenerCategory.Startup, "Listening...");
    }

    protected override async Task<ListenerContext?> WaitForConnectionAsyncInternal(CancellationToken? globalCt = null)
    {
        if (this._socket == null)
            throw new InvalidOperationException("Cannot wait for a connection when we are not listening");

        System.Net.Sockets.Socket client = await this._socket.AcceptAsync(globalCt ?? CancellationToken.None);
        NetworkStream stream = new(client);

        try
        {
            return this.ReadRequestIntoContext(client, stream);
        }
        catch (NotSupportedException e)
        {
            this.Logger.LogWarning(ListenerCategory.Request, $"Failed to handle request due to invalid HTTP version {e.Message}");
            return null;
        }
        catch(Exception e)
        {
            this.Logger.LogWarning(ListenerCategory.Request, $"Failed to read request: {e}");
            await new SocketHttpListenerContext(client).SendResponse(HttpStatusCode.BadRequest);
            return null;
        }
    }

    private ListenerContext ReadRequestIntoContext(System.Net.Sockets.Socket client, Stream stream)
    {
        Span<char> method = stackalloc char[RequestLineMethodLimit];
        Span<char> path = stackalloc char[RequestLinePathLimit];
        Span<char> version = stackalloc char[RequestLineVersionLimit];

        try
        {
            // Read method
            int read = stream.ReadIntoBufferUntilChar(' ', method);
            method = method[..read];
            
            // Read path
            read = stream.ReadIntoBufferUntilChar(' ', path);
            path = path[..read];
            
            // Read version
            read = stream.ReadIntoBufferUntilChar('\r', version);
            version = version[..read];

            stream.ReadByte(); // skip \n after \r
        }
        catch (Exception e)
        {
            throw new Exception("Failed to read request line. Maybe you tried to connect with HTTPS?", e);
        }

        ListenerContext context = new SocketHttpListenerContext(client)
        {
            RealRemoteEndpoint = (client.RemoteEndPoint as IPEndPoint)!,
        };

        HttpVersion httpVersion = version switch
        {
            "HTTP/1.0" => HttpVersion.Http1_0,
            "HTTP/1.1" => HttpVersion.Http1_1,
            _ => HttpVersion.Unknown,
        };

        if (httpVersion == HttpVersion.Unknown)
            throw new NotSupportedException(version.ToString());
        
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
        context.Protocol = httpVersion switch
        {
            HttpVersion.Http1_0 => HttpProtocolInformation.Http1_0,
            HttpVersion.Http1_1 => HttpProtocolInformation.Http1_1,
        };
#pragma warning restore CS8509

        context.Method = MethodUtils.FromString(typeof(HttpProtocolMethods), method);
        if (context.Method == Method.Invalid)
        {
            throw new Exception("Rejected request that sent invalid method " + method.ToString());
        }
        
        foreach ((string? key, string? value) in ReadHeaders(stream))
        {
            Debug.Assert(key != null);
            Debug.Assert(value != null);
            
            context.RequestHeaders.Add(key, value);
        }

        if (context.RequestHeaders["Host"] == null)
        {
            if (httpVersion >= HttpVersion.Http1_1)
            {
                throw new Exception("Rejected request without Host header");
            }

            context.RequestHeaders["Host"] = "localhost";
        }

        if (this._useForwardedIp && context.RequestHeaders["X-Forwarded-For"] != null)
        {
            string forwardedFor = context.RequestHeaders["X-Forwarded-For"]!.Split(',', 2)[0];
            
            if (forwardedFor.Contains(':')) // if IPV6, surround in brackets to support parsing
                forwardedFor = '[' + forwardedFor + ']';

            string forwardedIp = $"{forwardedFor}:{context.RealRemoteEndpoint.Port}";
            bool result = IPEndPoint.TryParse(forwardedIp, out IPEndPoint? endPoint);

            if (!result)
            {
                throw new Exception($"Rejected request from proxy that sent invalid IP '{forwardedIp}'");
            }
            
            Debug.Assert(endPoint != null);

            context.RemoteEndpoint = endPoint;
        }
        else
        {
            context.RemoteEndpoint = context.RealRemoteEndpoint;
        }

        // skip nullable warning since we have already asserted that this header exists
        string host = context.RequestHeaders.GetValues("Host")!.First();
        
        context.Uri = new Uri($"http://{host}{path}", UriKind.Absolute);
        
        if (context.RequestHeaders["Cookie"] != null)
        {
            foreach ((string? key, string? value) in ReadCookies(context.RequestHeaders["Cookie"]))
            {
                Debug.Assert(key != null);
                Debug.Assert(value != null);
                
                context.Cookies.Add(key, value);
            }
        }

        context.Query = HttpUtility.ParseQueryString(context.Uri.Query);

        MemoryStream inputStream = new((int)context.ContentLength);
        if (context.ContentLength > 0)
        {
            stream.ReadIntoStream(inputStream, (int)context.ContentLength);
            inputStream.Seek(0, SeekOrigin.Begin);
        }
        context.InputStream = inputStream;

        return context;
    }

    public override void Dispose()
    {
        this._socket?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
