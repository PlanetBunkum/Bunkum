using Bunkum.HttpServer;
using BunkumTests.HttpServer;

BunkumConsole.AllocateConsole();

BunkumHttpServer server = new();
server.DiscoverEndpointsFromAssembly(typeof(ServerDependentTest).Assembly);

// await server.StartAndBlockAsync();
server.Start();
await Task.Delay(-1);