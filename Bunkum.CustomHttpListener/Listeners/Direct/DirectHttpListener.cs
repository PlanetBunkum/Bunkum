using System.Diagnostics;
using System.Net;
using System.Web;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.CustomHttpListener.Request;

namespace Bunkum.CustomHttpListener.Listeners.Direct;

public class DirectHttpListener : BunkumHttpListener
{
    private readonly Queue<DirectHttpMessage> _messages = new();

    public HttpClient GetClient()
    {
        return new HttpClient(new BunkumHttpMessageHandler(this))
        {
            BaseAddress = new Uri("http://direct/"),
        };
    }

    internal void EnqueueMessage(DirectHttpMessage message)
    {
        this._messages.Enqueue(message);
    }

    public override void StartListening()
    {
        // No initialization required
    }

    protected override async Task<ListenerContext?> WaitForConnectionAsyncInternal()
    {
        while (true)
        {
            bool gotMessage = this._messages.TryDequeue(out DirectHttpMessage? message);
            if (gotMessage)
            {
                Debug.Assert(message != null);

                MemoryStream stream;
                Stream? requestStream = message.Message.Content?.ReadAsStream();

                if (requestStream != null && requestStream.Length != 0)
                {
                    stream = new MemoryStream((int)requestStream.Length);

                    await requestStream.CopyToAsync(stream, (int)requestStream.Length);
                    stream.Position = 0;
                }
                else
                {
                    stream = new MemoryStream(0);
                }

                ListenerContext context = new DirectListenerContext(message.Stream, message.Reset)
                {
                    Uri = message.Message.RequestUri!,
                    Method = MethodUtils.FromString(message.Message.Method.Method),
                    Query = HttpUtility.ParseQueryString(message.Message.RequestUri!.Query),
                    RemoteEndpoint = IPEndPoint.Parse("0.0.0.0"),
                    InputStream = stream,
                };

                foreach ((string? key, IEnumerable<string>? values) in message.Message.Headers)
                {
                    Debug.Assert(key != null);
                    Debug.Assert(values != null);

                    foreach (string value in values) context.RequestHeaders.Add(key, value);
                }

                foreach ((string? key, string? value) in ReadCookies(context.RequestHeaders["Cookie"]))
                {
                    Debug.Assert(key != null);
                    Debug.Assert(value != null);

                    context.Cookies.Add(key, value);
                }

                return context;
            }

            Thread.Sleep(10);
        }
    }
}