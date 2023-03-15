using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;

namespace Bunkum.HttpServer.Endpoints.Middlewares;

public interface IMiddleware
{
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next);
}