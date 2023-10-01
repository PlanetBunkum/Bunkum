using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Bunkum.Listener;
using Bunkum.Listener.Extensions;
using Bunkum.Listener.Protocol;
using Bunkum.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.Protocols.Gopher.Socket;

public partial class SocketGopherListener : BunkumGopherListener
{
    private System.Net.Sockets.Socket? _socket;
    private readonly Uri _listenEndpoint;
    
    [GeneratedRegex("^[a-zA-Z]+$")]
    private static partial Regex LettersRegex();
    
    public SocketGopherListener(Uri listenEndpoint, Logger logger) : base(logger)
    {
        this._listenEndpoint = listenEndpoint;
        
        this.Logger.LogInfo(ListenerCategory.Startup, "Internal Gopher server is listening at URL {0}", listenEndpoint);
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
        
        return this.ReadRequestIntoContext(client, stream);
    }

    private static string GetPath(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[RequestLinePathLimit];
        int read = stream.ReadIntoBufferUntilChar('\n', buffer);
        buffer = buffer[..read];

        StringBuilder pathBuilder = new();
        if (buffer[0] != (byte)'/')
        {
            pathBuilder.Append('/');
        }

        pathBuilder.Append(Encoding.UTF8.GetString(buffer));
        return pathBuilder.ToString();
    }

    private ListenerContext ReadRequestIntoContext(System.Net.Sockets.Socket client, Stream stream)
    {
        string path = GetPath(stream);

        ListenerContext context = new SocketGopherListenerContext(client)
        {
            RealRemoteEndpoint = (client.RemoteEndPoint as IPEndPoint)!,
            RemoteEndpoint = (client.RemoteEndPoint as IPEndPoint)!,
            Method = Method.Invalid,
            Protocol = GopherProtocolInformation.Gopher,
            InputStream = new MemoryStream(0),
            Uri = new Uri($"gopher://{this._listenEndpoint.Host}:{this._listenEndpoint.Port}{path}", UriKind.Absolute),
        };

        return context;
    }
}