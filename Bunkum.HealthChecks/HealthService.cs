using System.Reflection;
using Bunkum.Core;
using Bunkum.Core.Database;
using Bunkum.Core.Services;
using Bunkum.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.HealthChecks;

public class HealthService : Service
{
    private readonly List<IHealthCheck> _healthChecks;

    public HealthService(Logger logger, IEnumerable<IHealthCheck> checks) : base(logger)
    {
        this._healthChecks = checks.ToList();
    }

    public override object? AddParameterToEndpoint(ListenerContext context, BunkumParameterInfo parameter, Lazy<IDatabaseContext> database)
    {
        if (parameter.ParameterType.IsAssignableFrom(typeof(HealthReport)))
            return this.GenerateReport();

        return base.AddParameterToEndpoint(context, parameter, database);
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