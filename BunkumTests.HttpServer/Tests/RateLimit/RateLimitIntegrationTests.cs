using System.Net;
using Bunkum.HttpServer;
using Bunkum.HttpServer.RateLimit;
using Bunkum.HttpServer.Services;
using BunkumTests.HttpServer.Endpoints;
using BunkumTests.HttpServer.Time;

namespace BunkumTests.HttpServer.Tests.RateLimit;

public class RateLimitIntegrationTests : ServerDependentTest
{
    [Test]
    public void AllowsSingleRequest()
    {
        MockTimeProvider timeProvider = new();
        RateLimiter rateLimiter = new(timeProvider);
        
        (BunkumHttpServer server, HttpClient client) = this.Setup();
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
        MockTimeProvider timeProvider = new();
        RateLimiter rateLimiter = new(timeProvider);
        
        (BunkumHttpServer server, HttpClient client) = this.Setup();
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
        MockTimeProvider timeProvider = new();
        RateLimiter rateLimiter = new(timeProvider);
        
        (BunkumHttpServer server, HttpClient client) = this.Setup();
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
        MockTimeProvider timeProvider = new();
        RateLimiter rateLimiter = new(timeProvider);
        
        (BunkumHttpServer server, HttpClient client) = this.Setup();
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

        timeProvider.Seconds = RateLimitSettings.DefaultRequestTimeoutDuration;
        
        newMsg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
        Assert.That(newMsg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}