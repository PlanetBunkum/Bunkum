using Bunkum.HttpServer.Authentication;

namespace Bunkum.HttpServer.RateLimit;

/// <summary>
/// Exposes methods to Bunkum that allow the rate limiting system to operate on users.
/// </summary>
public interface IRateLimitUser : IUser
{
    /// <param name="obj">The object to check against.</param>
    /// <returns>True if the object is equal to this user's id, false otherwise.</returns>
    public bool RateLimitUserIdIsEqual(object obj);
    
    public object RateLimitUserId { get; }
}