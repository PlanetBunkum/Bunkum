using System.Reflection;
using Bunkum.Core.Database;
using Bunkum.Core.Responses;
using Bunkum.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.Core.Services;

public abstract class Service
{
    protected readonly Logger Logger;
    
    protected internal Service(Logger logger)
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
    /// Runs after the request has been handled by the main middleware.
    /// May also be called when a response fails to generate due to an exception. A null response is passed in this case.
    /// </summary>
    public virtual void AfterRequestHandled(ListenerContext context, Response? response, MethodInfo method, Lazy<IDatabaseContext> database)
    {}
    
    /// <summary>
    /// Called when the endpoint is looking for a custom parameter. You can provide one using this method.
    /// </summary>
    /// <param name="context">The context of the request.</param>
    /// <param name="parameter">Information about the parameter being processed.</param>
    /// <param name="database">The database.</param>
    /// <returns>Null if this service has no suitable object for this parameter, otherwise the object to add.</returns>
    [Obsolete($"Using {nameof(ParameterInfo)} is no longer supported. Please switch to using {nameof(BunkumParameterInfo)} (another overload of this method)")]
    public virtual object? AddParameterToEndpoint(ListenerContext context, ParameterInfo parameter, Lazy<IDatabaseContext> database)
    {
        return this.AddParameterToEndpoint(context, new BunkumParameterInfo(parameter), database);
    }

    /// <summary>
    /// Called when the endpoint is looking for a custom parameter. You can provide one using this method.
    /// </summary>
    /// <param name="context">The context of the request.</param>
    /// <param name="parameter">Information about the parameter being processed.</param>
    /// <param name="database">The database.</param>
    /// <returns>Null if this service has no suitable object for this parameter, otherwise the object to add.</returns>
    public virtual object? AddParameterToEndpoint(ListenerContext context, BunkumParameterInfo parameter, Lazy<IDatabaseContext> database)
    {
        return null;
    }

    /// <summary>
    /// Checks if the parameter is assignable to a type.
    /// </summary>
    /// <param name="paramInfo">The parameter in question.</param>
    /// <typeparam name="TExtendable">The type, usually an interface or abstract class.</typeparam>
    /// <returns>true if the parameter's type extends the other type; false if not.</returns>
    protected static bool ParameterBasedFrom<TExtendable>(BunkumParameterInfo paramInfo)
        => paramInfo.ParameterType.IsAssignableTo(typeof(TExtendable));

    /// <summary>
    /// Checks if the parameter is equal to the type.
    /// </summary>
    /// <param name="paramInfo">The parameter in question.</param>
    /// <typeparam name="TOther">The type to check against.</typeparam>
    /// <returns>true if the parameter's type is equal to the other type; false if not.</returns>
    protected static bool ParameterEqualTo<TOther>(BunkumParameterInfo paramInfo)
        => paramInfo.ParameterType == typeof(TOther);
}