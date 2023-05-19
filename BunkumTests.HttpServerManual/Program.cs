using Bunkum.HttpServer;
using BunkumTests.HttpServer;

BunkumConsole.AllocateConsole();

BunkumHttpServer server = new(new Uri("http://0.0.0.0:10060/"));
server.DiscoverEndpointsFromAssembly(typeof(ServerDependentTest).Assembly);

// await server.StartAndBlockAsync();
server.Start();
await Task.Delay(-1);