using CommandLine;
using Newtonsoft.Json;

namespace Bunkum.Standalone.CommandLine;

public class BunkumStandaloneArguments
{
    [Option("protocol", Required = false, HelpText = "The protocol to use. Can be Http, Gopher, or Gemini.")]
    public SupportedProtocol Protocol { get; set; } = SupportedProtocol.Http;

    [Option("debug", Required = false, HelpText = "Prints debug logging information.")]
    public bool Debug { get; set; } = false;
    
    [Option("trace", Required = false, HelpText = "Prints trace logging information.")]
    public bool Trace { get; set; } = false;
}