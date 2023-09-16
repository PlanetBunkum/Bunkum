using System.Net.Http.Headers;
using Bunkum.HttpServer;
using BunkumTests.HttpServer.Endpoints;
using BunkumTests.HttpServer.Middlewares;

namespace BunkumTests.HttpServer.Tests.Middlewares;

public class NullByteRemoval : ServerDependentTest
{
    [Test]
    public async Task TestNullByteRemoval()
    {
        (BunkumHttpServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<TestEndpoints>();

        HttpRequestMessage request = new(HttpMethod.Get, "/echoString");
        request.Content = new StringContent("BeforeNull\0AfterNull\0\0");
        
        HttpResponseMessage msg = await client.SendAsync(request);
        Assert.Multiple(async () =>
        {
            Assert.That(msg.IsSuccessStatusCode, Is.True);
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo("BeforeNull"));
        });
    }
}