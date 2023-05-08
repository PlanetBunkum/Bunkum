using System.Net;
using System.Text;

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

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(this.Send(request, cancellationToken));
    }

    private static HttpResponseMessage ParseResponseMessage(Stream stream)
    {
        byte[] buffer = new byte[stream.Length];
        _ = stream.Read(buffer);
        string responseStr = Encoding.ASCII.GetString(buffer);
        
        string[] lines = responseStr.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        string[] responseLine = lines[0].Split(' ');
        
        HttpStatusCode statusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), responseLine[1]);
        HttpResponseMessage response = new(statusCode);
        
        // skip first line, then parse rest of response as headers
        for (int i = 1; i < lines.Length; i++)
        {
            string[] header = lines[i].Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
            if (header.Length == 2)
            {
                response.Headers.TryAddWithoutValidation(header[0], header[1]);
            }
        }
        
        response.Content = new StreamContent(stream);

        return response;
    }
}