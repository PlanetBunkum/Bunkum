using System.Net;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using BunkumTests.HttpServer.Tests;

namespace BunkumTests.HttpServer.Endpoints;

public class ResponseEndpoints : EndpointGroup
{
    [HttpEndpoint("/response/string")]
    public string String(RequestContext context)
    {
        return "works";
    }
    
    [HttpEndpoint("/response/responseObject")]
    public Response ResponseObject(RequestContext context)
    {
        return new Response("works", ContentType.Plaintext);
    }
    
    [HttpEndpoint("/response/responseObjectWithCode")]
    public Response ResponseObjectWithCode(RequestContext context)
    {
        return new Response("works", ContentType.Plaintext, HttpStatusCode.Accepted); // random code lol
    }

    [HttpEndpoint("/response/serializedXml", HttpMethods.Get, ContentType.Xml)]
    [HttpEndpoint("/response/serializedJson", HttpMethods.Get, ContentType.Json)]
    public ResponseSerializationObject SerializedObject(RequestContext context)
    {
        return new ResponseSerializationObject();
    }
}