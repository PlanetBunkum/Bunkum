namespace Bunkum.Core.RateLimit.Info;

internal interface IRateLimitInfo
{
    internal List<int> RequestTimes { get; init; }
    internal int LimitedUntil { get; set; }
    public string Bucket { get; init; }
}