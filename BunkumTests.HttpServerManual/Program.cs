using Bunkum.HttpServer;
using BunkumTests.HttpServer;
using RefreshConsole = Bunkum.HttpServer.RefreshConsole;

RefreshConsole.AllocateConsole();

RefreshHttpServer server = new("http://+:10060/");
server.DiscoverEndpointsFromAssembly(typeof(ServerDependentTest).Assembly);

await server.StartAndBlockAsync();