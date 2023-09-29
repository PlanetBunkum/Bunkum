using System.Reflection;
using System.Text;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Debugging;
using Bunkum.Core.Extensions;
using Bunkum.Core.Listener.Request;
using Bunkum.Core.Responses;
using NotEnoughLogs;

namespace Bunkum.Core.Services;

/// <summary>
/// Enables request logging and other debug features in Bunkum.
/// </summary>
/// <seealso cref="DebugRequestBodyAttribute"/>
/// <seealso cref="DebugResponseBodyAttribute"/>
public class DebugService : Service
{
    internal DebugService(Logger logger) : base(logger)
    {}

    /// <inheritdoc/>
    public override Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        if (method.HasCustomAttribute<DebugRequestBodyAttribute>())
        {
            context.InputStream.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new(context.InputStream);
            string body = reader.ReadToEnd();
            this.Logger.LogDebug(BunkumCategory.Request, "Request body for {0} ({1} bytes):\n{2}", context.Uri.AbsolutePath, context.InputStream.Length, body);
        }
        
        return null;
    }
    
    /// <inheritdoc/>
    public override void AfterRequestHandled(ListenerContext context, Response response, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        if (!method.HasCustomAttribute<DebugResponseBodyAttribute>()) return;
        
        string body = Encoding.UTF8.GetString(response.Data);
        this.Logger.LogDebug(BunkumCategory.Request, "Response body for {0} ({1} bytes):\n{2}", context.Uri.AbsolutePath, response.Data.Length, body);
    }
}