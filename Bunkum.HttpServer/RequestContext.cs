using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;
using HttpVersion = Bunkum.CustomHttpListener.Request.HttpVersion;

namespace Bunkum.HttpServer;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")] // props are used by application
public struct RequestContext
{
    public EndPoint RemoteEndpoint { get; internal set; }
    public MemoryStream RequestStream { get; internal set; }

    public HttpVersion Version { get; internal set; }
    public Method Method { get; internal set; }
    public Uri Url { get; internal set; }
    
    public Logger Logger { get; internal set; }

    public NameValueCollection QueryString { get; internal set; }
    public NameValueCollection Cookies { get; internal set; }
    
    public NameValueCollection RequestHeaders { get; internal set; }
    public Dictionary<string, string> ResponseHeaders { get; internal set; }

    private const string ObsoleteServiceMessage = $"This method of accessing services is deprecated. Use a `{nameof(EndpointService)}` instead.";

    [Obsolete(ObsoleteServiceMessage, true)]
    // ReSharper disable once UnusedMember.Global
    public IEnumerable<Service> Services => throw new Exception(ObsoleteServiceMessage);
}