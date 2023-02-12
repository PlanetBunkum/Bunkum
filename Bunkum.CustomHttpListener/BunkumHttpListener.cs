using System.Net;
using System.Net.Sockets;
using System.Text;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.CustomHttpListener.Request;
using NotEnoughLogs;
using NotEnoughLogs.Loggers;

namespace Bunkum.CustomHttpListener;

public class BunkumHttpListener : IDisposable
{
    private readonly Uri _listenEndpoint;
    private readonly LoggerContainer<HttpLogContext> _logger;
    private Socket? _socket;

    private const int HeaderLimit = 1024 * 8; // 8KB of header data
    private const int RequestLineLimit = 256; // 256 bytes
    
    public BunkumHttpListener(Uri listenEndpoint)
    {
        this._listenEndpoint = listenEndpoint;
        this._logger = new LoggerContainer<HttpLogContext>();
        this._logger.RegisterLogger(new ConsoleLogger());
        
        this._logger.LogInfo(HttpLogContext.Startup, "Internal server is listening at URL " + listenEndpoint);
        this._logger.LogInfo(HttpLogContext.Startup, "Do not use the above URL to patch!");
    }

    public void StartListening()
    {
        if (this._socket != null) throw new InvalidOperationException("Cannot start listening when we are already doing so");

        IPAddress host = Dns.GetHostEntry(this._listenEndpoint.Host, AddressFamily.InterNetwork).AddressList[0];
        IPEndPoint listenEndpoint = new(host, this._listenEndpoint.Port);

        this._socket = new Socket(listenEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        
        this._socket.Bind(listenEndpoint);
        this._socket.Listen(128);
        
        this._logger.LogInfo(HttpLogContext.Startup, "Listening...");
    }

    public async Task<ListenerContext> WaitForConnectionAsync()
    {
        while (true)
        {
            ListenerContext? request = await this.WaitForConnectionAsyncInternal();
            if (request != null) return request;
        }
    }

    private async Task<ListenerContext?> WaitForConnectionAsyncInternal()
    {
        if (this._socket == null)
            throw new InvalidOperationException("Cannot wait for a connection when we are not listening");
        
        this._logger.LogTrace(HttpLogContext.Request, "Waiting for connection...");

        using Socket client = await this._socket.AcceptAsync();
        this._logger.LogDebug(HttpLogContext.Request, "Client connected from " + client.RemoteEndPoint);

        await using NetworkStream stream = new(client);
        byte[] requestLineBytes = new byte[RequestLineLimit];

        int readByte;
        int i = 0;
        while ((readByte = stream.ReadByte()) != -1)
        {
            if ((char)readByte == '\n') break;

            requestLineBytes[i] = (byte)readByte;
            i++;
        }

        string[] requestLineSplit = Encoding.ASCII.GetString(requestLineBytes).Split(' ');
        string method = requestLineSplit[0];
        string path = requestLineSplit[1];
        string version = requestLineSplit[2].TrimEnd('\0').TrimEnd('\r');

        ListenerContext context = new(client)
        {
            ResponseHeaders =
            {
                ["Server"] = "Bunkum",
                ["Connection"] = "close",
            },
        };

        if (version != "HTTP/1.1")
        {
            await context.SendResponse(HttpStatusCode.HttpVersionNotSupported);
            return null;
        }
        
        context.Uri = new Uri(path, UriKind.Absolute);

        Method parsedMethod = MethodUtils.FromString(method);
        if (parsedMethod == Method.Invalid)
        {
            await context.SendResponse(HttpStatusCode.BadRequest);
            return null;
        }

        context.Method = parsedMethod;

        return context;
    }

    public void Dispose()
    {
        this._socket?.Dispose();
    }
}