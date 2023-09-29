using System.Net;
using Bunkum.Core;
using BunkumTests.HttpServer.Endpoints;

namespace BunkumTests.HttpServer.Tests;

public class NullStatusCodeTests : ServerDependentTest
{
    [Test]
    public async Task ReturnsCorrectResponseWhenNull()
    {
        (BunkumServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<NullEndpoints>();

        HttpResponseMessage resp = await client.GetAsync("/null?null=true");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task ReturnsCorrectResponseWhenNotNull()
    {
        (BunkumServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<NullEndpoints>();

        HttpResponseMessage resp = await client.GetAsync("/null?null=false");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}