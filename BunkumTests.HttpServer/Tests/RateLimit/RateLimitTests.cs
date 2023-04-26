using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer;
using Bunkum.HttpServer.RateLimit;
using BunkumTests.HttpServer.Time;

namespace BunkumTests.HttpServer.Tests.RateLimit;

[Parallelizable]
public class RateLimitTests
{
#pragma warning disable CS0618
    private static readonly ListenerContext Ctx = new();
#pragma warning restore CS0618
    
    [Test]
    public void AllowsSingleRequest()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");

        RateLimiter rateLimiter = new(timeProvider);
        Assert.That(rateLimiter.ViolatesRateLimit(Ctx, user), Is.False);
    }
    
    [Test]
    public void AllowsManyRequests()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");

        RateLimiter rateLimiter = new(timeProvider);

        for (int i = 0; i < RateLimiter.MaxRequestAmount; i++)
        {
            Assert.That(rateLimiter.ViolatesRateLimit(Ctx, user), Is.False);
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
            Assert.That(rateLimiter.ViolatesRateLimit(Ctx, user), Is.False);
        }
        
        Assert.That(rateLimiter.ViolatesRateLimit(Ctx, user), Is.True);
    }
    
    [Test]
    public void AllowsRequestAgainAfterTimeout()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");

        RateLimiter rateLimiter = new(timeProvider);

        for (int i = 0; i < RateLimiter.MaxRequestAmount; i++)
        {
            Assert.That(rateLimiter.ViolatesRateLimit(Ctx, user), Is.False);
        }
        
        Assert.That(rateLimiter.ViolatesRateLimit(Ctx, user), Is.True);

        timeProvider.Seconds = 30;
        Assert.That(rateLimiter.ViolatesRateLimit(Ctx, user), Is.False);
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
            Assert.That(rateLimiter.ViolatesRateLimit(Ctx, user1), Is.False);
        }

        Assert.Multiple(() =>
        {
            Assert.That(rateLimiter.ViolatesRateLimit(Ctx, user1), Is.True);
            Assert.That(rateLimiter.ViolatesRateLimit(Ctx, user2), Is.False);
        });
    }
}