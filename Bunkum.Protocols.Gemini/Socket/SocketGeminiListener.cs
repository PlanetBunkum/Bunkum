using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Bunkum.Listener;
using Bunkum.Listener.Extensions;
using Bunkum.Listener.Protocol;
using Bunkum.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.Protocols.Gemini.Socket;

public partial class SocketGeminiListener : BunkumGeminiListener
{
    private System.Net.Sockets.Socket? _socket;
    private readonly Uri _listenEndpoint;
    private readonly X509Certificate2 _cert;

    [GeneratedRegex("^[a-zA-Z]+$")]
    private static partial Regex LettersRegex();
    
    public SocketGeminiListener(X509Certificate2 cert, Uri listenEndpoint, Logger logger) : base(logger)
    {
        this._listenEndpoint = listenEndpoint;
        this._cert = cert;
   
        this.Logger.LogInfo(ListenerCategory.Startup, "Internal Gemini server is listening at URL {0}", listenEndpoint);
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

        return await this.ReadRequestIntoContext(client, stream);
    }

    private static string GetPath(Stream stream)
    {
        const int maxUriSize = 1024;
        
        //Allocate the buffer size needed for the whole URI + \r\n
        Span<byte> buffer = stackalloc byte[maxUriSize + 2];
        int read = stream.ReadIntoBufferUntilChar('\n', buffer);
        //Slice the buffer, removing the \r
        buffer = buffer[..(read - 1)];

        if (buffer.Length > 1024)
            throw new FormatException("Client sent too long of a URI");

        if (buffer[0] == 0xFE && buffer[1] == 0xFF || buffer[0] == 0xFF && buffer[1] == 0xFE)
            throw new FormatException("Client sent illegal byte order mark");

        return Encoding.UTF8.GetString(buffer);
    }

    private async Task<ListenerContext> ReadRequestIntoContext(System.Net.Sockets.Socket client, Stream rawStream)
    {
        SslStream stream = new(rawStream);

        await stream.AuthenticateAsServerAsync(
            this._cert,
            false,
            SslProtocols.Tls12 | SslProtocols.Tls13,
            false
        );

        Uri uri = new(GetPath(stream));
        
        return new SocketGeminiListenerContext(client, stream)
        {
            RealRemoteEndpoint = (client.RemoteEndPoint as IPEndPoint)!,
            RemoteEndpoint = (client.RemoteEndPoint as IPEndPoint)!,
            Method = Method.Invalid,
            Protocol = GeminiProtocolInformation.Gemini,
            InputStream = new MemoryStream(0),
            Uri = uri,
        };
    }
}