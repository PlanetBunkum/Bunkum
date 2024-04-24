using System.Reflection;
using Bunkum.Core.Database;
using Bunkum.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.Core.Services;

/// <summary>
/// A type of service that can expose itself as a parameter to endpoints.
/// </summary>
public abstract class EndpointService : Service
{
    protected EndpointService(Logger logger) : base(logger)
    {}
    
    /// <inheritdoc/>
    public override object? AddParameterToEndpoint(ListenerContext context, BunkumParameterInfo parameter, Lazy<IDatabaseContext> database)
    {
        if (parameter.ParameterType.IsAssignableTo(this.GetType()))
            return this;

        return null;
    }
}