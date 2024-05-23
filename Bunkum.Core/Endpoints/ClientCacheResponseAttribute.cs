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
    internal bool OnlyCacheSuccess { get; }
    
    /// <summary>
    /// Indicates that the client should cache the response for 1 day.
    /// </summary>
    /// <param name="onlyCacheSuccess">
    /// If true, only send the cache hint when the response is a 2XX status code.
    /// If false, always send the cache hint.
    /// </param>
    public ClientCacheResponseAttribute(bool onlyCacheSuccess = true) : this(86400, onlyCacheSuccess) // 1 day
    {}
    
    /// <summary>
    /// Indicates that the client should cache the response for the given length.
    /// </summary>
    /// <param name="seconds">The number of seconds that the client should keep the response cached for</param>
    /// <param name="onlyCacheSuccess">
    /// If true, only send the cache hint when the response is a 2XX status code.
    /// If false, always send the cache hint.
    /// </param>
    public ClientCacheResponseAttribute(long seconds, bool onlyCacheSuccess = true)
    {
        this.Seconds = seconds;
        this.OnlyCacheSuccess = onlyCacheSuccess;
    }
    
    /// <summary>
    /// Indicates that the client should cache the response for the given length.
    /// </summary>
    /// <param name="span">The time span that the client should keep the response cached for</param>
    /// <param name="onlyCacheSuccess">
    /// If true, only send the cache hint when the response is a 2XX status code.
    /// If false, always send the cache hint.
    /// </param>
    public ClientCacheResponseAttribute(TimeSpan span, bool onlyCacheSuccess = true)
    {
        this.Seconds = span.Ticks / TimeSpan.TicksPerSecond;
        this.OnlyCacheSuccess = onlyCacheSuccess;
    }
}