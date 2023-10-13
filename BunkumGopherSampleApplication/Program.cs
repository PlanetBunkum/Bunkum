using System.Reflection;
using Bunkum.Core;
using Bunkum.Core.Responses;
using Bunkum.Protocols.Gopher;
using Bunkum.Protocols.Gopher.Responses.Serialization;

Response.AddSerializer<BunkumGophermapSerializer>();

BunkumServer server = new BunkumGopherServer();

server.Initialize = s =>
{
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
};

server.Start();
await Task.Delay(-1);