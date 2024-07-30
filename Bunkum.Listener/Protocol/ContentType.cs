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
    public const string Apng = "image/apng";
    public const string Jpeg = "image/jpeg";
    public const string Gif = "image/gif";
    public const string Bmp = "image/bmp";
    public const string Webp = "image/webp";

    public const string Webm = "video/webm";
    public const string Mp4 = "video/mp4";
    public const string Mpeg = "video/mpeg";
    public const string Flv = "video/x-flv";
    
    public const string Ogg = "audio/ogg";
}