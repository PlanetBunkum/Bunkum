using System.Net;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Bunkum.Cottle.Services;
using Bunkum.Listener.Request;
using Cottle;

namespace Bunkum.Cottle.Middlewares;

public class CottleMiddleware : IMiddleware
{
    private readonly CottleService _service;

    public CottleMiddleware(CottleService service)
    {
        this._service = service;
    }

    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        string rendered = this._service.CompiledDocument.Render(Context.Empty);
        context.Write(rendered);
        context.ResponseCode = HttpStatusCode.OK;
        // next();
    }
}