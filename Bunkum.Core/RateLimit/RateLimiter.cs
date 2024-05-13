using System.Net;
using System.Reflection;
using Bunkum.Core.RateLimit.Info;
using Bunkum.Listener.Request;

namespace Bunkum.Core.RateLimit;

public class RateLimiter : IRateLimiter 
{
    private readonly TimeProvider _timeProvider;
    private readonly RateLimitSettings _settings;
    
    public RateLimiter()
    {
        this._timeProvider = TimeProvider.System;
        this._settings = RateLimitSettings.DefaultSettings;
    }

    public RateLimiter(TimeProvider timeProvider)
    {
        this._timeProvider = timeProvider;
        this._settings = RateLimitSettings.DefaultSettings;
    }

    public RateLimiter(RateLimitSettings settings)
    {
        this._timeProvider = TimeProvider.System;
        this._settings = settings;
    }
    
    public RateLimiter(TimeProvider provider, RateLimitSettings settings)
    {
        this._timeProvider = provider;
        this._settings = settings;
    }

    private readonly List<RateLimitUserInfo> _userInfos = new(25);
    private readonly List<RateLimitRemoteEndpointInfo> _remoteEndpointInfos = new(25);

    public bool UserViolatesRateLimit(ListenerContext context, MethodInfo? method, IRateLimitUser user)
    {
        RateLimitSettings settings = method?.GetCustomAttribute<RateLimitSettingsAttribute>()?.Settings ?? this._settings;

        lock (this._remoteEndpointInfos)
        {
            RateLimitUserInfo? info = this._userInfos
                .FirstOrDefault(i =>
                    user.RateLimitUserIdIsEqual(i.User.RateLimitUserId) && i.Bucket == settings.Bucket);

            if (info == null)
            {
                info = new RateLimitUserInfo(user, settings.Bucket);
                lock (this._userInfos)
                {
                    this._userInfos.Add(info);
                }
            }

            lock (info)
            {
                return this.ViolatesRateLimit(context, info, settings);
            }
        }
    }

    public bool RemoteEndpointViolatesRateLimit(ListenerContext context, MethodInfo? method)
    {
        IPAddress ipAddress = context.RemoteEndpoint.Address;
        
        RateLimitSettings settings = method?.GetCustomAttribute<RateLimitSettingsAttribute>()?.Settings ?? this._settings;

        lock (this._remoteEndpointInfos)
        {
            RateLimitRemoteEndpointInfo? info = this._remoteEndpointInfos
                .FirstOrDefault(i => ipAddress.Equals(i.IpAddress) && i.Bucket == settings.Bucket);

            if (info == null)
            {
                info = new RateLimitRemoteEndpointInfo(ipAddress, settings.Bucket);

                this._remoteEndpointInfos.Add(info);
            }

            lock (info)
            {
                return this.ViolatesRateLimit(context, info, settings);
            }
        }
    }

    private bool ViolatesRateLimit(ListenerContext context, IRateLimitInfo info, RateLimitSettings settings)
    {
        int now = (int)this._timeProvider.GetUtcNow().ToUnixTimeSeconds();
        
        if (info.LimitedUntil != 0)
        {
            if (info.LimitedUntil > now) return true;
            info.LimitedUntil = 0;
            info.RequestTimes.Clear();
        }
        
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