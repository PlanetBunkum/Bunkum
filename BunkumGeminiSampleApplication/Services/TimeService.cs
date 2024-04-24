using System.Reflection;
using Bunkum.Core;
using Bunkum.Core.Database;
using Bunkum.Core.Services;
using Bunkum.Listener.Request;
using NotEnoughLogs;

namespace BunkumGeminiSampleApplication.Services;

/// <summary>
/// A service that provides time to endpoints.
/// </summary>
public class TimeService : Service
{
    // This constructor takes in the same dependencies an Endpoint can, so for example you could include ExampleConfiguration
    // as a parameter and it would be passed in. The only requirement is the logger, similar to how Endpoints require a RequestContext.
    internal TimeService(Logger logger) : base(logger)
    {}
    
    public override object? AddParameterToEndpoint(ListenerContext context, BunkumParameterInfo parameter, Lazy<IDatabaseContext> database)
    {
        this.Logger.LogDebug(BunkumCategory.Service, $"TimeService is attempting to pass something in for `{parameter.ParameterType.Name} {parameter.Name}`");
        if (parameter.ParameterType == typeof(DateTimeOffset))
        {
            this.Logger.LogDebug(BunkumCategory.Service, "Matched! Passing the time in.");
            return DateTimeOffset.Now;
        }

        this.Logger.LogDebug(BunkumCategory.Service, "No dice. Won't pass anything in for this parameter.");
        return base.AddParameterToEndpoint(context, parameter, database);
    }
}