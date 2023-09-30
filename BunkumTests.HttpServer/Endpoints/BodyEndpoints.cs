using System.Text;
using System.Xml.Serialization;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Listener.Parsing;
using Newtonsoft.Json;
using HttpMethod = Bunkum.Core.Listener.Parsing.HttpMethod;

namespace BunkumTests.HttpServer.Endpoints;

public class BodyEndpoints : EndpointGroup
{
    [Endpoint("/body/string", HttpMethod.Post)]
    public string String(RequestContext context, string body)
    {
        return body;
    }

    [JsonObject(MemberSerialization.Fields)]
    public class Serializable
    {
        [XmlElement]
        public string Field = "";
    }
    
    [Endpoint("/body/json", HttpMethod.Post, ContentType.Json)]
    public Serializable Json(RequestContext context, Serializable body)
    {
        return body;
    }
    
    [Endpoint("/body/xml", HttpMethod.Post, ContentType.Xml)]
    public Serializable Xml(RequestContext context, Serializable body)
    {
        return body;
    }
    
    [Endpoint("/body/byteArray", HttpMethod.Post)]
    public string ByteArray(RequestContext context, byte[] body)
    {
        return Encoding.Default.GetString(body);
    }
    
    [Endpoint("/body/stream", HttpMethod.Post)]
    public string Stream(RequestContext context, Stream body)
    {
        MemoryStream stream = (MemoryStream)body;
        return Encoding.Default.GetString(stream.GetBuffer());
    }
}