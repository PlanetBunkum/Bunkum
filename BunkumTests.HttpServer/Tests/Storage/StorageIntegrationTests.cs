using System.Net;
using Bunkum.Core;
using Bunkum.Core.Storage;
using BunkumTests.HttpServer.Endpoints;

namespace BunkumTests.HttpServer.Tests.Storage;

public class StorageIntegrationTests : ServerDependentTest
{
    [Test]
    public async Task PutsAndGetsData()
    {
        (BunkumServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<StorageEndpoints>();
        server.AddStorageService<InMemoryDataStore>();
        
        HttpResponseMessage msg = await client.GetAsync("/storage/put");
        Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        msg = await client.GetAsync("/storage/get");
        Assert.Multiple(async () =>
        {
            Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo("data"));
        });
    }
}