using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Bunkum.CustomHttpListener.Extensions;
using Bunkum.CustomHttpListener.Request;
using NotEnoughLogs;
using NotEnoughLogs.Loggers;

namespace Bunkum.CustomHttpListener;

/// <summary>
/// A custom HTTP socket listener used by Bunkum's HTTP server.
/// This probably isn't what you're looking for. 
/// </summary>
public abstract class BunkumHttpListener : IDisposable
{
    protected readonly LoggerContainer<HttpLogContext> Logger;

    private const int HeaderLineLimit = 1024; // 1KB per header
    private const int RequestLineLimit = 256; // 256 bytes

    protected BunkumHttpListener(bool logToConsole)
    {
        this.Logger = new LoggerContainer<HttpLogContext>();
        if(logToConsole) this.Logger.RegisterLogger(new ConsoleLogger());
    }

    public abstract void StartListening();

    public async Task WaitForConnectionAsync(Func<ListenerContext, Task> action)
    {
        while (true)
        {
            ListenerContext? request = null;
            try
            {
                request = await this.WaitForConnectionAsyncInternal();
            }
            catch (Exception e)
            {
                this.Logger.LogError(HttpLogContext.Request, "Failed to handle a connection: " + e);
                if(request != null) await request.SendResponse(HttpStatusCode.BadRequest);
                else this.Logger.LogWarning(HttpLogContext.Request, "Couldn't inform the client of the issue due to the request being null.");
                continue;
            }
            
            if (request == null) continue;
            
            await action.Invoke(request);
            await request.SendResponse(HttpStatusCode.NotFound);
            return;
        }
    }

    protected abstract Task<ListenerContext?> WaitForConnectionAsyncInternal();

    internal static IEnumerable<(string, string)> ReadCookies(string? header)
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

    internal static async Task<string[]> ReadRequestLine(Stream stream, CancellationToken ct)
    {
        byte[] requestLineBytes = new byte[RequestLineLimit];
        // Probably breaks spec to just look for \n instead of \r\n but who cares
        await stream.ReadIntoBufferUntilCharAsync('\n', requestLineBytes, ct);
        
        return Encoding.ASCII.GetString(requestLineBytes).Split(' ');
    }

    internal static async IAsyncEnumerable<(string key, string value)> ReadHeaders(Stream stream, [EnumeratorCancellation] CancellationToken ct = default)
    {
        while (true)
        {
            byte[] headerLineBytes = new byte[HeaderLineLimit];
            int count = await stream.ReadIntoBufferUntilCharAsync('\n', headerLineBytes, ct);

            string headerLine = Encoding.UTF8.GetString(headerLineBytes, 0, count);
            int index = headerLine.IndexOf(": ", StringComparison.Ordinal);
            if(index == -1) break; // no more headers

            string key = headerLine.Substring(0, index);
            string value = headerLine.Substring(index + 2).TrimEnd('\r');

            yield return (key, value);
        }
    }

    public virtual void Dispose() {}
}