using System.Reflection;
using Bunkum.Core;
using Bunkum.Protocols.Gopher;

BunkumServer server = new BunkumGopherServer();

server.Initialize = s =>
{
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
};

server.Start();
await Task.Delay(-1);