using Bunkum.HttpServer;
using BunkumTests.HttpServer.Endpoints;
using BunkumTests.HttpServer.Middlewares;

namespace BunkumTests.HttpServer.Tests.Middlewares;

public class MiddlewareOrderTest : ServerDependentTest
{
    [Test]
    public async Task MiddlewaresOrderCorrectly()
    {
        (BunkumHttpServer server, HttpClient client) = this.Setup();
        server.AddMiddleware<MiddlewareA>();
        server.AddMiddleware<MiddlewareB>(); // Adding MiddlewareB encapsulates MiddlewareA
        server.AddEndpointGroup<TestEndpoints>();

        HttpResponseMessage msg = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/"));
        Assert.Multiple(async () =>
        {
            Assert.That(msg.IsSuccessStatusCode, Is.True);
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo("BATest"));
        });
    }
}