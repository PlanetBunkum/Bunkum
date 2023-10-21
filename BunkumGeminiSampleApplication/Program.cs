using System.Reflection;
using Bunkum.Core;
using Bunkum.Core.Responses;
using Bunkum.Protocols.Gemini;

// Response.AddSerializer<BunkumGeminiSerializer>();

BunkumServer server = new BunkumGeminiServer();

server.Initialize = s =>
{
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
};

server.Start();
await Task.Delay(-1);;