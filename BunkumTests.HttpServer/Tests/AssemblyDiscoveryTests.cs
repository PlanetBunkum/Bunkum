using System.Net;
using Bunkum.Core;

namespace BunkumTests.HttpServer.Tests;

public class AssemblyDiscoveryTests : ServerDependentTest
{
    [Test]
    public void CanDiscoverFromThisAssembly()
    {
        (BunkumServer server, HttpClient client) = this.Setup();
        
        HttpResponseMessage msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
        Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        
        server.DiscoverEndpointsFromAssembly(typeof(AssemblyDiscoveryTests).Assembly);
        
        msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
        Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}