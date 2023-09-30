using Bunkum.Core;
using Bunkum.ProfanityFilter;
using Bunkum.ProfanityFilter.Test;
using Bunkum.Protocols.Http;

BunkumServer server = new BunkumHttpServer();
server.AddProfanityService();

server.AddEndpointGroup<FilterEndpoints>();

server.Start();
await Task.Delay(-1);