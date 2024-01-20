using CommandLine;
using Newtonsoft.Json;

namespace Bunkum.Standalone.CommandLine;

public class BunkumStandaloneArguments
{
    [Option("protocol", Required = false, HelpText = "The protocol to use. Can be Http, Gopher, or Gemini.")]
    public SupportedProtocol Protocol { get; set; } = SupportedProtocol.Http;
}