using Bunkum.Core;
using Bunkum.HealthChecks;
using Bunkum.Protocols.Http;
using BunkumTests.HttpServer;
using BunkumConsole = Bunkum.Core.BunkumConsole;

BunkumConsole.AllocateConsole();

BunkumServer server = new BunkumHttpServer();
server.Initialize = s =>
{
    s.AddHealthCheckService();
    s.DiscoverEndpointsFromAssembly(typeof(ServerDependentTest).Assembly);
};

// await server.StartAndBlockAsync();
server.Start();

await Task.Delay(-1);