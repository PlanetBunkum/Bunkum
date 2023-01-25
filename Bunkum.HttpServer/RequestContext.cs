using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Bunkum.HttpServer.Storage;
using NotEnoughLogs;

namespace Bunkum.HttpServer;

[SuppressMessage("ReSharper", "NotAccessedField.Global")] // fields are used by application
public struct RequestContext
{
    public MemoryStream RequestStream;
    public NameValueCollection QueryString;
    public Uri Url;
    public LoggerContainer<BunkumContext> Logger;
    public IDataStore DataStore;
}