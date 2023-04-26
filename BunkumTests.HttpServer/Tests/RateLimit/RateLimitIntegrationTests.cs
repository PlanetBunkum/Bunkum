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
        MockRateLimitUser user = new("user");

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
}