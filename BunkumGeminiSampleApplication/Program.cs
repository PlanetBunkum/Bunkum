using System.Reflection;
using Bunkum.Core;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Gemini;
using BunkumGeminiSampleApplication.Configuration;
using BunkumGeminiSampleApplication.Services;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;

BunkumServer server = new BunkumGeminiServer(null, new LoggerConfiguration
{
    Behaviour = new QueueLoggingBehaviour(),
    #if DEBUG
    MaxLevel = LogLevel.Trace,
    #else
    MaxLevel = LogLevel.Info,
    #endif
});

server.Initialize = s =>
{
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
    s.AddConfigFromJsonFile<ExampleConfiguration>("example.json");
    s.AddService<TimeService>();
    s.AddStorageService<InMemoryDataStore>();
};

server.Start();
await Task.Delay(-1);