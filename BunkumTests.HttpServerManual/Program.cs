using Bunkum.HttpServer;
using BunkumTests.HttpServer;

BunkumConsole.AllocateConsole();

BunkumHttpServer server = new();
server.Initialize = s =>
{
    s.AddHealthCheckService();
    s.DiscoverEndpointsFromAssembly(typeof(ServerDependentTest).Assembly);
};

// await server.StartAndBlockAsync();
server.Start();

await Task.Delay(-1);