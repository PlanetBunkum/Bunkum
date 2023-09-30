using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;

namespace Bunkum.Core.Health.Endpoints;

internal class HealthCheckEndpoints : EndpointGroup
{
    [HttpEndpoint("/_health", ContentType.Json)]
    [Authentication(false)]
    public HealthReport GetHealthReport(RequestContext context, HealthReport report) => report;
}