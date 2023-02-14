using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
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
        
        this._logger.LogTrace(HttpLogContext.Request, "Waiting for connection...");

        Socket client = await this._socket.AcceptAsync();
        this._logger.LogDebug(HttpLogContext.Request, "Client connected from " + client.RemoteEndPoint);

        NetworkStream stream = new(client);

        string[] requestLineSplit = ReadRequestLine(stream);
        string method = requestLineSplit[0];
        string path = requestLineSplit[1];
        string version = requestLineSplit[2].TrimEnd('\0').TrimEnd('\r');

        ListenerContext context = new(client, stream)
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

        Method parsedMethod = MethodUtils.FromString(method);
        if (parsedMethod == Method.Invalid)
        {
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

        if (!context.RequestHeaders.ContainsKey("Host"))
        {
            await context.SendResponse(HttpStatusCode.BadRequest);
            return null;
        }
        
        context.Uri = new Uri($"http://{context.RequestHeaders["Host"]}{path}", UriKind.Absolute);

        return context;
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