using System.Net;
using Bunkum.HttpServer;

namespace BunkumTests.HttpServer.Tests;

public class AssemblyDiscoveryTests : ServerDependentTest
{
    [Test]
    public void CanDiscoverFromThisAssembly()
    {
        (BunkumHttpServer server, HttpClient client) = this.Setup();
        
        HttpResponseMessage msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
        Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        
        server.DiscoverEndpointsFromAssembly(typeof(AssemblyDiscoveryTests).Assembly);
        
        msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
        Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}