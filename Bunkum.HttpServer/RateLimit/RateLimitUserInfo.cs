namespace Bunkum.HttpServer.RateLimit;

internal class RateLimitUserInfo
{
    internal RateLimitUserInfo(IRateLimitUser user)
    {
        this.User = user;
    }
    
    internal readonly IRateLimitUser User;
    internal readonly List<int> RequestTimes = new(RateLimiter.MaxRequestAmount / 2);
    internal int LimitedUntil;
}