using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;

namespace BunkumSampleApplication.Services;

/// <summary>
/// A service that provides time to endpoints.
/// </summary>
public class TimeService : Service
{
    // This constructor takes in the same dependencies an Endpoint can, so for example you could include ExampleConfiguration
    // as a parameter and it would be passed in. The only requirement is the logger, similar to how Endpoints require a RequestContext.
    internal TimeService(Logger logger) : base(logger)
    {}
    
    public override object? AddParameterToEndpoint(ListenerContext context, ParameterInfo paramInfo, Lazy<IDatabaseContext> database)
    {
        this.Logger.LogDebug(BunkumCategory.Service, $"TimeService is attempting to pass something in for `{paramInfo.ParameterType.Name} {paramInfo.Name}`");
        if (paramInfo.ParameterType == typeof(DateTimeOffset))
        {
            this.Logger.LogDebug(BunkumCategory.Service, "Matched! Passing the time in.");
            return DateTimeOffset.Now;
        }

        this.Logger.LogDebug(BunkumCategory.Service, "No dice. Won't pass anything in for this parameter.");
        return base.AddParameterToEndpoint(context, paramInfo, database);
    }
}