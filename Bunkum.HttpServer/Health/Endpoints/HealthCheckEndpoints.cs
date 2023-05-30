using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer.Endpoints;

namespace Bunkum.HttpServer.Health.Endpoints;

internal class HealthCheckEndpoints : EndpointGroup
{
    [Endpoint("/_health", ContentType.Json)]
    public HealthReport GetHealthReport(RequestContext context, IEnumerable<IHealthCheck> healthChecks)
    {
        List<HealthStatus> statusList = healthChecks.Select(healthCheck => healthCheck.RunCheck() with {CheckName = healthCheck.Name}).ToList();

        return new HealthReport
        {
            StatusType = statusList.Min(s => s.StatusType),
            Checks = statusList,
        };
    }
}