using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Time;

namespace Bunkum.HttpServer.RateLimit;

public class RateLimiter : IRateLimiter 
{
    private readonly ITimeProvider _timeProvider;

    public RateLimiter(ITimeProvider timeProvider) => this._timeProvider = timeProvider;
    public RateLimiter() => this._timeProvider = new RealTimeProvider();

    public const int RequestTimeout = 60;
    public const int MaxRequestAmount = 50;

    private readonly List<RateLimitUserInfo> _userInfos = new();

    public bool ViolatesRateLimit(RequestContext context, IRateLimitUser user)
    {
        RateLimitUserInfo? info = this._userInfos
            .FirstOrDefault(i => user.UserIdIsEqual(i.User.RateLimitUserId));

        int now = this._timeProvider.Seconds;

        if (info == null)
        {
            info = new RateLimitUserInfo(user, now);
            this._userInfos.Add(info);
        }
        
        if (info.LimitedUntil != null)
        {
            if (info.LimitedUntil > now) return true;
            info.LimitedUntil = null;
        }
        
        info.RequestTimes.RemoveAll(r => r - RequestTimeout < now - RequestTimeout);
        info.RequestTimes.Add(now);
        
        if (info.RequestTimes.Count > MaxRequestAmount)
        {
            info.LimitedUntil = now + 30;
            return true;
        }
        
        return false;
    }
}