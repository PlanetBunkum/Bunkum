using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;

namespace Bunkum.HttpServer.Health;

public class HealthService : Service
{
    private readonly List<IHealthCheck> _healthChecks;

    public HealthService(LoggerContainer<BunkumContext> logger, IEnumerable<IHealthCheck> checks) : base(logger)
    {
        this._healthChecks = checks.ToList();
    }

    public override object? AddParameterToEndpoint(ListenerContext context, ParameterInfo paramInfo, Lazy<IDatabaseContext> database)
    {
        // Only pass into endpoint inside Bunkum
        Assembly declaringAssembly = paramInfo.Member.DeclaringType!.Assembly;
        if (declaringAssembly.Equals(Assembly.GetAssembly(typeof(HealthService))))
        {
            if (paramInfo.ParameterType.IsAssignableFrom(typeof(HealthReport)))
                return this.GenerateReport();
        }

        return base.AddParameterToEndpoint(context, paramInfo, database);
    }

    public HealthReport GenerateReport()
    {
        List<HealthStatus> statusList = this._healthChecks
            .Select(healthCheck => healthCheck.RunCheck() with {CheckName = healthCheck.Name})
            .ToList();

        return new HealthReport
        {
            StatusType = statusList.Min(s => s.StatusType),
            Checks = statusList,
        };
    }
}