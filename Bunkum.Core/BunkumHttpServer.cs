using System.Net;
using Bunkum.Core.Listener;
using HttpListenerContext = Bunkum.Core.Listener.Request.Http.HttpListenerContext;
using HttpMethod = Bunkum.Core.Listener.Parsing.HttpMethod;
using HttpVersion = Bunkum.Core.Listener.Request.Http.HttpVersion;

namespace Bunkum.Core;

public class BunkumHttpServer : BunkumServer<BunkumHttpListener, HttpListenerContext, HttpStatusCode, HttpVersion, HttpMethod>
{
    protected override void HandleRequestError(HttpListenerContext context, Exception? e)
    {
        context.ResponseCode = HttpStatusCode.InternalServerError;
            
#if DEBUG
        context.Write(e?.ToString() ?? "Internal Server Error");
#else
        context.Write("Internal Server Error");
#endif
    }
}