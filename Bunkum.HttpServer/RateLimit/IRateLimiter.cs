using System.Reflection;
using Bunkum.CustomHttpListener.Request;

namespace Bunkum.HttpServer.RateLimit;

public interface IRateLimiter
{
    public bool UserViolatesRateLimit(ListenerContext context, MethodInfo? method, IRateLimitUser user);
    public bool RemoteEndpointViolatesRateLimit(ListenerContext context, MethodInfo? method);
}