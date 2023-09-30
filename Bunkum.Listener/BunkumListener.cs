using System.Net;
using Bunkum.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.Listener;

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

    /// <summary>
    /// The maximum length a request header can have.
    /// </summary>
    protected const int HeaderLineLimit = 1024; // 1KB per cookieHeader
    
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
                this.Logger.LogError(ListenerCategory.Request, "Failed to handle a connection: " + e);
                if(request != null) await request.SendResponse(HttpStatusCode.BadRequest);
                else this.Logger.LogWarning(ListenerCategory.Request, "Couldn't inform the client of the issue due to the request being null.");
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

    public virtual void Dispose() {}
}