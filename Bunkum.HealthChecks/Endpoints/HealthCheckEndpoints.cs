using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;

namespace Bunkum.HealthChecks.Endpoints;

internal class HealthCheckEndpoints : EndpointGroup
{
    [HttpEndpoint("/_health", ContentType.Json)]
    [Authentication(false)]
    public HealthReport GetHealthReport(RequestContext context, HealthReport report) => report;
}