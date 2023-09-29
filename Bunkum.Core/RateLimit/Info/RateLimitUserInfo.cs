namespace Bunkum.Core.RateLimit.Info;

internal class RateLimitUserInfo : IRateLimitInfo
{
    internal RateLimitUserInfo(IRateLimitUser user, string bucket)
    {
        this.User = user;
        this.Bucket = bucket;
    }

    internal IRateLimitUser User { get; init; }
    public string Bucket { get; init; }
    public List<int> RequestTimes { get; init; } = new(RateLimitSettings.DefaultMaxRequestAmount / 2);
    public int LimitedUntil { get; set; }
}