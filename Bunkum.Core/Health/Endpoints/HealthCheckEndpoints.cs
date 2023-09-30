using Bunkum.Core.Endpoints;
using Bunkum.Core.Listener.Protocol;

namespace Bunkum.Core.Health.Endpoints;

internal class HealthCheckEndpoints : EndpointGroup
{
    [Endpoint("/_health", ContentType.Json)]
    [Authentication(false)]
    public HealthReport GetHealthReport(RequestContext context, HealthReport report) => report;
}