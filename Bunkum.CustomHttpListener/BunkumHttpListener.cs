using System.Net;
using System.Net.Sockets;
using System.Text;
using NotEnoughLogs;
using NotEnoughLogs.Loggers;

namespace Bunkum.CustomHttpListener;

public class BunkumHttpListener : IDisposable
{
    private readonly Uri _listenEndpoint;
    private readonly LoggerContainer<HttpLogContext> _logger;
    private Socket? _socket;

    private const int HeaderLimit = 1024 * 8; // 8KB of header data
    
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

    public async Task WaitForConnectionAsync()
    {
        if (this._socket == null)
            throw new InvalidOperationException("Cannot wait for a connection when we are not listening");
        
        this._logger.LogTrace(HttpLogContext.Request, "Waiting for connection...");

        using Socket client = await this._socket.AcceptAsync();
        this._logger.LogDebug(HttpLogContext.Request, "Client connected from " + client.RemoteEndPoint);
        
        await client.SendAsync("HTTP/1.1 200 OK\r\n"u8.ToArray(), SocketFlags.None);
        client.Close();
    }
    
    public void Dispose()
    {
        this._socket?.Dispose();
    }
}