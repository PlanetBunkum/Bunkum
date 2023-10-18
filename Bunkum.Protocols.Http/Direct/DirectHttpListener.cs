using System.Diagnostics;
using System.Net;
using System.Web;
using Bunkum.Listener;
using Bunkum.Listener.Protocol;
using Bunkum.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.Protocols.Http.Direct;

public class DirectHttpListener : BunkumHttpListener, IListenerWithCallback
{
    public Action<ListenerContext>? Callback { get; set; }

    public DirectHttpListener(Logger logger) : base(logger)
    {}

    public HttpClient GetClient()
    {
        return new HttpClient(new BunkumHttpMessageHandler(this))
        {
            BaseAddress = new Uri("http://direct/"),
        };
    }

    internal void EnqueueMessage(DirectHttpMessage message)
    {
        if (this.Callback == null)
            throw new InvalidOperationException("The callback was not initialized for this listener.");
        
        ListenerContext? context = HandleMessage(message).Result;
        if (context == null) return;
        
        this.Callback(context);
    }

    public override void StartListening()
    {
        // No initialization required
    }

    private static async Task<ListenerContext?> HandleMessage(DirectHttpMessage? message)
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(5_000);
        CancellationToken ct = cts.Token;

        Debug.Assert(message != null);

        MemoryStream stream;
        Stream? requestStream = null;

        if (message.Message.Content != null)
            requestStream = await message.Message.Content.ReadAsStreamAsync(ct);

        if (requestStream != null && requestStream.Length != 0)
        {
            stream = new MemoryStream((int)requestStream.Length);

            await requestStream.CopyToAsync(stream, (int)requestStream.Length, ct);
            stream.Position = 0;
        }
        else
        {
            stream = new MemoryStream(0);
        }

        ListenerContext context = new DirectHttpListenerContext(message.Stream, message.Reset)
        {
            Uri = message.Message.RequestUri!,
            Method = MethodUtils.FromString(typeof(HttpProtocolMethods), message.Message.Method.Method),
            Query = HttpUtility.ParseQueryString(message.Message.RequestUri!.Query),
            RemoteEndpoint = IPEndPoint.Parse("0.0.0.0"),
            InputStream = stream,
        };

        foreach ((string? key, IEnumerable<string>? values) in message.Message.Headers)
        {
            Debug.Assert(key != null);
            Debug.Assert(values != null);

            //Special case for User-Agent because microsoft sucks and splits up user agents by spaces >:(
            if (key.Equals("User-Agent", StringComparison.InvariantCultureIgnoreCase))
                context.RequestHeaders.Add(key, string.Join(" ", values));
            else
                foreach (string value in values)
                    context.RequestHeaders.Add(key, value);
        }

        string? cookieHeader = context.RequestHeaders["Cookie"];
        if (cookieHeader != null)
        {
            foreach ((string? key, string? value) in ReadCookies(context.RequestHeaders["Cookie"]))
            {
                Debug.Assert(key != null);
                Debug.Assert(value != null);

                context.Cookies.Add(key, value);
            }
        }

        return context;
    }

    protected override Task<ListenerContext?> WaitForConnectionAsyncInternal(CancellationToken? globalCt = null)
    {
        return Task.FromResult<ListenerContext?>(null);
    }
}