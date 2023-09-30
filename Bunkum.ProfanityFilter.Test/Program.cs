using Bunkum.Core;
using Bunkum.ProfanityFilter;
using Bunkum.ProfanityFilter.Test;

BunkumServer server = new();
server.AddProfanityService();

server.AddEndpointGroup<FilterEndpoints>();

server.Start();
await Task.Delay(-1);