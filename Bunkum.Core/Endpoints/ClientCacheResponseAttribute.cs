using System.Diagnostics.CodeAnalysis;

namespace Bunkum.Core.Endpoints;

/// <summary>
/// Indicates that the client should cache the response for the given length.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[AttributeUsage(AttributeTargets.Method)]
public class ClientCacheResponseAttribute : Attribute
{
    internal long Seconds { get; }
    
    /// <summary>
    /// Indicates that the client should cache the response for 1 day.
    /// </summary>
    public ClientCacheResponseAttribute()
    {
        this.Seconds = 86400; // 1 day
    }
    
    /// <summary>
    /// Indicates that the client should cache the response for the given length.
    /// </summary>
    /// <param name="seconds">The number of seconds that the client should keep the response cached for</param>
    public ClientCacheResponseAttribute(long seconds)
    {
        this.Seconds = seconds;
    }
    
    /// <summary>
    /// Indicates that the client should cache the response for the given length.
    /// </summary>
    /// <param name="span">The time span that the client should keep the response cached for</param>
    public ClientCacheResponseAttribute(TimeSpan span)
    {
        this.Seconds = span.Ticks / TimeSpan.TicksPerSecond;
    }
}