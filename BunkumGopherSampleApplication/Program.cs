using System.Reflection;
using Bunkum.Core;
using Bunkum.Protocols.Gemini;
using Bunkum.Protocols.Gopher;
using Bunkum.Protocols.Gopher.Responses.Serialization;
using Bunkum.Serialization.GopherToGemini;

BunkumServer gopherServer = new BunkumGopherServer();

gopherServer.Initialize = s =>
{
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
    s.AddSerializer<BunkumGophermapSerializer>();
};

BunkumServer geminiServer = new BunkumGeminiServer();
geminiServer.Initialize = s =>
{
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
    s.AddSerializer<BunkumGophermapGeminiSerializer>();
};

gopherServer.Start();
geminiServer.Start();
await Task.Delay(-1);