using CommandLine;

namespace Bunkum.Standalone.CommandLine;

public class BunkumStandaloneArguments
{
    [Option("protocol", Required = false)]
    public SupportedProtocol Protocol { get; set; } = SupportedProtocol.Http;
}