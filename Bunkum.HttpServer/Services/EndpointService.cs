using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;
using NotEnoughLogs;

namespace Bunkum.HttpServer.Services;

/// <summary>
/// A type of service that can expose itself as a parameter to endpoints.
/// </summary>
public abstract class EndpointService : Service
{
    protected EndpointService(LoggerContainer<BunkumContext> logger) : base(logger)
    {}

    public override object? AddParameterToEndpoint(ListenerContext context, ParameterInfo paramInfo, Lazy<IDatabaseContext> database)
    {
        if (paramInfo.ParameterType.IsAssignableTo(this.GetType()))
            return this;

        return null;
    }
}