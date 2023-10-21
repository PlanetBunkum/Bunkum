using System.Reflection;
using Bunkum.Core;
using Bunkum.Protocols.Gopher;
using Bunkum.Protocols.Gopher.Responses.Serialization;

BunkumServer server = new BunkumGopherServer();

server.Initialize = s =>
{
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
    s.AddSerializer<BunkumGophermapSerializer>();
};

server.Start();
await Task.Delay(-1);