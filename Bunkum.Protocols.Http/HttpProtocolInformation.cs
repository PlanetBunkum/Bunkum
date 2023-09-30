using System.Diagnostics.CodeAnalysis;
using Bunkum.Listener.Protocol;

namespace Bunkum.Protocols.Http;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class HttpProtocolInformation
{
    public static readonly ProtocolInformation Http1_0 = new("HTTP", "1.0");
    public static readonly ProtocolInformation Http1_1 = new("HTTP", "1.1");
}