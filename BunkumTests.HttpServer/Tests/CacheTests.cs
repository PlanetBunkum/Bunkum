using System.Net;
using Bunkum.Core;
using BunkumTests.HttpServer.Endpoints;

namespace BunkumTests.HttpServer.Tests;

public class CacheTests : ServerDependentTest
{
    [Test]
    [TestCase("/cache/onlySuccess/ok", HttpStatusCode.OK, true)]
    [TestCase("/cache/onlySuccess/fail", HttpStatusCode.InternalServerError, false)]
    [TestCase("/cache/all/ok", HttpStatusCode.OK, true)]
    [TestCase("/cache/all/fail", HttpStatusCode.InternalServerError, true)]
    public async Task CacheHeaderBehavior(string endpoint, HttpStatusCode expectedStatusCode, bool expectCache)
    {
        // Arrange
        (BunkumServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<CacheTestEndpoints>();
        
        // Act
        HttpResponseMessage msg = await client.GetAsync(endpoint);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(msg.StatusCode, Is.EqualTo(expectedStatusCode));
            
            if (expectCache)
                Assert.That(msg.Headers.CacheControl?.MaxAge, Is.EqualTo(TimeSpan.FromSeconds(60)));
            else
                Assert.That(msg.Headers.CacheControl, Is.Null);
        });
    }
}