using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer.Storage;
using NotEnoughLogs;

namespace Bunkum.HttpServer;

[SuppressMessage("ReSharper", "NotAccessedField.Global")] // fields are used by application
public struct RequestContext
{
    public EndPoint RemoteEndpoint;
    public MemoryStream RequestStream;

    public Method Method;
    public Uri Url;
    
    public LoggerContainer<BunkumContext> Logger;
    public IDataStore DataStore;
    
    public NameValueCollection QueryString;
    public NameValueCollection Cookies;
}