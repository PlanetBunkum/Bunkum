using Bunkum.HttpServer.RateLimit;
using Newtonsoft.Json;

namespace Bunkum.HttpServer.Authentication.Dummy;

public class DummyUser : IRateLimitUser
{
    [JsonProperty("userId")]
    public ulong UserId { get; set; } = 1;
    [JsonProperty("username")]
    public string Username { get; set; } = "Dummy";

    public bool RateLimitUserIdIsEqual(object obj)
    {
        return this.UserId == (ulong)obj;
    }

    public object RateLimitUserId => this.UserId;
}