using System.Reflection;
using Bunkum.Core;
using Bunkum.Cottle.Extensions;
using Bunkum.Protocols.Http;
using Cottle;

BunkumHttpServer server = new();

server.Initialize = s =>
{
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
    s.AddCottle(new DocumentConfiguration());
};
server.Start();
await Task.Delay(-1);