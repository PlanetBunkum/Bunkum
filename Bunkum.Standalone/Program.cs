using System.Reflection;
using System.Runtime.CompilerServices;
using Bunkum.Core;
using Bunkum.Protocols.Gemini;
using Bunkum.Protocols.Gopher;
using Bunkum.Protocols.Http;
using Bunkum.Standalone.CommandLine;
using Bunkum.Standalone.Middlewares;
using CommandLine;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;

namespace Bunkum.Standalone;

internal class Program
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PrintExperimentalWarning()
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Warning: Bunkum's standalone server is currently experimental. Use caution.");
        Console.ForegroundColor = oldColor;
    }
    
    public static async Task Main(string[] args)
    {
        PrintExperimentalWarning();
        using Parser parser = new(settings =>
        {
            settings.CaseInsensitiveEnumValues = true;
            settings.AutoHelp = true;
            settings.AutoVersion = true;
            settings.HelpWriter = Console.Out;
        });

        await parser.ParseArguments<BunkumStandaloneArguments>(args)
            .WithParsedAsync(StartServer);
    }

    private static async Task StartServer(BunkumStandaloneArguments arguments)
    {
        const string dataFolder = "BUNKUM_DATA_FOLDER";

        if (Environment.GetEnvironmentVariable(dataFolder) == null)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string configPath = Path.Join(appData, "bunkum");
    
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);
    
            Environment.SetEnvironmentVariable(dataFolder, configPath);
        }

        LogLevel level;
        if (arguments.Debug) level = LogLevel.Debug;
        else if (arguments.Trace) level = LogLevel.Trace;
        else level = LogLevel.Info;

        LoggerConfiguration logConfig = new()
        {
            Behaviour = new QueueLoggingBehaviour(),
            MaxLevel = level,
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
    }
}