using System.Net;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.RateLimit.Info;
using Bunkum.HttpServer.Time;

namespace Bunkum.HttpServer.RateLimit;

public class RateLimiter : IRateLimiter 
{
    private readonly ITimeProvider _timeProvider;
    private readonly RateLimitSettings _settings;
    
    public RateLimiter()
    {
        this._timeProvider = new RealTimeProvider();
        this._settings = RateLimitSettings.DefaultSettings;
    }

    public RateLimiter(ITimeProvider timeProvider)
    {
        this._timeProvider = timeProvider;
        this._settings = RateLimitSettings.DefaultSettings;
    }

    public RateLimiter(RateLimitSettings settings)
    {
        this._timeProvider = new RealTimeProvider();
        this._settings = settings;
    }
    
    public RateLimiter(ITimeProvider provider, RateLimitSettings settings)
    {
        this._timeProvider = provider;
        this._settings = settings;
    }

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

        return this.ViolatesRateLimit(context, info);
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

        return this.ViolatesRateLimit(context, info);
    }

    private bool ViolatesRateLimit(ListenerContext context, IRateLimitInfo info)
    {
        int now = this._timeProvider.Seconds;
        
        if (info.LimitedUntil != 0)
        {
            if (info.LimitedUntil > now) return true;
            info.LimitedUntil = 0;
            info.RequestTimes.Clear();
        }
        
        info.RequestTimes.RemoveAll(r => r <= now - this._settings.RequestTimeoutDuration);
        
        if (info.RequestTimes.Count + 1 > this._settings.MaxRequestAmount)
        {
            info.LimitedUntil = now + this._settings.RequestBlockDuration;
            context.ResponseHeaders.Add("Retry-After", this._settings.RequestBlockDuration.ToString());
            
            return true;
        }
        
        info.RequestTimes.Add(now);
        
        return false;
    }
}