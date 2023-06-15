using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Web;
using Bunkum.CustomHttpListener.Extensions;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.CustomHttpListener.Request;

namespace Bunkum.CustomHttpListener.Listeners;

public partial class SocketHttpListener : BunkumHttpListener
{
    private Socket? _socket;
    private readonly Uri _listenEndpoint;
    private readonly bool _useForwardedIp;
    
    [GeneratedRegex("^[a-zA-Z]+$")]
    private static partial Regex LettersRegex();

    public SocketHttpListener(Uri listenEndpoint, bool useForwardedIp, bool logToConsole = true) : base(logToConsole)
    {
        this._listenEndpoint = listenEndpoint;
        this._useForwardedIp = useForwardedIp;
        
        this.Logger.LogInfo(HttpLogContext.Startup, "Internal server is listening at URL " + listenEndpoint);
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

        this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        this._socket.Bind(listenEndpoint);
        this._socket.Listen(128);
        
        this.Logger.LogInfo(HttpLogContext.Startup, "Listening...");
    }

    protected override async Task<ListenerContext?> WaitForConnectionAsyncInternal()
    {
        if (this._socket == null)
            throw new InvalidOperationException("Cannot wait for a connection when we are not listening");

        Socket client = await this._socket.AcceptAsync(CancellationToken.None);
        NetworkStream stream = new(client);

        CancellationTokenSource cts = new();
        cts.CancelAfter(5_000);
        CancellationToken ct = cts.Token;

        string method;
        string path;
        string version;

        try
        {
            string[] requestLineSplit = await ReadRequestLine(stream, ct);

            method = requestLineSplit[0];
            path = requestLineSplit[1];
            version = requestLineSplit[2].TrimEnd('\0').TrimEnd('\r');
        }
        catch (Exception e)
        {
            this.Logger.LogError(HttpLogContext.Request, "Failed to read request line: " + e);
            await new SocketListenerContext(client).SendResponse(HttpStatusCode.BadRequest);
            return null;
        }

        ListenerContext context = new SocketListenerContext(client)
        {
            ResponseHeaders =
            {
                ["Server"] = "Bunkum",
                ["Connection"] = "close",
                ["Date"] = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"),
            },
            RealRemoteEndpoint = (client.RemoteEndPoint as IPEndPoint)!,
        };

        if (version != "HTTP/1.1")
        {
            await context.SendResponse(HttpStatusCode.HttpVersionNotSupported);
            return null;
        }

        Method parsedMethod = MethodUtils.FromString(method);
        if (parsedMethod == Method.Invalid)
        {
            this.Logger.LogWarning(HttpLogContext.Request, "Rejected request that sent invalid method " + method);
            await context.SendResponse(HttpStatusCode.BadRequest);
            return null;
        }

        context.Method = parsedMethod;

        // ReSharper disable once MethodSupportsCancellation
#pragma warning disable CA2016
        await foreach ((string? key, string? value) in ReadHeaders(stream).WithCancellation(ct))
#pragma warning restore CA2016
        {
            Debug.Assert(key != null);
            Debug.Assert(value != null);
            
            context.RequestHeaders.Add(key, value);
        }

        if (context.RequestHeaders["Host"] == null)
        {
            this.Logger.LogWarning(HttpLogContext.Request, "Rejected request without Host header");
            await context.SendResponse(HttpStatusCode.BadRequest);
            return null;
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
                this.Logger.LogWarning(HttpLogContext.Request, $"Rejected request from proxy that sent invalid IP '{forwardedIp}'");
                await context.SendResponse(HttpStatusCode.BadRequest);
                return null;
            }
            
            Debug.Assert(endPoint != null);

            context.RemoteEndpoint = endPoint;
        }
        else
        {
            context.RemoteEndpoint = context.RealRemoteEndpoint;
        }
        
        context.Uri = new Uri($"http://{context.RequestHeaders["Host"]}{path}", UriKind.Absolute);
        
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
            await stream.ReadIntoStreamAsync(inputStream, (int)context.ContentLength, ct);
            inputStream.Seek(0, SeekOrigin.Begin);
        }
        context.InputStream = inputStream;

        return context;
    }

    public override void Dispose()
    {
        this._socket?.Dispose();
        base.Dispose();
    }
}