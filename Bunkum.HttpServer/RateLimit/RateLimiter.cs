using System.Net;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.RateLimit.Info;
using Bunkum.HttpServer.Time;

namespace Bunkum.HttpServer.RateLimit;

public class RateLimiter : IRateLimiter 
{
    private readonly ITimeProvider _timeProvider;

    public RateLimiter(ITimeProvider timeProvider) => this._timeProvider = timeProvider;
    public RateLimiter() => this._timeProvider = new RealTimeProvider();

    public const int RequestTimeout = 60;
    public const int MaxRequestAmount = 50;

    private readonly List<RateLimitUserInfo> _userInfos = new(25);
    private readonly List<RateLimitRemoteEndpointInfo> _remoteEndpointInfos = new(25);

    public bool UserViolatesRateLimit(ListenerContext context, IRateLimitUser user)
    {
        RateLimitUserInfo? info = this._userInfos
            .FirstOrDefault(i => user.RateLimitUserIdIsEqual(i.User.RateLimitUserId));

        if (info == null)
        {
            info = new RateLimitUserInfo(user);
            this._userInfos.Add(info);
        }

        return this.ViolatesRateLimit(info);
    }

    public bool RemoteEndpointViolatesRateLimit(ListenerContext context)
    {
        IPAddress ipAddress = context.RemoteEndpoint.Address;

        RateLimitRemoteEndpointInfo? info = this._remoteEndpointInfos
            .FirstOrDefault(i => ipAddress.Equals(i.IpAddress));

        if (info == null)
        {
            info = new RateLimitRemoteEndpointInfo(ipAddress);
            this._remoteEndpointInfos.Add(info);
        }

        return this.ViolatesRateLimit(info);
    }

    private bool ViolatesRateLimit(IRateLimitInfo info)
    {
        int now = this._timeProvider.Seconds;
        
        if (info.LimitedUntil != 0)
        {
            if (info.LimitedUntil > now) return true;
            info.LimitedUntil = 0;
        }
        
        info.RequestTimes.RemoveAll(r => r <= now - RequestTimeout);
        
        if (info.RequestTimes.Count + 1 > MaxRequestAmount)
        {
            info.LimitedUntil = now + 30;
            return true;
        }
        
        info.RequestTimes.Add(now);
        
        return false;
    }
}