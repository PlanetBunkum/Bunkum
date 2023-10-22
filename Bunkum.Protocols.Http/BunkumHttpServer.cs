using Bunkum.Core;
using Bunkum.Listener;
using NotEnoughLogs;
using NotEnoughLogs.Sinks;

namespace Bunkum.Protocols.Http;

public class BunkumHttpServer : BunkumServer
{
    public BunkumHttpServer(LoggerConfiguration? configuration = null, List<ILoggerSink>? sinks = null) : base(configuration, sinks)
    {}

    [Obsolete("This constructor is obsolete, `UseListener` is preferred instead!")]
    public BunkumHttpServer(BunkumListener listener, LoggerConfiguration? configuration = null, List<ILoggerSink>? sinks = null) : base(listener, configuration, sinks)
    {}

    /// <inherit-doc/>
    protected override BunkumListener CreateDefaultListener(Uri listenEndpoint, bool useForwardedIp, Logger logger)
    {
        return new Socket.SocketHttpListener(listenEndpoint, useForwardedIp, logger);
    }

    /// <inherit-doc/>
    protected override string ProtocolUriName => "http";
}