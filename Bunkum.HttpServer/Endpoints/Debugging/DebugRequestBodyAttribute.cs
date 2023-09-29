using Bunkum.HttpServer.Endpoints.Middlewares;
using Bunkum.HttpServer.Services;

namespace Bunkum.HttpServer.Endpoints.Debugging;

/// <summary>
/// Tells Bunkum to log request bodies for this <see cref="EndpointAttribute">Endpoint</see>. Useful for debugging.
/// </summary>
/// <remarks>
/// Requires the <see cref="DebugService"/> to be added to the <see cref="BunkumHttpServer"/> via the <see cref="BunkumHttpServer.AddMiddleware{TMiddleware}()"/> function.
/// </remarks>
/// <seealso cref="DebugService"/>
public class DebugRequestBodyAttribute : Attribute
{}