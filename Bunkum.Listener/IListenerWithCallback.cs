using Bunkum.Listener.Request;

namespace Bunkum.Listener;

public interface IListenerWithCallback
{
    Action<ListenerContext>? Callback { get; set; }
}