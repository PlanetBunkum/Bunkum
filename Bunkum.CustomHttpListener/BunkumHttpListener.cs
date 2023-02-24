using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Bunkum.CustomHttpListener.Extensions;
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

    private const int HeaderLineLimit = 1024; // 1KB per header
    private const int RequestLineLimit = 256; // 256 bytes
    
    public BunkumHttpListener(Uri listenEndpoint)
    {
        this._listenEndpoint = listenEndpoint;
        this._logger = new LoggerContainer<HttpLogContext>();
        this._logger.RegisterLogger(new ConsoleLogger());
        
        this._logger.LogInfo(HttpLogContext.Startup, "Internal server is listening at URL " + listenEndpoint);
        this._logger.LogInfo(HttpLogContext.Startup, "The above URL is probably not the URL you should use to patch. " +
                                                     "See https://littlebigrefresh.github.io/Docs/patch-url for more information.");
    }

    public void StartListening()
    {
        if (this._socket != null) throw new InvalidOperationException("Cannot start listening when we are already doing so");

        IPAddress host;
        
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (Regex.IsMatch(this._listenEndpoint.Host, @"^[a-zA-Z]+$")) // If host contains letters
            host = Dns.GetHostEntry(this._listenEndpoint.Host, AddressFamily.InterNetwork).AddressList[0];
        else
            host = IPAddress.Parse(this._listenEndpoint.Host);

        IPEndPoint listenEndpoint = new(host, this._listenEndpoint.Port);

        this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        this._socket.Bind(listenEndpoint);
        this._socket.Listen(128);
        
        this._logger.LogInfo(HttpLogContext.Startup, "Listening...");
    }

    public async Task WaitForConnectionAsync(Func<ListenerContext, Task> action)
    {
        while (true)
        {
            ListenerContext? request = await this.WaitForConnectionAsyncInternal();
            if (request == null) continue;
            
            await action.Invoke(request);
            if(!request.SocketClosed) await request.SendResponse(HttpStatusCode.NotFound);
            return;
        }
    }

    private async Task<ListenerContext?> WaitForConnectionAsyncInternal()
    {
        if (this._socket == null)
            throw new InvalidOperationException("Cannot wait for a connection when we are not listening");

        Socket client = await this._socket.AcceptAsync();
        NetworkStream stream = new(client);

        string[] requestLineSplit = ReadRequestLine(stream);
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
            RemoteEndpoint = client.RemoteEndPoint!,
        };

        if (version != "HTTP/1.1")
        {
            await context.SendResponse(HttpStatusCode.HttpVersionNotSupported);
            return null;
        }

        Method parsedMethod = MethodUtils.FromString(method);
        if (parsedMethod == Method.Invalid)
        {
            this._logger.LogWarning(HttpLogContext.Request, "Rejected request that sent invalid method " + method);
            await context.SendResponse(HttpStatusCode.BadRequest);
            return null;
        }

        context.Method = parsedMethod;

        foreach ((string? key, string? value) in ReadHeaders(stream))
        {
            Debug.Assert(key != null);
            Debug.Assert(value != null);
            
            context.RequestHeaders.Add(key, value);
        }

        if (context.RequestHeaders["Host"] == null)
        {
            this._logger.LogWarning(HttpLogContext.Request, "Rejected request without Host header");
            await context.SendResponse(HttpStatusCode.BadRequest);
            return null;
        }
        
        context.Uri = new Uri($"http://{context.RequestHeaders["Host"]}{path}", UriKind.Absolute);
        
        if (context.RequestHeaders["Cookie"] != null)
        {
            foreach ((string? key, string? value) in ReadCookies(context.RequestHeaders["Cookie"]!))
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

    private static IEnumerable<(string, string)> ReadCookies(string header)
    {
        if (string.IsNullOrEmpty(header)) yield break;

        string[] pairs = header.Split(';');
        foreach (string pair in pairs)
        {
            int index = pair.IndexOf('=');
            if (index < 0) continue; // Pair is split by =, if we cant find it then this is obviously bad data

            string key = pair.Substring(0, index).TrimStart();
            string value = pair.Substring(index + 1).TrimEnd();

            yield return (key, value);
        }
    }

    private static string[] ReadRequestLine(Stream stream)
    {
        byte[] requestLineBytes = new byte[RequestLineLimit];
        // Probably breaks spec to just look for \n instead of \r\n but who cares
        stream.ReadIntoBufferUntilChar('\n', requestLineBytes);
        
        return Encoding.ASCII.GetString(requestLineBytes).Split(' ');
    }

    private static IEnumerable<(string, string)> ReadHeaders(Stream stream)
    {
        while (true)
        {
            byte[] headerLineBytes = new byte[HeaderLineLimit];
            int count = stream.ReadIntoBufferUntilChar('\n', headerLineBytes);

            string headerLine = Encoding.UTF8.GetString(headerLineBytes, 0, count);
            int index = headerLine.IndexOf(": ", StringComparison.Ordinal);
            if(index == -1) break; // no more headers

            string key = headerLine.Substring(0, index);
            string value = headerLine.Substring(index + 2).TrimEnd('\r');

            yield return (key, value);
        }
    }

    public void Dispose()
    {
        this._socket?.Dispose();
    }
}