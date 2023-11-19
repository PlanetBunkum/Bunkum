using System.Text;
using System.Xml.Serialization;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Newtonsoft.Json;

namespace BunkumTests.HttpServer.Endpoints;

public class BodyEndpoints : EndpointGroup
{
    [HttpEndpoint("/body/string", HttpMethods.Post)]
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
    
    [HttpEndpoint("/body/json", HttpMethods.Post, ContentType.Json)]
    public Serializable Json(RequestContext context, Serializable body)
    {
        return body;
    }
    
    [HttpEndpoint("/body/xml", HttpMethods.Post, ContentType.Xml)]
    public Serializable Xml(RequestContext context, Serializable body)
    {
        return body;
    }
    
    [HttpEndpoint("/body/byteArray", HttpMethods.Post)]
    public string ByteArray(RequestContext context, byte[] body)
    {
        return Encoding.Default.GetString(body);
    }
    
    [HttpEndpoint("/body/stream", HttpMethods.Post)]
    public string Stream(RequestContext context, Stream body)
    {
        MemoryStream stream = (MemoryStream)body;
        return Encoding.Default.GetString(stream.ToArray());
    }
}