using System.Net;
using System.Text;
using Bunkum.Core.Listener.Extensions;
using Bunkum.Core.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.Core.Listener;

/// <summary>
/// A custom HTTP socket listener used by Bunkum's HTTP server.
/// This probably isn't what you're looking for. 
/// </summary>
public abstract class BunkumListener : IDisposable
{
    /// <summary>
    /// The logger.
    /// </summary>
    protected readonly Logger Logger;

    private const int HeaderLineLimit = 1024; // 1KB per cookieHeader
    
    /// <summary>
    /// The maximum length the request line's method can have in bytes.
    /// </summary>
    protected const int RequestLineMethodLimit = 16; // bytes
    
    /// <summary>
    /// The maximum length the request line's path can have in bytes.
    /// </summary>
    protected const int RequestLinePathLimit = 128; // bytes
    
    /// <summary>
    /// The maximum length the request line's version can have in bytes.
    /// </summary>
    protected const int RequestLineVersionLimit = 16; // bytes

    /// <summary>
    /// Instantiates the listener, passing in the logger.
    /// </summary>
    /// <param name="logger">The logger to use for this listener.</param>
    protected BunkumListener(Logger logger)
    {
        this.Logger = logger;
    }

    /// <summary>
    /// Tells the listener to start listening for requests. Called after startup tasks have completed.
    /// </summary>
    public abstract void StartListening();

    /// <summary>
    /// Loop over <see cref="WaitForConnectionAsyncInternal"/> and wait for connections.
    /// Handles errors cleanly, and calls back when a valid request comes through.
    /// </summary>
    /// <param name="action">The code to run when a valid request is made via this listener.</param>
    /// <param name="globalCt">A cancellation token to allow for stopping listening.</param>
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
                this.Logger.LogError(HttpLogCategory.Request, "Failed to handle a connection: " + e);
                if(request != null) await request.SendResponse(HttpStatusCode.BadRequest);
                else this.Logger.LogWarning(HttpLogCategory.Request, "Couldn't inform the client of the issue due to the request being null.");
                continue;
            }
            
            if (request == null) continue;
            
            await action.Invoke(request);
            await request.SendResponse(HttpStatusCode.NotFound);
            return;
        }
    }

    /// <summary>
    /// Listen for an individual request and parses it.
    /// </summary>
    /// <param name="globalCt">A cancellation token to allow for stopping listening.</param>
    /// <returns>A valid <see cref="ListenerContext"/> if the request is valid, null otherwise.</returns>
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