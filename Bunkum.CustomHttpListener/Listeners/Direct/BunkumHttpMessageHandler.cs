using System.Diagnostics;
using System.Net;
using Bunkum.CustomHttpListener.Extensions;

namespace Bunkum.CustomHttpListener.Listeners.Direct;

public class BunkumHttpMessageHandler : HttpMessageHandler
{
    private readonly DirectHttpListener _listener;

    public BunkumHttpMessageHandler(DirectHttpListener listener)
    {
        this._listener = listener;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        MemoryStream stream = new();
        ManualResetEventSlim reset = new(false);
        this._listener.EnqueueMessage(new DirectHttpMessage(request, stream, reset));

        // Wait for the signal that tells us the server has finished responding
        reset.Wait(cancellationToken);
        stream.Position = 0;

        return ParseResponseMessage(stream);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(this.Send(request, cancellationToken));
    }

    private static HttpResponseMessage ParseResponseMessage(Stream stream)
    {
        string[] responseLine = BunkumHttpListener.ReadRequestLine(stream);

        HttpStatusCode statusCode = Enum.Parse<HttpStatusCode>(responseLine[1]);
        HttpResponseMessage response = new(statusCode);

        string contentLengthStr = null!;
        
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
        stream.ReadIntoStream(inputStream, count);
        
        inputStream.Seek(0, SeekOrigin.Begin);

        stream.Position = 0;
        response.Content = new StreamContent(inputStream);

        return response;
    }
}