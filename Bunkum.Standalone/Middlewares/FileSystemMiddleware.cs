using System.Net;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Bunkum.Listener.Request;

namespace Bunkum.Standalone.Middlewares;

public class FileSystemMiddleware : IMiddleware
{
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        string path = Path.Join(Environment.CurrentDirectory, context.Uri.AbsolutePath);
        if (!File.Exists(path))
        {
            context.ResponseCode = HttpStatusCode.NotFound;
            return;
        }

        try
        {
            byte[] data = File.ReadAllBytes(path);
            
            context.ResponseCode = HttpStatusCode.OK;
            context.Write(data);
        }
        catch (UnauthorizedAccessException)
        {
            context.ResponseCode = HttpStatusCode.Forbidden;
        }
        catch
        {
            context.ResponseCode = HttpStatusCode.InternalServerError;
        }
    }
}