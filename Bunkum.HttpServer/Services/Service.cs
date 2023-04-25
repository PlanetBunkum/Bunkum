using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Responses;
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
    public virtual Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        return null;
    }

    /// <summary>
    /// Called when the endpoint is looking for a custom parameter. You can provide one using this method.
    /// </summary>
    /// <param name="context">The context of the request.</param>
    /// <param name="paramInfo">Information about the parameter being processed.</param>
    /// <param name="database">The database.</param>
    /// <returns>Null if this service has no suitable object for this parameter, otherwise the object to add.</returns>
    public virtual object? AddParameterToEndpoint(ListenerContext context, ParameterInfo paramInfo, Lazy<IDatabaseContext> database)
    {
        return null;
    }
}