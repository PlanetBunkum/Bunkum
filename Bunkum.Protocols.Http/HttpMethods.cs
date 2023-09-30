using Bunkum.Listener.Protocol;

namespace Bunkum.Protocols.Http;

/// <summary>
/// A collection of <see cref="Method"/>s for use with the HTTP protocol.
/// </summary>
public enum HttpMethods
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Get,
    Put,
    Post,
    Delete,
    Head,
    Options,
    Trace,
    Patch,
    Connect,
}