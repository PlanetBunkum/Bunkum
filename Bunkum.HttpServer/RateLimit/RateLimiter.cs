using System.Net;
using System.Reflection;
using Bunkum.CustomHttpListener.Parsing;
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

    public bool UserViolatesRateLimit(ListenerContext context, MethodInfo? method, IRateLimitUser user)
    {
        RateLimitUserInfo? info = this._userInfos
            .FirstOrDefault(i => user.RateLimitUserIdIsEqual(i.User.RateLimitUserId));

        if (info == null)
        {
            info = new RateLimitUserInfo(user);
            this._userInfos.Add(info);
        }

        return this.ViolatesRateLimit(context, info, method);
    }

    public bool RemoteEndpointViolatesRateLimit(ListenerContext context, MethodInfo? method)
    {
        IPAddress ipAddress = context.RemoteEndpoint.Address;

        RateLimitRemoteEndpointInfo? info = this._remoteEndpointInfos
            .FirstOrDefault(i => ipAddress.Equals(i.IpAddress));

        if (info == null)
        {
            info = new RateLimitRemoteEndpointInfo(ipAddress);
            this._remoteEndpointInfos.Add(info);
        }

        return this.ViolatesRateLimit(context, info, method);
    }

    private bool ViolatesRateLimit(ListenerContext context, IRateLimitInfo info, MethodInfo? method)
    {
        int now = this._timeProvider.Seconds;
        
        if (info.LimitedUntil != 0)
        {
            if (info.LimitedUntil > now) return true;
            info.LimitedUntil = 0;
            info.RequestTimes.Clear();
        }
        
        RateLimitSettings settings = method?.GetCustomAttribute<RateLimitSettingsAttribute>()?.Settings ?? this._settings;
        
        info.RequestTimes.RemoveAll(r => r <= now - settings.RequestTimeoutDuration);
        
        if (info.RequestTimes.Count + 1 > settings.MaxRequestAmount)
        {
            info.LimitedUntil = now + settings.RequestBlockDuration;
            context.ResponseHeaders.TryAdd("Retry-After", settings.RequestBlockDuration.ToString());
            
            return true;
        }
        
        info.RequestTimes.Add(now);
        
        return false;
    }
}