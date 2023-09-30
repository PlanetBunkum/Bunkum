using System.Diagnostics.CodeAnalysis;

namespace Bunkum.Listener.Request;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum HttpVersion
{
    Unknown,
    Http1_0,
    Http1_1,
}