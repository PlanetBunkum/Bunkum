using System.Diagnostics.CodeAnalysis;

namespace Bunkum.Core.Listener.Request.Http;

/// <summary>
/// The version of the connecting HTTP client.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum HttpVersion
{
    /// <summary>
    /// The protocol version is unknown, invalid, or unsupported.
    /// </summary>
    Unknown,
    /// <summary>
    /// HTTP/1.0
    /// </summary>
    Http1_0,
    /// <summary>
    /// HTTP/1.1
    /// </summary>
    Http1_1,
}