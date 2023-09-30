using System.Diagnostics;
using System.Net;
using Bunkum.Core.Listener.Extensions;
using HttpListenerContext = Bunkum.Core.Listener.Request.Http.HttpListenerContext;

namespace Bunkum.Core.Listener.Listeners.Direct;

public class BunkumHttpMessageHandler : HttpMessageHandler
{
    private readonly DirectHttpListener _listener;

    public BunkumHttpMessageHandler(DirectHttpListener listener)
    {
        this._listener = listener;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken ct)
    {
        return this.SendAsync(request, ct).Result;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken ct)
    {
        MemoryStream stream = new();
        ManualResetEventSlim reset = new(false);
        this._listener.EnqueueMessage(new DirectHttpMessage(request, stream, reset));

        // Wait for the signal that tells us the server has finished responding
        reset.Wait(ct);
        stream.Position = 0;

        return ParseResponseMessage(stream);
    }

    private static HttpResponseMessage ParseResponseMessage(Stream stream)
    {
        // Parse the response line (HTTP/1.1 200 OK)
        // Skip version (HTTP/1.1)
        stream.SkipBufferUntilChar(' ');

        // status code can only be 3 bytes. 2xx, 3xx, 4xx, etc.
        Span<char> statusBytes = stackalloc char[3];
        stream.ReadIntoBufferUntilChar(' ', statusBytes);

        ushort status = ushort.Parse(statusBytes); 

        HttpStatusCode statusCode = (HttpStatusCode)status;
        HttpResponseMessage response = new(statusCode);
        
        // Skip to headers
        stream.SkipBufferUntilChar('\n');

        string contentLengthStr = "0";
        
        foreach ((string? key, string? value) in BunkumHttpListener.ReadHeaders(stream))
        {
            Debug.Assert(key != null);
            Debug.Assert(value != null);

            if (key == "Content-Length") contentLengthStr = value;

            response.Headers.TryAddWithoutValidation(key, value);
        }

        // int count = (int)(stream.Length - stream.Position);
        Debug.Assert(contentLengthStr != null);
        int count = int.Parse(contentLengthStr);
            
        MemoryStream inputStream = new(count);
        if (count > 0)
        {
            stream.ReadIntoStream(inputStream, count);
            inputStream.Position = 0;
        }
        
        response.Content = new StreamContent(inputStream);

        return response;
    }
}