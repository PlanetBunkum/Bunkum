using Bunkum.HttpServer;
using BunkumTests.HttpServer.Endpoints;

namespace BunkumTests.HttpServer.Tests;

public class RequestHeaderTests : ServerDependentTest
{
    [Test]
    public async Task ParsesHeadersCorrectly()
    {
        (BunkumHttpServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<HeaderEndpoints>();

        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Test-Header", "test");

        Assert.That(await client.GetStringAsync("header/X-Test-Header"), Is.EqualTo("test"));
    }
    
    [Test]
    public async Task ParsesCookiesCorrectly()
    {
        (BunkumHttpServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<HeaderEndpoints>();

        client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", "cookie=test;cookie2=test2");

        Assert.That(await client.GetStringAsync("cookie/cookie"), Is.EqualTo("test"));
        Assert.That(await client.GetStringAsync("cookie/cookie2"), Is.EqualTo("test2"));

        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", "cookie=test; cookie2=test2");
        
        Assert.That(await client.GetStringAsync("cookie/cookie"), Is.EqualTo("test"));
        Assert.That(await client.GetStringAsync("cookie/cookie2"), Is.EqualTo("test2"));
    }
}