using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer.Endpoints;

namespace Bunkum.HttpServer.Health.Endpoints;

internal class HealthCheckEndpoints : EndpointGroup
{
    [Endpoint("/_health", ContentType.Json)]
    public HealthReport GetHealthReport(RequestContext context, HealthReport report) => report;
}