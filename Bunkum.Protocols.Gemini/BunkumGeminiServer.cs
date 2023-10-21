using Bunkum.Core;
using Bunkum.Listener;
using Bunkum.Protocols.Gemini.Socket;
using NotEnoughLogs;
using NotEnoughLogs.Sinks;

namespace Bunkum.Protocols.Gemini;

public class BunkumGeminiServer : BunkumServer
{
    public BunkumGeminiServer(LoggerConfiguration? configuration = null, List<ILoggerSink>? sinks = null) : base(configuration, sinks)
    {}
    
    protected override BunkumListener CreateDefaultListener(Uri listenEndpoint, bool useForwardedIp, Logger logger)
    {
        return new SocketGeminiListener(listenEndpoint, logger);
    }
    protected override string ProtocolUriName => "gemini";
}