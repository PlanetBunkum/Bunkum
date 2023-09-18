using System.Text;
using System.Xml.Serialization;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Newtonsoft.Json;

namespace BunkumTests.HttpServer.Endpoints;

public class BodyEndpoints : EndpointGroup
{
    [Endpoint("/body/string", Method.Post)]
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
    
    [Endpoint("/body/json", Method.Post, ContentType.Json)]
    public Serializable Json(RequestContext context, Serializable body)
    {
        return body;
    }
    
    [Endpoint("/body/xml", Method.Post, ContentType.Xml)]
    public Serializable Xml(RequestContext context, Serializable body)
    {
        return body;
    }
    
    [Endpoint("/body/byteArray", Method.Post)]
    public string ByteArray(RequestContext context, byte[] body)
    {
        return Encoding.Default.GetString(body);
    }
    
    [Endpoint("/body/stream", Method.Post)]
    public string Stream(RequestContext context, Stream body)
    {
        MemoryStream stream = (MemoryStream)body;
        return Encoding.Default.GetString(stream.GetBuffer());
    }
}