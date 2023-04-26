using System.Net;

namespace Bunkum.HttpServer.RateLimit.Info;

public class RateLimitRemoteEndpointInfo : IRateLimitInfo
{
    public RateLimitRemoteEndpointInfo(IPAddress ipAddress)
    {
        this.IpAddress = ipAddress;
    }

    internal IPAddress IpAddress { get; init; }
    public List<int> RequestTimes { get; init; } = new(RateLimitSettings.DefaultMaxRequestAmount / 2);
    public int LimitedUntil { get; set; }
}