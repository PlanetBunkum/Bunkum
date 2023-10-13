using System.Diagnostics.CodeAnalysis;

namespace Bunkum.Listener.Protocol;

/// <summary>
/// A set of MIME content-type.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static class ContentType
{
    public const string Html = "text/html; charset=utf-8";
    public const string Plaintext = "text/plain";
    public const string Xml = "text/xml";
    public const string Json = "application/json";
    public const string BinaryData = "application/octet-stream";
    public const string Png = "image/png";
    public const string Jpeg = "image/jpeg";
}