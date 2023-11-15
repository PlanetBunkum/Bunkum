using System.Reflection;
using Bunkum.Core.RateLimit;
using Bunkum.Listener.Request;
using Bunkum.Protocols.Http.Direct;
using Microsoft.Extensions.Time.Testing;

namespace BunkumTests.HttpServer.Tests.RateLimit;

[Parallelizable]
public class RateLimitTests
{
    private static readonly ListenerContext Ctx = new DirectHttpListenerContext();

    [Test]
    public void AllowsSingleRequest()
    {
        FakeTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");

        RateLimiter rateLimiter = new(timeProvider);
        Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user), Is.False);
    }
    
    [Test]
    public void AllowsManyRequests()
    {
        FakeTimeProvider timeProvider = new();
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
        FakeTimeProvider timeProvider = new();
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
        FakeTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");

        RateLimiter rateLimiter = new(timeProvider);

        for (int i = 0; i < RateLimitSettings.DefaultMaxRequestAmount; i++)
        {
            Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user), Is.False);
        }
        
        Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user), Is.True);

        timeProvider.Advance(TimeSpan.FromSeconds(RateLimitSettings.DefaultRequestTimeoutDuration));
        Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, null, user), Is.False);
    }
    
    [Test]
    public void DeniesCorrectUser()
    {
        FakeTimeProvider timeProvider = new();
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
        FakeTimeProvider timeProvider = new();
        MockRateLimitUser user = new("user");
        
        RateLimiter rateLimiter = new(timeProvider);
        MethodInfo method = this.GetType().GetMethod(nameof(this.RespectsAttributeEndpoint), BindingFlags.Instance | BindingFlags.NonPublic)!;

        Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, method, user), Is.False);
        Assert.That(rateLimiter.UserViolatesRateLimit(Ctx, method, user), Is.True);
    }
}