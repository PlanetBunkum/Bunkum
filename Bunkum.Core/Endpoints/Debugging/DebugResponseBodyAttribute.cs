using Bunkum.Core.Services;

namespace Bunkum.Core.Endpoints.Debugging;

/// <summary>
/// Tells Bunkum to log response bodies for this <see cref="EndpointAttribute">Endpoint</see>. Useful for debugging.
/// </summary>
/// <remarks>
/// Requires the <see cref="DebugService"/> to be added to the <see cref="BunkumServer"/> via the <see cref="BunkumServer.AddMiddleware{TMiddleware}()"/> function.
/// </remarks>
/// <seealso cref="DebugService"/>
public class DebugResponseBodyAttribute : Attribute
{}