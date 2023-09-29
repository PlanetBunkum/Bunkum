using System.Reflection;
using Bunkum.Core.Listener.Request;

namespace Bunkum.Core.RateLimit;

public interface IRateLimiter
{
    public bool UserViolatesRateLimit(ListenerContext context, MethodInfo? method, IRateLimitUser user);
    public bool RemoteEndpointViolatesRateLimit(ListenerContext context, MethodInfo? method);
}