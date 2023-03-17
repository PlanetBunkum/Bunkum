using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Bunkum.HttpServer.Endpoints;

/// <summary>
/// Indicates that the client should cache the response for the given length.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class ClientCacheResponseAttribute : Attribute
{
    internal long Seconds { get; }

    public ClientCacheResponseAttribute()
    {
        this.Seconds = 86400; // 1 day
    }

    public ClientCacheResponseAttribute(long seconds)
    {
        this.Seconds = seconds;
    }

    public ClientCacheResponseAttribute(TimeSpan span)
    {
        this.Seconds = span.Ticks / TimeSpan.TicksPerSecond;
    }
}