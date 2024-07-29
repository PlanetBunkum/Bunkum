using System.Buffers.Text;
using System.Collections.Specialized;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Bunkum.Listener;
using Bunkum.Listener.Protocol;
using Bunkum.Listener.Request;
using Bunkum.Protocols.TlsSupport;
using NotEnoughLogs;

namespace Bunkum.Protocols.Gemini.Socket;

public partial class SocketGeminiListener : BunkumGeminiListener
{
    private System.Net.Sockets.Socket? _socket;
    private readonly Uri _listenEndpoint;
    private readonly X509Certificate2 _cert;
    private readonly SslConfiguration _sslConfiguration;
    private readonly GeminiBunkumConfig _config;
    private readonly Uri _externalUri;

    [GeneratedRegex("^[a-zA-Z]+$")]
    private static partial Regex LettersRegex();
    
    public SocketGeminiListener(X509Certificate2 cert, SslConfiguration sslConfiguration, Uri listenEndpoint, Logger logger, GeminiBunkumConfig config) : base(logger)
    {
        this._listenEndpoint = listenEndpoint;
        this._cert = cert;
        this._sslConfiguration = sslConfiguration;
        this._externalUri = new Uri(config.ExternalUrl);
        this._config = config;
   
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

    private record RequestData(string Uri, byte[]? Body, string? Token, string? Mime);
    
    /// <summary>
    /// Reads the absolute URI of the request
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <returns>The absolute URI sent by the client</returns>
    /// <exception cref="FormatException">The format of the request is invalid in some way</exception>
    private static RequestData ReadRequest(Stream stream)
    {
        // The max URI size as defined in the specification
        const int maxUriSize = 1024;
        
        // Allocate the buffer size needed for the max sized URI + \r\n
        Span<byte> buffer = stackalloc byte[maxUriSize + 2];                
        int read = stream.Read(buffer);
        
        // If the client sent no data, complain
        if (read == 0)
            throw new FormatException("Unexpected EOF when reading request.");
            
        // Throw an explicit format exception rather than an out of bounds read later on
        if (read < 2)
            throw new FormatException("Request too short, must be >= 2 bytes to contain the CRLF.");

        buffer = buffer[..read];

        int lineBreakIndex = buffer.IndexOf("\r\n"u8);
        if(lineBreakIndex == -1)
            throw new FormatException("URI truncated or missing CRLF.");
        
        // Enforce no BOM at start of request
        if (buffer[0] == 0xFE && buffer[1] == 0xFF || buffer[0] == 0xFF && buffer[1] == 0xFE)
            throw new FormatException("Client sent illegal BOM at start of URI.");

        Span<byte> uriBuffer = buffer[..lineBreakIndex];

        string? titanToken = null;
        int? titanSize = null;
        string? titanMime = null;
        
        int firstParamIndex = uriBuffer.IndexOf((byte)';');
        int urlQueryIndex = uriBuffer.IndexOf((byte)'?');
        if (firstParamIndex != -1)
        {
            Span<byte> titanParams = uriBuffer[(firstParamIndex + 1)..];

            // Strip out the `?query` at the end, if present
            if (urlQueryIndex != -1 && urlQueryIndex > firstParamIndex) 
                titanParams = titanParams[..(urlQueryIndex - firstParamIndex - 1)];
            
            while (true)
            {
                int nextParamIndex = titanParams.IndexOf((byte)';');
                Span<byte> currentTitanParams = titanParams[..(nextParamIndex == -1 ? titanParams.Length : nextParamIndex)];

                int splitIndex = currentTitanParams.IndexOf((byte)'=');
                if (splitIndex != -1)
                {
                    Span<byte> before = currentTitanParams[..splitIndex];
                    Span<byte> after = currentTitanParams[(splitIndex + 1)..];

                    if (before.SequenceEqual("token"u8)) titanToken = HttpUtility.UrlDecode(after.ToArray(), Encoding.UTF8);
                    else if (before.SequenceEqual("mime"u8)) titanMime = Encoding.UTF8.GetString(after);
                    else if (before.SequenceEqual("size"u8))
                    {
                        // Try to parse the titan size into an int
                        if (!Utf8Parser.TryParse(after, out int parsedTitanSize, out int _))
                            throw new FormatException("Titan size parameter was in an incorrect format.");

                        titanSize = parsedTitanSize;
                    }
                }

                if (nextParamIndex == -1)
                    break;
                
                titanParams = titanParams[(nextParamIndex + 1)..];
            }
            
            if (titanSize == null)
                throw new FormatException("Client did not send the size with the request.");
        }

        byte[] titanData = [];
        if (titanSize is > 0)
        {
            titanData = new byte[titanSize.Value];
            
            // Copy the body data we have already read
            Span<byte> preReadBody = buffer[(lineBreakIndex + 2)..];
            preReadBody.CopyTo(titanData.AsSpan()[..preReadBody.Length]);

            // This is the amount of data we have left to read
            int toRead = titanData.Length - preReadBody.Length;
            int totalRead = preReadBody.Length;
            Span<byte> bodyBuffer = stackalloc byte[2048];                
            while (toRead > 0)
            {
                read = stream.Read(bodyBuffer[..Math.Min(toRead, bodyBuffer.Length)]);
                
                Span<byte> readData = bodyBuffer[..read];
                readData.CopyTo(titanData.AsSpan()[totalRead..(totalRead + read)]);
                
                toRead -= read;
                totalRead += read;
            }
        }

        // If the URL query is after the titan parameters, we need to strip out the titan parameters from the URL before passing it back to Bunkum
        if (urlQueryIndex > firstParamIndex && firstParamIndex != -1)
        {
            Span<byte> preParams = buffer[..firstParamIndex];
            Span<byte> postParams = buffer[urlQueryIndex..lineBreakIndex];

            byte[] conglomerate = new byte[preParams.Length + postParams.Length];
            preParams.CopyTo(conglomerate);
            postParams.CopyTo(conglomerate.AsSpan()[preParams.Length..]);

            return new RequestData(Encoding.UTF8.GetString(conglomerate), titanData, titanToken, titanMime);
        }

        return new RequestData(Encoding.UTF8.GetString(uriBuffer), titanData, titanToken, titanMime);
    }

    private async Task<ListenerContext?> ReadRequestIntoContext(System.Net.Sockets.Socket client, Stream rawStream)
    {
        SslStream stream = new(rawStream);

        SslServerAuthenticationOptions authOptions = new()
        {
            EnabledSslProtocols = this._sslConfiguration.EnabledSslProtocols,
            ServerCertificate = this._cert,
            ClientCertificateRequired = false,  
            RemoteCertificateValidationCallback = (_, _, _, _) => true, 
        };

        // If the cipher suite set is enabled, and we are not on windows, enable the selected cipher suites
        if (this._sslConfiguration.EnabledCipherSuites != null && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            authOptions.CipherSuitesPolicy = new CipherSuitesPolicy(this._sslConfiguration.EnabledCipherSuites);
        
        await stream.AuthenticateAsServerAsync(authOptions);

        Uri uri;
        RequestData data;
        try
        {
            uri = new Uri((data = ReadRequest(stream)).Uri);

            if (!string.IsNullOrWhiteSpace(uri.UserInfo))
                throw new FormatException("URI User info is not allowed.");
        }
        catch(Exception ex) when (ex is UriFormatException or FormatException)
        {
            await new SocketGeminiListenerContext(client, stream).SendResponse(HttpStatusCode.BadRequest, Encoding.UTF8.GetBytes(ex.Message));
            return null;
        }

        if (uri.Scheme != "gemini" && uri.Scheme != "titan" && !this._config.AllowProxyRequests)
            throw new FormatException($"Unsupported scheme {uri.Scheme}");
        
        // While the spec says you SHOULD NOT allow IPv4/IPv6 addresses in the authority field, for flexibility, we allow this
        // Check to make sure the authority matches the defined external authority
        if (uri.HostNameType is not UriHostNameType.IPv4 and not UriHostNameType.IPv6 && !this._config.AllowProxyRequests)
        {
            bool bad = false;
                            
            // If the port is -1, then we need to have special handling to make this
            if (this._externalUri.Port == -1 && !this._externalUri.Host.Equals(uri.Host, StringComparison.InvariantCultureIgnoreCase))
                bad = true;
            else if (!this._externalUri.Authority.Equals(uri.Authority, StringComparison.InvariantCultureIgnoreCase))
                bad = true;

            if (bad)
            {
                await new SocketGeminiListenerContext(client, stream).SendResponse(HttpStatusCode.ProxyAuthenticationRequired, "This server does not support proxy requests."u8.ToArray());
                return null;
            }
        }
        
        string? query = uri.Query.Length > 0 ? uri.Query[1..] : null;

        SocketGeminiListenerContext context = new(client, stream)
        {
            RealRemoteEndpoint = (client.RemoteEndPoint as IPEndPoint)!,
            RemoteEndpoint = (client.RemoteEndPoint as IPEndPoint)!,
            Method = Method.Invalid,
            Protocol = GeminiProtocolInformation.Gemini,
            InputStream = new MemoryStream(0),
            Uri = uri,
            RemoteCertificate = stream.RemoteCertificate,
            Query = new NameValueCollection(),
        };

        if(query != null)
            context.Query["input"] = HttpUtility.UrlDecode(query);

        if (data.Body != null)
        {
            if (data.Token != null)
                context.Query["token"] = data.Token;

            if (data.Mime != null)
                context.RequestHeaders["Content-Type"] = data.Mime;

            context.InputStream = new MemoryStream(data.Body);
        }
        else
        {
            context.InputStream = new MemoryStream(0);
        }

        return context;
    }
}