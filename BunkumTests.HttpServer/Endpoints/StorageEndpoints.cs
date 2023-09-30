using System.Net;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Core.Storage;

namespace BunkumTests.HttpServer.Endpoints;

public class StorageEndpoints : EndpointGroup
{
    [HttpEndpoint("/storage/put")]
    public Response Put(RequestContext context, IDataStore dataStore)
    {
        dataStore.WriteToStore("file", "data"u8);
        return HttpStatusCode.OK;
    }

    [HttpEndpoint("/storage/get")]
    public byte[] Get(RequestContext context, IDataStore dataStore) => dataStore.GetDataFromStore("file");
}