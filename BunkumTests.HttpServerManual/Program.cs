using Bunkum.HttpServer;
using BunkumTests.HttpServer;

BunkumConsole.AllocateConsole();

BunkumHttpServer server = new("http://+:10060/");
server.DiscoverEndpointsFromAssembly(typeof(ServerDependentTest).Assembly);

await server.StartAndBlockAsync();