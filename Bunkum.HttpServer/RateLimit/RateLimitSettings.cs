using System.Diagnostics.CodeAnalysis;

namespace Bunkum.HttpServer.RateLimit;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public struct RateLimitSettings
{
    /// <summary>
    /// Defines settings to be used by the <see cref="RateLimiter"/>.
    /// </summary>
    /// <param name="requestTimeoutDuration">How long should it take a request to stop counting towards the rate limit?</param>
    /// <param name="maxRequestAmount">How many requests until the rate limit is triggered?</param>
    /// <param name="requestBlockDuration">How long should the user be blocked when the rate limit is triggered?</param>
    public RateLimitSettings(int requestTimeoutDuration, int maxRequestAmount, int requestBlockDuration, string bucket)
    {
        this.RequestTimeoutDuration = requestTimeoutDuration;
        this.MaxRequestAmount = maxRequestAmount;
        this.RequestBlockDuration = requestBlockDuration;
        this.Bucket = bucket;
    }

    public const int DefaultRequestTimeoutDuration = 60;
    public const int DefaultMaxRequestAmount = 50;
    public const int DefaultRequestBlockDuration = 30;
    public const string DefaultBucket = "global";
    
    public static RateLimitSettings DefaultSettings =
        new(DefaultRequestTimeoutDuration, DefaultMaxRequestAmount, DefaultRequestBlockDuration, DefaultBucket);

    /// <summary>
    /// How long should it take a request to stop counting towards the rate limit?
    /// </summary>
    public readonly int RequestTimeoutDuration;
    /// <summary>
    /// How many requests until the rate limit is triggered?
    /// </summary>
    public readonly int MaxRequestAmount;
    /// <summary>
    /// How long should the user be blocked when the rate limit is triggered?
    /// </summary>
    public readonly int RequestBlockDuration;
    
    public string Bucket;
}