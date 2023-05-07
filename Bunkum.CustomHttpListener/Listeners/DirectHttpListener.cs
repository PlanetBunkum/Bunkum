using Bunkum.CustomHttpListener.Request;

namespace Bunkum.CustomHttpListener.Listeners;

public class DirectHttpListener : BunkumHttpListener
{
    public DirectHttpListener(Uri listenEndpoint) : base(listenEndpoint)
    {
    }

    public override void StartListening()
    {
        throw new NotImplementedException();
    }

    protected override Task<ListenerContext?> WaitForConnectionAsyncInternal()
    {
        throw new NotImplementedException();
    }
}