using Bunkum.Core;
using Bunkum.Listener;
using Bunkum.Protocols.Gemini.Socket;
using NotEnoughLogs;

namespace Bunkum.Protocols.Gemini;

public class BunkumGeminiServer : BunkumServer
{
    protected override BunkumListener CreateDefaultListener(Uri listenEndpoint, bool useForwardedIp, Logger logger)
    {
        return new SocketGeminiListener(listenEndpoint, logger);
    }
    protected override string ProtocolUriName => "gemini";
}