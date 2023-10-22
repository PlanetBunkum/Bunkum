using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;

namespace Bunkum.Protocols.Http;

public class HttpEndpointAttribute : EndpointAttribute
{
    public HttpEndpointAttribute(string route, HttpMethods method = HttpMethods.Get, string contentType = Bunkum.Listener.Protocol.ContentType.Plaintext) : base(route)
    {
        this.Method = MethodUtils.FromEnum(typeof(HttpProtocolMethods), method);
        this.ContentType = contentType;
    }
    
    public HttpEndpointAttribute(string route, string contentType)
        : this(route, HttpMethods.Get, contentType)
    {
        
    }
    
    public HttpEndpointAttribute(string route, string contentType, HttpMethods method)
        : this(route, method, contentType)
    {
        
    }

    /// <inherit-doc/>
    public override ProtocolInformation Protocol => HttpProtocolInformation.Http1_1;
}