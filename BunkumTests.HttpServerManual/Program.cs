using Bunkum.Core;
using BunkumTests.HttpServer;
using BunkumConsole = Bunkum.Core.BunkumConsole;

BunkumConsole.AllocateConsole();

BunkumServer server = new();
server.Initialize = s =>
{
    s.AddHealthCheckService();
    s.DiscoverEndpointsFromAssembly(typeof(ServerDependentTest).Assembly);
};

// await server.StartAndBlockAsync();
server.Start();

await Task.Delay(-1);