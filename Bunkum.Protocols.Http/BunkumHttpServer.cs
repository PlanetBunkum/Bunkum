using Bunkum.Core;
using Bunkum.Listener;
using NotEnoughLogs;

namespace Bunkum.Protocols.Http;

public class BunkumHttpServer : BunkumServer
{
    public BunkumHttpServer(LoggerConfiguration? configuration = null) : base(configuration)
    {}

    public BunkumHttpServer(BunkumListener listener, bool logToConsole = true, LoggerConfiguration? configuration = null) : base(listener, logToConsole, configuration)
    {}

    protected override BunkumListener CreateDefaultListener(Uri listenEndpoint, bool useForwardedIp, Logger logger)
    {
        return new Socket.SocketHttpListener(listenEndpoint, useForwardedIp, logger);
    }

    protected override string ProtocolUriName => "http";
}