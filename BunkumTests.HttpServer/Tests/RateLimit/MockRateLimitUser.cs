using Bunkum.HttpServer.RateLimit;

namespace BunkumTests.HttpServer.Tests.RateLimit;

public class MockRateLimitUser : IRateLimitUser
{
    public MockRateLimitUser(string id)
    {
        this.UserId = id;
    }

    private string UserId { get; }
    
    public bool UserIdIsEqual(object obj)
    {
        return (string)obj == this.UserId;
    }

    public object RateLimitUserId => this.UserId;
}