using NotEnoughLogs;

namespace Bunkum.HttpServer.Services;

public abstract class Service
{
    protected readonly LoggerContainer<BunkumContext> Logger;
    
    protected internal Service(LoggerContainer<BunkumContext> logger)
    {
        this.Logger = logger;
    }
    
    /// <summary>
    /// Runs when startup tasks are run.
    /// </summary>
    public virtual void Initialize()
    {
        
    }

    /// <summary>
    /// Runs when the request is about to be handled by the main middleware.
    /// </summary>
    public virtual void OnRequestHandled()
    {
        
    }
}