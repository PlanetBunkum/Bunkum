using Bunkum.HttpServer;
using Bunkum.HttpServer.RateLimit;
using BunkumTests.HttpServer.Time;

namespace BunkumTests.HttpServer.Tests.RateLimit;

[Parallelizable]
public class RateLimitTests
{
    [Test]
    public void AllowsSingleRequest()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");

        RateLimiter rateLimiter = new(timeProvider);
        Assert.That(rateLimiter.ViolatesRateLimit(new RequestContext(), user), Is.False);
    }
    
    [Test]
    public void AllowsManyRequests()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");

        RateLimiter rateLimiter = new(timeProvider);

        for (int i = 0; i < RateLimiter.MaxRequestAmount; i++)
        {
            Assert.That(rateLimiter.ViolatesRateLimit(new RequestContext(), user), Is.False);
        }
    }
    
    [Test]
    public void DeniesTooManyRequests()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");

        RateLimiter rateLimiter = new(timeProvider);

        for (int i = 0; i < RateLimiter.MaxRequestAmount; i++)
        {
            Assert.That(rateLimiter.ViolatesRateLimit(new RequestContext(), user), Is.False);
        }
        
        Assert.That(rateLimiter.ViolatesRateLimit(new RequestContext(), user), Is.True);
    }
    
    [Test]
    public void AllowsRequestAgainAfterTimeout()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");

        RateLimiter rateLimiter = new(timeProvider);

        for (int i = 0; i < RateLimiter.MaxRequestAmount; i++)
        {
            Assert.That(rateLimiter.ViolatesRateLimit(new RequestContext(), user), Is.False);
        }
        
        Assert.That(rateLimiter.ViolatesRateLimit(new RequestContext(), user), Is.True);

        timeProvider.Seconds = 30;
        Assert.That(rateLimiter.ViolatesRateLimit(new RequestContext(), user), Is.False);
    }
    
    [Test]
    public void DeniesCorrectUser()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user1 = new("user1");
        MockRateLimitUser user2 = new("user2");

        RateLimiter rateLimiter = new(timeProvider);

        for (int i = 0; i < RateLimiter.MaxRequestAmount; i++)
        {
            Assert.That(rateLimiter.ViolatesRateLimit(new RequestContext(), user1), Is.False);
        }

        Assert.Multiple(() =>
        {
            Assert.That(rateLimiter.ViolatesRateLimit(new RequestContext(), user1), Is.True);
            Assert.That(rateLimiter.ViolatesRateLimit(new RequestContext(), user2), Is.False);
        });
    }
}