using Bunkum.Core;
using Bunkum.Listener;
using Bunkum.Protocols.Gopher.Socket;
using NotEnoughLogs;
using NotEnoughLogs.Sinks;

namespace Bunkum.Protocols.Gopher;

public class BunkumGopherServer : BunkumServer
{
    public BunkumGopherServer(LoggerConfiguration? configuration = null, List<ILoggerSink>? sinks = null) : base(configuration, sinks)
    {}

    [Obsolete("This constructor is obsolete, `UseListener` is preferred instead!")]
    public BunkumGopherServer(BunkumListener listener, LoggerConfiguration? configuration = null, List<ILoggerSink>? sinks = null) : base(listener, configuration, sinks)
    {}

    protected override BunkumListener CreateDefaultListener(Uri listenEndpoint, bool useForwardedIp, Logger logger)
    {
        return new SocketGopherListener(listenEndpoint, logger);
    }

    protected override string ProtocolUriName => "gopher";
}