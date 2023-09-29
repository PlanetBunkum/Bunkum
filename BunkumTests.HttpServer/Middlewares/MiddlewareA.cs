using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Bunkum.Core.Listener.Request;

namespace BunkumTests.HttpServer.Middlewares;

public class MiddlewareA : IMiddleware
{
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        context.Write("A");
        next();
    }
}