using Bunkum.Core;
using Bunkum.ProfanityFilter;
using Bunkum.ProfanityFilter.Test;

BunkumServer server = new();
server.AddProfanityService();

server.AddEndpointGroup<FilterEndpoints>();

await server.StartAndBlockAsync();