using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Bunkum.Listener.Request;

namespace BunkumTests.HttpServer.Middlewares;

public class MiddlewareB : IMiddleware
{
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        context.Write("B");
        next();
    }
}