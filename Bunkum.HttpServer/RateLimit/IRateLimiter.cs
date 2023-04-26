using Bunkum.CustomHttpListener.Request;

namespace Bunkum.HttpServer.RateLimit;

public interface IRateLimiter
{
    public bool UserViolatesRateLimit(ListenerContext context, IRateLimitUser user);
    public bool RemoteEndpointViolatesRateLimit(ListenerContext context);
}