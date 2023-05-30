using Bunkum.HttpServer;

namespace BunkumTests.HttpServer.Tests;

public class HealthCheckTests : ServerDependentTest
{
    [Test]
    public async Task HealthChecksWork()
    {
        (BunkumHttpServer server, HttpClient client) = this.Setup();
        server.AddHealthCheckService();
    }
}