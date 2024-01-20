using System.Net;
using Bunkum.Core;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Services;
using BunkumTests.HttpServer.Endpoints;
using Microsoft.Extensions.Time.Testing;

namespace BunkumTests.HttpServer.Tests.RateLimit;

public class RateLimitIntegrationTests : ServerDependentTest
{
    [Test]
    public void AllowsSingleRequest()
    {
        FakeTimeProvider timeProvider = new();
        RateLimiter rateLimiter = new(timeProvider);
        
        (BunkumServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<TestEndpoints>();
        server.AddService<RateLimitService>(rateLimiter);
        
        HttpResponseMessage msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
        Assert.Multiple(async () =>
        {
            Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo(TestEndpoints.TestString));
        });
    }
    
    [Test]
    public void AllowsManyRequests()
    {
        FakeTimeProvider timeProvider = new();
        RateLimiter rateLimiter = new(timeProvider);
        
        (BunkumServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<TestEndpoints>();
        server.AddService<RateLimitService>(rateLimiter);

        for (int i = 0; i < RateLimitSettings.DefaultMaxRequestAmount; i++)
        {
            HttpResponseMessage msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
            Assert.Multiple(async () =>
            {
                Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo(TestEndpoints.TestString));
            });
        }
    }
    
    [Test]
    public void DeniesTooManyRequests()
    {
        FakeTimeProvider timeProvider = new();
        RateLimiter rateLimiter = new(timeProvider);
        
        (BunkumServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<TestEndpoints>();
        server.AddService<RateLimitService>(rateLimiter);

        for (int i = 0; i < RateLimitSettings.DefaultMaxRequestAmount; i++)
        {
            HttpResponseMessage msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
            Assert.Multiple(async () =>
            {
                Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo(TestEndpoints.TestString));
            });
        }
        
        HttpResponseMessage badMsg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
        Assert.That(badMsg.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
    }
    
    [Test]
    public void AllowsRequestAgainAfterTimeout()
    {
        FakeTimeProvider timeProvider = new();
        RateLimiter rateLimiter = new(timeProvider);
        
        (BunkumServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<TestEndpoints>();
        server.AddService<RateLimitService>(rateLimiter);

        for (int i = 0; i < RateLimitSettings.DefaultMaxRequestAmount; i++)
        {
            HttpResponseMessage msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
            Assert.Multiple(async () =>
            {
                Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo(TestEndpoints.TestString));
            });
        }
        
        HttpResponseMessage newMsg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
        Assert.That(newMsg.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));

        timeProvider.Advance(TimeSpan.FromSeconds(RateLimitSettings.DefaultRequestTimeoutDuration));
        
        newMsg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
        Assert.That(newMsg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}