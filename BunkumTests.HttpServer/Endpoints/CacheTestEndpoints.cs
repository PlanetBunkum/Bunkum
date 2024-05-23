using System.Net;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;

namespace BunkumTests.HttpServer.Endpoints;

// ReSharper disable RedundantArgumentDefaultValue

public class CacheTestEndpoints : EndpointGroup
{
    [HttpEndpoint("/cache/onlySuccess/ok")]
    [ClientCacheResponse(60, true)]
    public Response GetOnlySuccessOk(RequestContext context)
    {
        return new Response("ok");
    }
    
    [HttpEndpoint("/cache/onlySuccess/fail")]
    [ClientCacheResponse(60, true)]
    public Response GetOnlySuccessFail(RequestContext context)
    {
        return new Response("fail", ContentType.Plaintext, HttpStatusCode.InternalServerError);
    }
    
    [HttpEndpoint("/cache/all/ok")]
    [ClientCacheResponse(60, false)]
    public Response GetAllOk(RequestContext context)
    {
        return new Response("ok");
    }
    
    [HttpEndpoint("/cache/all/fail")]
    [ClientCacheResponse(60, false)]
    public Response GetAllFail(RequestContext context)
    {
        return new Response("fail", ContentType.Plaintext, HttpStatusCode.InternalServerError);
    }
}