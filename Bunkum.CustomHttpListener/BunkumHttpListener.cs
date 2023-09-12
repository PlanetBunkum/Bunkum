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

    private const int HeaderLineLimit = 1024; // 1KB per cookieHeader
    protected const int RequestLineMethodLimit = 16; // bytes
    protected const int RequestLinePathLimit = 128; // bytes
    protected const int RequestLineVersionLimit = 16; // bytes

    protected BunkumHttpListener(bool logToConsole)
    {
        this.Logger = new LoggerContainer<HttpLogContext>();
        if(logToConsole) this.Logger.RegisterLogger(new ConsoleLogger());
    }

    public abstract void StartListening();

    public async Task WaitForConnectionAsync(Func<ListenerContext, Task> action, CancellationToken? globalCt = null)
    {
        while (globalCt is not { IsCancellationRequested: true })
        {
            ListenerContext? request = null;
            try
            {
                request = await this.WaitForConnectionAsyncInternal(globalCt);
            }
            catch(OperationCanceledException)
            {
                // ignore
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

    protected abstract Task<ListenerContext?> WaitForConnectionAsyncInternal(CancellationToken? globalCt = null);

    internal static IEnumerable<(string, string)> ReadCookies(ReadOnlySpan<char> cookieHeader)
    {
        List<(string key, string value)> cookies = new(10);

        bool parsedName = false;
        
        int nameIndex = 0;
        int dataIndex = 0;
        int startIndex = 0;
        
        for (int cookieIndex = 0; cookieIndex < cookieHeader.Length; cookieIndex++)
        {
            char c = cookieHeader[cookieIndex];
            if (!parsedName)
            {
                if (c != '=')
                {
                    nameIndex = cookieIndex + 1;
                    continue;
                }
                
                parsedName = true;
                dataIndex = cookieIndex;
            }


            bool isLastByte = cookieIndex == cookieHeader.Length - 1;
            if (c == ';' || isLastByte)
            {
                if (isLastByte) dataIndex++;

                ReadOnlySpan<char> nameSlice = cookieHeader[startIndex..nameIndex].TrimStart();
                ReadOnlySpan<char> dataSlice = cookieHeader[(nameIndex + 1)..dataIndex].TrimEnd();
                
                cookies.Add((nameSlice.ToString(), dataSlice.ToString()));
                startIndex = cookieIndex + 1;
                parsedName = false;
            }
            else
            {
                dataIndex++;
            }
        }

        return cookies;
    }

    internal static IEnumerable<(string key, string value)> ReadHeaders(Stream stream)
    {
        List<(string key, string value)> headers = new(10);
        Span<byte> headerLineBytes = stackalloc byte[HeaderLineLimit];
        
        while (true)
        {
            int count = stream.ReadIntoBufferUntilChar('\r', headerLineBytes);
            stream.ReadByte(); // Skip \n

            string headerLine = Encoding.UTF8.GetString(headerLineBytes[..count]);
            int index = headerLine.IndexOf(": ", StringComparison.Ordinal);
            if(index == -1) break; // no more headers

            string key = headerLine.Substring(0, index);
            string value = headerLine.Substring(index + 2);

            headers.Add((key, value));
        }

        return headers;
    }

    public virtual void Dispose() {}
}