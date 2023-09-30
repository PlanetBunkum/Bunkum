using Bunkum.Core.Database;
using Bunkum.Listener.Request;

namespace Bunkum.Core.Endpoints.Middlewares;

public interface IMiddleware
{
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next);
}