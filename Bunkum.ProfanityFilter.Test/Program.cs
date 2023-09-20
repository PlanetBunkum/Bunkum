using Bunkum.HttpServer;
using Bunkum.ProfanityFilter;
using Bunkum.ProfanityFilter.Test;

BunkumHttpServer server = new();
server.AddProfanityService();

server.AddEndpointGroup<FilterEndpoints>();

await server.StartAndBlockAsync();