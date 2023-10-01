using Bunkum.Listener.Request;

namespace Bunkum.Protocols.Http.Socket;

public class SocketHttpListenerContext : SocketListenerContext
{
    public SocketHttpListenerContext(System.Net.Sockets.Socket socket) : base(socket)
    {}
}