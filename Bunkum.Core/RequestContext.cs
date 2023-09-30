using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Bunkum.Listener.Protocol;
using NotEnoughLogs;
using HttpVersion = Bunkum.Listener.Request.HttpVersion;

namespace Bunkum.Core;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")] // props are used by application
public struct RequestContext
{
    public EndPoint RemoteEndpoint { get; internal set; }
    public MemoryStream RequestStream { get; internal set; }

    public Enum Version { get; internal set; }
    public Method Method { get; internal set; }
    public Uri Url { get; internal set; }
    
    public Logger Logger { get; internal set; }

    public NameValueCollection QueryString { get; internal set; }
    public NameValueCollection Cookies { get; internal set; }
    
    public NameValueCollection RequestHeaders { get; internal set; }
    public Dictionary<string, string> ResponseHeaders { get; internal set; }
}