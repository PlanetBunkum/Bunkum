using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;

namespace Bunkum.HttpServer.Health;

internal class HealthService : Service
{
    private readonly List<IHealthCheck> _healthChecks;

    internal HealthService(LoggerContainer<BunkumContext> logger, IEnumerable<IHealthCheck> checks) : base(logger)
    {
        this._healthChecks = checks.ToList();
    }

    public override object? AddParameterToEndpoint(ListenerContext context, ParameterInfo paramInfo, Lazy<IDatabaseContext> database)
    {
        // Only pass into endpoint inside Bunkum
        Assembly declaringAssembly = paramInfo.Member.DeclaringType!.Assembly;
        if (declaringAssembly.Equals(Assembly.GetAssembly(typeof(HealthService))))
        {
            if (paramInfo.ParameterType.IsAssignableFrom(typeof(IEnumerable<IHealthCheck>)))
                return this._healthChecks;
        }

        return base.AddParameterToEndpoint(context, paramInfo, database);
    }
}