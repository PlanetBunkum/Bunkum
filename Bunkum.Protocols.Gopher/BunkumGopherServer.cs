using Bunkum.Core;
using Bunkum.Listener;
using Bunkum.Protocols.Gopher.Socket;
using NotEnoughLogs;

namespace Bunkum.Protocols.Gopher;

public class BunkumGopherServer : BunkumServer
{
    public BunkumGopherServer(LoggerConfiguration? configuration = null) : base(configuration)
    {}

    public BunkumGopherServer(BunkumListener listener, bool logToConsole = true, LoggerConfiguration? configuration = null) : base(listener, logToConsole, configuration)
    {}

    protected override BunkumListener CreateDefaultListener(Uri listenEndpoint, bool useForwardedIp, Logger logger)
    {
        return new SocketGopherListener(listenEndpoint, logger);
    }

    protected override string ProtocolUriName => "gopher";
}