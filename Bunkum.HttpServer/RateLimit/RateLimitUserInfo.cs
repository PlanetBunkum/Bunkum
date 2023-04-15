using System.Collections.Concurrent;
using System.Diagnostics;

namespace Bunkum.HttpServer.RateLimit;

internal class RateLimitUserInfo
{
    internal RateLimitUserInfo(IRateLimitUser user, int started)
    {
        this.User = user;
        this.Started = started;
    }
    
    internal readonly IRateLimitUser User;
    
    internal readonly List<int> RequestTimes = new(RateLimiter.MaxRequestAmount);
    internal readonly int Started;
    
    internal int? LimitedUntil;
}