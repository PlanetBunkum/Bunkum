using System.Reflection;
using Bunkum.Core;
using Bunkum.Protocols.Gemini;
using Bunkum.Protocols.Gopher;
using Bunkum.Protocols.Http;
using Bunkum.Standalone.CommandLine;
using Bunkum.Standalone.Middlewares;
using CommandLine;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;

Parser parser = new(settings =>
{
    settings.CaseInsensitiveEnumValues = true;
});

ParserResult<BunkumStandaloneArguments>? result = parser.ParseArguments<BunkumStandaloneArguments>(args);
if (result == null)
{
    Environment.Exit(1);
    return;
}

BunkumStandaloneArguments arguments = result.Value;

Environment.SetEnvironmentVariable("BUNKUM_DATA_FOLDER", Path.GetTempPath());

LoggerConfiguration logConfig = new()
{
    Behaviour = new QueueLoggingBehaviour(),
#if DEBUG
    MaxLevel = LogLevel.Trace,
#else
    MaxLevel = LogLevel.Info,
#endif
};

BunkumServer server = arguments.Protocol switch
{
    SupportedProtocol.Http => new BunkumHttpServer(logConfig),
    SupportedProtocol.Gopher => new BunkumGopherServer(logConfig),
    SupportedProtocol.Gemini => new BunkumGeminiServer(null, logConfig),
    _ => throw new NotImplementedException(arguments.Protocol.ToString()),
};

server.Initialize = s =>
{
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
    s.AddMiddleware<FileSystemMiddleware>();
};

server.Start();
await Task.Delay(-1);