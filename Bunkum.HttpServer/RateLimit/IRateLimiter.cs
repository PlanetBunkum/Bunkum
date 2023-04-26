using Bunkum.CustomHttpListener.Request;

namespace Bunkum.HttpServer.RateLimit;

public interface IRateLimiter
{
    public bool ViolatesRateLimit(ListenerContext context, IRateLimitUser user);
}