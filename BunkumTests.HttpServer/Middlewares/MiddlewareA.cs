using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Endpoints.Middlewares;

namespace BunkumTests.HttpServer.Middlewares;

public class MiddlewareA : IMiddleware
{
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        context.Write("A");
        next();
    }
}