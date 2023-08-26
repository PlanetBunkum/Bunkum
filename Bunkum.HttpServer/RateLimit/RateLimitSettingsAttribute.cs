namespace Bunkum.HttpServer.RateLimit;

[AttributeUsage(AttributeTargets.Method)]
public class RateLimitSettingsAttribute : Attribute
{
    internal readonly RateLimitSettings Settings;

    /// <summary>
    /// Defines settings to be used by the <see cref="RateLimiter"/> for this endpoint.
    /// </summary>
    /// <param name="requestTimeoutDuration">How long should it take a request to stop counting towards the rate limit?</param>
    /// <param name="maxRequestAmount">How many requests until the rate limit is triggered?</param>
    /// <param name="requestBlockDuration">How long should the user be blocked when the rate limit is triggered?</param>
    public RateLimitSettingsAttribute(int requestTimeoutDuration, int maxRequestAmount, int requestBlockDuration, string bucket = "global")
    {
        this.Settings = new RateLimitSettings(requestTimeoutDuration, maxRequestAmount, requestBlockDuration, bucket);
    }
}