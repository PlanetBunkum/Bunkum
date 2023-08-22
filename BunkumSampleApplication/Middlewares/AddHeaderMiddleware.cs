using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Endpoints.Middlewares;

namespace BunkumSampleApplication.Middlewares;

public class AddHeaderMiddleware : IMiddleware
{
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        // Modify the request by adding a header.
        context.ResponseHeaders.Add("X-Middleware", $"This is a header added by {nameof(AddHeaderMiddleware)}.");

        // Call the next middleware in the chain. By design, not calling next() will stop the chain from continuing!
        // You can use this to stop requests or create your own response.
        next();
    }
}