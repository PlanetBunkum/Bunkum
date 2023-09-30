using System.Net;
using System.Text;
using Bunkum.Core.Listener.Extensions;
using Bunkum.Core.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.Core.Listener;

/// <summary>
/// A listener service used by a BunkumServer.
/// This probably isn't what you're looking for, unless you're looking to add protocol support to Bunkum. 
/// </summary>
public abstract class BunkumListener<TListenerContext, TStatusCode, TProtocolVersion, TProtocolMethod> : IDisposable
    where TStatusCode : Enum
    where TProtocolVersion : Enum
    where TProtocolMethod : Enum
    where TListenerContext : ListenerContext<TStatusCode, TProtocolVersion, TProtocolMethod>
{
    /// <summary>
    /// The logger.
    /// </summary>
    protected readonly Logger Logger;

    /// <summary>
    /// The maximum length a header can have.
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
    public async Task WaitForConnectionAsync(Func<TListenerContext, Task> action, CancellationToken? globalCt = null)
    {
        while (globalCt is not { IsCancellationRequested: true })
        {
            TListenerContext? request = null;
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
                if(request != null) await request.HandleInvalidRequest();
                else this.Logger.LogWarning(HttpLogCategory.Request, "Couldn't inform the client of the issue due to the request being null.");
                continue;
            }
            
            if (request == null) continue;
            
            await action.Invoke(request);
            await request.HandleNoEndpoint();
            return;
        }
    }

    /// <summary>
    /// Listen for an individual request and parses it.
    /// </summary>
    /// <param name="globalCt">A cancellation token to allow for stopping listening.</param>
    /// <returns>A valid <see cref="TListenerContext"/> if the request is valid, null otherwise.</returns>
    protected abstract Task<TListenerContext?> WaitForConnectionAsyncInternal(CancellationToken? globalCt = null);

    public virtual void Dispose() {}
}