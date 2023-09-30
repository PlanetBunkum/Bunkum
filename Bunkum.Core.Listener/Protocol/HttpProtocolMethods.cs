using Bunkum.Core.Listener.Protocol;

namespace Bunkum.Core.Listener.Parsing;

/// <summary>
/// A collection of <see cref="Method"/>s for use with the HTTP protocol.
/// </summary>
public static class HttpProtocolMethods
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static readonly Method Get = new("GET"); 
    public static readonly Method Put = new("PUT"); 
    public static readonly Method Post = new("POST"); 
    public static readonly Method Delete = new("DELETE"); 
    public static readonly Method Head = new("HEAD"); 
    public static readonly Method Options = new("OPTIONS"); 
    public static readonly Method Trace = new("TRACE"); 
    public static readonly Method Patch = new("PATCH"); 
    public static readonly Method Connect = new("CONNECT");
}