using Bunkum.Listener.Protocol;

namespace Bunkum.Core.Endpoints;

public class HttpEndpointAttribute : EndpointAttribute
{
    public HttpEndpointAttribute(string route, HttpMethods method = HttpMethods.Get, ContentType contentType = ContentType.Plaintext) : base(route)
    {
        this.Method = MethodUtils.FromEnum(typeof(HttpProtocolMethods), method);
        this.ContentType = contentType;
    }
    
    public HttpEndpointAttribute(string route, ContentType contentType)
        : this(route, HttpMethods.Get, contentType)
    {
        
    }
    
    public HttpEndpointAttribute(string route, ContentType contentType, HttpMethods method)
        : this(route, method, contentType)
    {
        
    }
}