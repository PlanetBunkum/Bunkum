using System.Reflection;
using Bunkum.Core.RateLimit;
using Bunkum.Listener.Request;
using BunkumTests.HttpServer.Time;

namespace BunkumTests.HttpServer.Tests.RateLimit;

[Parallelizable]
public class RateLimitTests
{
    private static readonly ListenerContext Ctx = new DirectListenerContext();

    [Test]
    public void AllowsSingleRequest()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");

        RateLimiter rateLimiter = new(timeProvider);
        Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user), Is.False);
    }
    
    [Test]
    public void AllowsManyRequests()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");

        RateLimiter rateLimiter = new(timeProvider);

        for (int i = 0; i < RateLimitSettings.DefaultMaxRequestAmount; i++)
        {
            Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user), Is.False);
        }
    }
    
    [Test]
    public void DeniesTooManyRequests()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");

        RateLimiter rateLimiter = new(timeProvider);

        for (int i = 0; i < RateLimitSettings.DefaultMaxRequestAmount; i++)
        {
            Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user), Is.False);
        }
        
        Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user), Is.True);
    }
    
    [Test]
    public void AllowsRequestAgainAfterTimeout()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");

        RateLimiter rateLimiter = new(timeProvider);

        for (int i = 0; i < RateLimitSettings.DefaultMaxRequestAmount; i++)
        {
            Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user), Is.False);
        }
        
        Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user), Is.True);

        timeProvider.Seconds = RateLimitSettings.DefaultRequestTimeoutDuration;
        Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user), Is.False);
    }
    
    [Test]
    public void DeniesCorrectUser()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user1 = new("user1");
        MockRateLimitUser user2 = new("user2");

        RateLimiter rateLimiter = new(timeProvider);

        for (int i = 0; i < RateLimitSettings.DefaultMaxRequestAmount; i++)
        {
            Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user1), Is.False);
        }

        Assert.Multiple(() =>
        {
            Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user1), Is.True);
            Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user2), Is.False);
        });
    }

    [RateLimitSettings(1, 1, 60)]
    private void RespectsAttributeEndpoint() {}

    [Test]
    public void RespectsAttribute()
    {
        MockTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");
        
        RateLimiter rateLimiter = new(timeProvider);
        MethodInfo method = this.GetType().GetMethod(nameof(this.RespectsAttributeEndpoint), BindingFlags.Instance | BindingFlags.NonPublic)!;

        Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, method, user), Is.False);
        Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, method, user), Is.True);
    }
}